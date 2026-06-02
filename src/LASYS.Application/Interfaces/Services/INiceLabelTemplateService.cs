namespace LASYS.Application.Interfaces.Services
{
    public interface INiceLabelTemplateService : IDisposable
    {
        string? TemplatePath { get; }
        void LoadTemplate(string templatePath);
        void CloseTemplate();
        void SetVariables(IReadOnlyDictionary<string, string> variables);
        void SetVariable(string variableName, string value);

        bool GeneratePreview(string outputDirectory, string fileName, int width = 800, int height = 600);
        bool GeneratePrn(string outputDirectory, string fileName, out string prnPath);
        bool IsTemplateLoaded { get; }
        IReadOnlyList<string> GetTemplateVariables();

    }
}
