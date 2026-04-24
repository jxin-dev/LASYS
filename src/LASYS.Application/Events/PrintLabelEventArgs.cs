namespace LASYS.Application.Events
{
    public class PrintLabelEventArgs : EventArgs
    {
        public string ItemCode { get; }
        public int RevisionNumber { get; }
        public string BoxType { get; }
        public string FilePath { get; }
        public PrintLabelEventArgs(string itemCode, int revisionNumber, string boxType, string filePath)
        {
            ItemCode = itemCode;
            RevisionNumber = revisionNumber;
            BoxType = boxType;
            FilePath = filePath;
        }
    }
}
