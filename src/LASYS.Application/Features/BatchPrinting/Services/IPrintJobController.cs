using LASYS.Application.Features.BatchPrinting.Models;
using LASYS.Application.Features.LabelInstructions.GetLabelInstructionContext;

namespace LASYS.Application.Features.BatchPrinting.Services
{
    public interface IPrintJobController
    {
        Guid CreateJob(string printerName, LabelPrintingContext context);
        PrintJobState GetJob(Guid jobId);
        void Pause(Guid jobId);
        void Resume(Guid jobId);
        void Stop(Guid jobId);
        void Complete(Guid jobId);
        void Reset(Guid jobId);
        void Printed(Guid jobId);
    }
}
