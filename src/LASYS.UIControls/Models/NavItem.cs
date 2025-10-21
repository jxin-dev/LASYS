namespace LASYS.UIControls.Models
{
    public class NavItem
    {
        public string Text { get; set; } = string.Empty;
        public event EventHandler? Clicked;
        public List<NavItem> SubItems { get; set; } = new();
        internal void RaiseClicked() => Clicked?.Invoke(this, EventArgs.Empty);
    }
}
