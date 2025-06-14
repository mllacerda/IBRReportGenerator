using ReportGenerator.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ReportGenerator.Domain.Interfaces
{
    public interface IMessageQueueService
    {
        Task PublishReportRequestAsync(ReportRequest request);
    }
}
