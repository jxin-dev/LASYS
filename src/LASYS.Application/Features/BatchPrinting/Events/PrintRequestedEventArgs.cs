using LASYS.Application.Features.LabelInstructions.GetLabelInstructionContext;

namespace LASYS.Application.Features.BatchPrinting.Events
{
    public sealed class PrintRequestedEventArgs : EventArgs
    {
        public Guid JobId{ get; }
        public int Quantity { get; set; }
        public PrintRequestedEventArgs(Guid jobId, int quantity)
        {
            JobId = jobId;
            Quantity = quantity;
        }

    }

}
