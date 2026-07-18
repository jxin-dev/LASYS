namespace LASYS.Application.Features.BatchPrinting.Events
{
    public sealed class PrintRequestedEventArgs : EventArgs
    {
        public Guid JobId{ get; }
        public int Quantity { get; }
        public bool EndOfBatch { get; }
        public PrintRequestedEventArgs(Guid jobId, int quantity, bool endOfBatch)
        {
            JobId = jobId;
            Quantity = quantity;
            EndOfBatch = endOfBatch;
        }

    }

}
