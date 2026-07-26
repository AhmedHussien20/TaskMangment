using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using TaskMangment.Application.Common;
using TaskMangment.Application.Common.Notification;
using TaskMangment.Application.DTOs.ReportsDTO;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Domain.Entities;
using TaskMangment.Infrastructure;
using TaskMangment.Infrastructure.DataContext;
using TaskMangment.Infrastructure.PDF;

namespace TaskMangment.Hangfire.Jobs
{
    public class SendMonthlyEmployeeDiscountsJob
    {
        private readonly AppDbContext _db;
        private readonly IEmailService _emailService;
        private readonly IEmailTemplateRenderer _emailTemplateRenderer;
        private readonly IWhatsAppService _whatsAppService;
        private readonly IBlobStorageService _blobStorage;
        private readonly ILogger<SendMonthlyEmployeeDiscountsJob> _logger;

        public SendMonthlyEmployeeDiscountsJob(
            AppDbContext db,
            IEmailService emailService,
            IEmailTemplateRenderer emailTemplateRenderer,
            IWhatsAppService whatsAppService,
            IBlobStorageService blobStorage,
            ILogger<SendMonthlyEmployeeDiscountsJob> logger)
        {
            _db = db;
            _emailService = emailService;
            _emailTemplateRenderer = emailTemplateRenderer;
            _whatsAppService = whatsAppService;
            _blobStorage = blobStorage;
            _logger = logger;
        }

        public async Task ExecuteAsync()
        {
            var tz = TimeZoneHelper.GetSaudiArabia();
            var nowLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);

            var current25 = new DateTime(nowLocal.Year, nowLocal.Month, 25);
            var previous25 = current25.AddMonths(-1);
            // Period: previous 25 inclusive → current 25 exclusive
            // e.g. 25/06 → 24/07 inclusive (both ends for display)
            var fromDate = previous25.Date;
            var toDateExclusive = current25.Date;
            var toDate = toDateExclusive.AddDays(-1);

            _logger.LogInformation(
                "Starting monthly employee discounts job for period {FromDate} to {ToDate} (exclusive end {ToDateExclusive}). TimeZone={TimeZone}",
                fromDate,
                toDate,
                toDateExclusive,
                tz.Id);

            var discountRows = await _db.Discounts
                .AsNoTracking()
                .Where(d =>
                    !d.IsDeleted &&
                    d.Amount > 0 &&
                    d.Employee != null &&
                    d.Employee.IsActive &&
                    !d.Employee.IsDeleted &&
                    d.Employee.BranchId != null &&
                    d.ViolationDate >= fromDate &&
                    d.ViolationDate < toDateExclusive)
                .SelectMany(
                    d => d.Employee.EmployeeRoles.Where(er =>
                        er.IsAssigned &&
                        !er.IsDeleted &&
                        er.Role != null &&
                        !er.Role.IsDeleted),
                    (d, er) => new
                    {
                        BranchId = d.Employee.BranchId!.Value,
                        d.EmployeeId,
                        EmployeeName = d.Employee.FullName,
                        d.Amount,
                        RoleTitle = er.Role.Name
                    })
                .GroupBy(x => new
                {
                    x.BranchId,
                    x.EmployeeId,
                    x.EmployeeName,
                    x.RoleTitle
                })
                .Select(g => new BranchDiscountRowDto
                {
                    BranchId = g.Key.BranchId,
                    EmployeeName = g.Key.EmployeeName ?? "غير معروف",
                    RoleTitle = g.Key.RoleTitle ?? "-",
                    TotalDiscount = g.Sum(x => x.Amount)
                })
                .OrderBy(x => x.BranchId)
                .ThenBy(x => x.RoleTitle)
                .ThenByDescending(x => x.TotalDiscount)
                .ThenBy(x => x.EmployeeName)
                .ToListAsync();

            if (discountRows.Count == 0)
            {
                _logger.LogInformation("No employee discounts found for the period. Job completed.");
                return;
            }

            var accountants = await _db.Employees
                .AsNoTracking()
                .Where(e =>
                    e.IsActive &&
                    !e.IsDeleted &&
                    e.Id == 380 &&
                    e.BranchId != null &&
                    (
                        e.FunctionCode == FunctionCode.Accounting ||
                        e.EmployeeRoles.Any(er =>
                            er.IsAssigned &&
                            !er.IsDeleted &&
                            er.Role != null &&
                            !er.Role.IsDeleted &&
                            (er.Role.Level == (int)RoleLevelEnum.Accountant
                             || er.Role.Level == 50
                             || er.Role.Level == 60))
                    ))
                .Select(e => new AccountantRecipientDto
                {
                    Id = e.Id,
                    BranchId = e.BranchId!.Value,
                    FullName = e.FullName,
                    Email = e.Email,
                    Mobile = e.Mobile
                })
                .ToListAsync();

            _logger.LogInformation(
                "Found {DiscountBranches} branch discount group(s) and {AccountantCount} accountant recipient(s)",
                discountRows.Select(x => x.BranchId).Distinct().Count(),
                accountants.Count);

            var branchNames = await _db.Branches
                .AsNoTracking()
                .Where(b => !b.IsDeleted)
                .Select(b => new { b.Id, b.Name })
                .ToDictionaryAsync(b => b.Id, b => b.Name);

            var discountsByBranch = discountRows
                .GroupBy(x => x.BranchId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var accountantsByBranch = accountants
                .GroupBy(x => x.BranchId)
                .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var (branchId, rows) in discountsByBranch)
            {
                if (!accountantsByBranch.TryGetValue(branchId, out var branchAccountants) ||
                    branchAccountants.Count == 0)
                {
                    _logger.LogWarning(
                        "Skipping branch {BranchId}: discounts exist but no accountant recipients found.",
                        branchId);
                    continue;
                }

                try
                {
                    await SendBranchReportAsync(
                        branchId,
                        branchNames.GetValueOrDefault(branchId, "-"),
                        rows,
                        branchAccountants,
                        fromDate,
                        toDate);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send monthly discounts report for branch {BranchId}", branchId);
                }
            }
        }

