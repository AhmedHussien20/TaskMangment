using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Domain.Entities
{
    public enum TaskPriority : byte { Low = 1, Medium = 2, High = 3 }
    public enum TaskStatus : byte { New = 1, InProgress = 2, Closed = 3, Archived = 4 }
    public enum ExtensionRequestStatus : byte { Pending = 1, Approved = 2, Rejected = 3 }
    public enum CloseRequestStatus : byte { Pending = 1, Approved = 2, Rejected = 3 }

}
