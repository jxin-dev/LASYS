namespace LASYS.DesktopApp.State.Printing
{
    public interface IPrintingState
    {
        PrintingStatus Status { get; }

        event Action<PrintingStatus> StatusChanged;
        void SetPrinting(PrintingStatus status);
    }
}
