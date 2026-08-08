using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Hangfire.Jobs
{
    public class SendEmailJob
    {
        public async Task ExecuteAsync(int userId)
        {
            // send email
        }
    }

}
