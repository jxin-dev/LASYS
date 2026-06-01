namespace LASYS.Application.Common.Models
{
    public sealed class NiceLabelVariableCollection
    {
        private readonly Dictionary<string, string> _variables;
        public IReadOnlyDictionary<string, string> Variables => _variables;
        public NiceLabelVariableCollection()
        {
            _variables = new();
        }
        private NiceLabelVariableCollection(Dictionary<string, string> variables)
        {
            _variables = variables;
        }
        public NiceLabelVariableCollection Add(string variableName, object? value)
        {
            _variables[variableName] = value?.ToString() ?? string.Empty;
            return this;
        }
        public NiceLabelVariableCollection Clone()
        {
            return new NiceLabelVariableCollection(new Dictionary<string, string>(_variables));
        }
    }
}
