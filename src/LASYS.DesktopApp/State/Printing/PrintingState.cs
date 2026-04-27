
using System.Diagnostics;

namespace LASYS.DesktopApp.State.Printing
{
    public class PrintingState : IPrintingState
    {
        public PrintingStatus Status { get; private set; } = PrintingStatus.Idle;

        public event Action<PrintingStatus>? StatusChanged;

        public void SetPrinting(PrintingStatus status)
        {
            if (!IsValidTransition(Status, status))
            {
                Debug.WriteLine($"Invalid transition from {Status} to {status}.");
                return;
            }

            Status = status;
            StatusChanged?.Invoke(status);
        }

        /// <summary>
        /// Validates if the transition from the current printing status to the next status is allowed.
        /// Prevents invalid state jumps and enforces a proper printing workflow.
        /// </summary>
        private bool IsValidTransition(PrintingStatus current, PrintingStatus next)
        {
            return (current, next) switch
            {
                (PrintingStatus.Idle, PrintingStatus.Printing) => true,
                (PrintingStatus.Printing, PrintingStatus.Paused) => true,
                (PrintingStatus.Paused, PrintingStatus.Printing) => true,
                (PrintingStatus.Printing, PrintingStatus.Stopping) => true,
                (PrintingStatus.Paused, PrintingStatus.Stopping) => true,
                (PrintingStatus.Stopping, PrintingStatus.Completed) => true,
                (PrintingStatus.Printing, PrintingStatus.Completed) => true,
                (_, PrintingStatus.Error) => true, // always allow error
                // All other transitions are invalid
                _ => false
            };
        }
    }
}
