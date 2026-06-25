namespace LASYS.Application.Events
{
    public class PrintLabelEventArgs : EventArgs
    {
        public string ItemCode { get; }
        public uint MasterRevision { get; }
        public string BoxType { get; }
        public string FilePath { get; }
        public PrintLabelEventArgs(string itemCode, uint masterRevision, string boxType, string filePath)
        {
            ItemCode = itemCode;
            MasterRevision = masterRevision;
            BoxType = boxType;
            FilePath = filePath;
        }
    }
}