        private async Task SendBranchReportAsync(
            int branchId,
            string branchName,
            List<BranchDiscountRowDto> rows,
            List<AccountantRecipientDto> recipients,
            DateTime fromDate,
            DateTime toDate)
        {
            var reportRows = rows.Select(r => new EmployeeTotalDiscountReportRowDto
            {
                EmployeeName = r.EmployeeName,
                RoleTitle = r.RoleTitle,
                TotalDiscount = r.TotalDiscount
            }).ToList();

            byte[]? pdfBytes = null;
            try
            {
                var pdfDocument = new MonthlyEmployeeTotalDiscountPdfDocument(reportRows, fromDate, toDate, branchName);
                pdfBytes = pdfDocument.GeneratePdf();
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "PDF generation failed for branch {BranchId}. Will send email without PDF attachment.",
                    branchId);
            }

            var fileName = $"employee-discounts-branch-{branchId}-{toDate:yyyyMMdd}.pdf";
            List<EmailAttachment>? emailAttachment = null;
            if (pdfBytes != null && pdfBytes.Length > 0)
            {
                emailAttachment = new List<EmailAttachment>
                {
                    new EmailAttachment
                    {
                        Name = fileName,
                        ContentBase64 = Convert.ToBase64String(pdfBytes)
                    }
                };
            }

            var whatsAppMessage = MonthlyEmployeeDiscountMessages.BuildWhatsAppMessage(branchName, fromDate, toDate);
            List<WhatsAppAttachment>? whatsAppAttachments = null;
            string? blobUrlWithoutSas = null;

            try
            {
                if (pdfBytes != null && pdfBytes.Length > 0)
                {
                    try
                    {
                        using var stream = new MemoryStream(pdfBytes);
                        blobUrlWithoutSas = await _blobStorage.UploadAsync(
                            stream,
                            fileName,
                            "application/pdf",
                            "monthly-employee-discounts");

                        whatsAppAttachments = new List<WhatsAppAttachment>
                        {
                            new WhatsAppAttachment
                            {
                                FileName = fileName,
                                Url = _blobStorage.WithSas(blobUrlWithoutSas),
                                ContentType = "application/pdf"
                            }
                        };
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(
                            ex,
                            "Blob upload failed for branch {BranchId}. Email will still send; WhatsApp may send without file.",
                            branchId);
                    }
                }

                foreach (var recipient in recipients)
                {
                    if (!string.IsNullOrWhiteSpace(recipient.Email))
                    {
                        try
                        {
                            var tokens = MonthlyEmployeeDiscountMessages.BuildEmailTokens(
                                recipient.FullName,
                                branchName,
                                fromDate,
                                toDate);

                            var rendered = await _emailTemplateRenderer.RenderWithTokensAsync(
                                MonthlyEmployeeDiscountMessages.EmailTemplateKey,
                                tokens);

                            await _emailService.SendEmailAsync(
                                recipient.Email,
                                rendered.Subject,
                                rendered.Body,
                                emailAttachment);

                            _logger.LogInformation(
                                "Sent monthly discounts email to {Email} for branch {BranchId} (hasPdf={HasPdf})",
                                recipient.Email,
                                branchId,
                                emailAttachment != null);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(
                                ex,
                                "Failed to send monthly discounts email to {Email} for branch {BranchId}",
                                recipient.Email,
                                branchId);
                        }
                    }
                    else
                    {
                        _logger.LogWarning(
                            "Accountant {EmployeeId} for branch {BranchId} has no email",
                            recipient.Id,
                            branchId);
                    }

                    if (!string.IsNullOrWhiteSpace(recipient.Mobile))
                    {
                        try
                        {
                            var maxRoleLevel = await GetEmployeeMaxRoleLevelAsync(recipient.Id);
                            if (maxRoleLevel < (int)RoleLevelEnum.Manager)
                            {
                                _logger.LogInformation(
                                    "Skipping monthly discounts WhatsApp for employee {EmployeeId} on branch {BranchId}: role level {RoleLevel} is below Manager",
                                    recipient.Id,
                                    branchId,
                                    maxRoleLevel);
                            }
                            else
                            {
                                await _whatsAppService.SendNotificationAsync(
                                    recipient.Mobile,
                                    recipient.FullName,
                                    whatsAppMessage,
                                    whatsAppAttachments);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(
                                ex,
                                "Failed to send monthly discounts WhatsApp to {Mobile} for branch {BranchId}",
                                recipient.Mobile,
                                branchId);
                        }
                    }
                }
            }
            finally
            {
                if (!string.IsNullOrWhiteSpace(blobUrlWithoutSas))
                {
                    try
                    {
                        await _blobStorage.DeleteAsync(blobUrlWithoutSas);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(
                            ex,
                            "Failed to delete temporary blob for branch {BranchId}",
                            branchId);
                    }
                }
            }
        }

        private async Task<int> GetEmployeeMaxRoleLevelAsync(int employeeId)
        {
            var maxLevel = await _db.Employees
                .AsNoTracking()
                .Where(e => e.Id == employeeId)
                .SelectMany(e => e.EmployeeRoles)
                .Where(er => er.IsAssigned && !er.IsDeleted && er.Role != null)
                .Select(er => (int?)er.Role.Level)
                .MaxAsync();

            return maxLevel ?? (int)RoleLevelEnum.Employee;
        }
    }
}
