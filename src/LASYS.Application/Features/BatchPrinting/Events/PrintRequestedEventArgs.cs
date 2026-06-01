using LASYS.Application.Features.LabelInstructions.GetLabelInstructionContext;

namespace LASYS.Application.Features.BatchPrinting.Events
{
    public sealed class PrintRequestedEventArgs : EventArgs
    {
        public LabelPrintingContext Context { get; }
        public int Quantity { get; set; }
        public PrintRequestedEventArgs(LabelPrintingContext context, int quantity)
        {
            Context = context;
            Quantity = quantity;
        }

    }

}
