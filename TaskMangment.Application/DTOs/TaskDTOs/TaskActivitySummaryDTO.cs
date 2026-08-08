using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Application.DTOs.TaskDTOs
{
    public class TaskActivitySummaryDTO
    {
        public int TaskId { get; set; }

        public int CommentsCount { get; set; }
        public TaskCommentGetDto? LastComment { get; set; }

        public int WarningsCount { get; set; }
        public WarningGetDto? LastWarning { get; set; }

        public int PenaltysCount { get; set; }
        public DiscountGetDto? LastPenalty { get; set; }


        public int ExtensionRequestsCount { get; set; }
        public TaskExtensionRequestDetailsDto? LastExtensionRequest { get; set; }

        public int CloseRequestsCount { get; set; }
        public TaskCloseRequestDetailsDto? LastCloseRequest { get; set; }


        public int PercentageCount { get; set; }
        public TaskPercentageGetDto? LastPercentage { get; set; }

    }

}
