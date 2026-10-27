using TaskMangment.Application.Interfaces.Services;

public class HangfireCurrentUserService : ICurrentUserService
{
    public int? UserId => null;
    public string? UserName => "Hangfire";
    public bool IsAuthenticated => false;

    // Hangfire has no JWT. Schema/company for jobs come from OperationalSchemaAmbient /
    // OperationalCompanyAmbient, which BlobStorageService and DbContext resolve separately.
    public int? CompanyId => null;
    public string? Schema => null;
}
