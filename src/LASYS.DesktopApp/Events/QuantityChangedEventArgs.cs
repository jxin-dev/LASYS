
namespace LASYS.DesktopApp.Events
{
    public sealed class QuantityChangedEventArgs : EventArgs
    {
        public int Quantity { get; }
        public Application.Common.Enums.BoxType? BoxType { get; }
        public QuantityChangedEventArgs(int quantity, Application.Common.Enums.BoxType? boxType)
        {
            Quantity = quantity;
            BoxType = boxType;
        }
    }
}
