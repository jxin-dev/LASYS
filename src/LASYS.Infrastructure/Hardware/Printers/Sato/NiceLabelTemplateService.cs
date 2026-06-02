using System.Drawing.Printing;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Interop.LabelGalleryPlus3WR;
using LASYS.Application.Interfaces.Services;

namespace LASYS.Infrastructure.Hardware.Printers.Sato
{
    public sealed class NiceLabelTemplateService : INiceLabelTemplateService
    {
        private readonly object _sync = new();

        private LGApp? _app;
        private LGLabel? _label;
        private LGLabel Label => _label ?? throw new InvalidOperationException("NiceLabel template is not loaded.");
        public string? TemplatePath { get; private set; }
        public bool IsTemplateLoaded => _label != null;
        public void LoadTemplate(string templatePath)
        {
            if (string.IsNullOrWhiteSpace(templatePath))
                throw new ArgumentException("Template path is required.", nameof(templatePath));

            if (!File.Exists(templatePath))
                throw new FileNotFoundException("Label template not found.", templatePath);

            lock (_sync)
            {
                _app ??= new LGApp();

                CloseTemplate();

                try
                {
                    _label = _app.LabelOpenEx(templatePath)
                        ?? throw new InvalidOperationException("Failed to open NiceLabel template.");

                    TemplatePath = templatePath;
                }
                catch
                {
                    _label = null;
                    TemplatePath = null;
                    throw;
                }
            }
        }
        public void CloseTemplate()
        {
            lock (_sync)
            {
                if (_label != null)
                {
                    try
                    {
                        _label.Free();
                    }
                    catch { }

                    if (OperatingSystem.IsWindows())
                    {
                        try
                        {
                            Marshal.FinalReleaseComObject(_label);
                        }
                        catch { }
                    }

                    _label = null;
                }

                TemplatePath = null;
            }
        }
        public bool GeneratePreview(string outputDirectory, string fileName, int width = 800, int height = 600)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(outputDirectory))
                    Directory.CreateDirectory(outputDirectory);

                var outputImagePath = Path.Combine(outputDirectory, $"{fileName}.jpg");

                lock (_sync)
                {
                    return Label.GetLabelPreview(outputImagePath, width, height);
                }
            }
            catch
            {
                return false;
            }
        }

        public bool GeneratePrn(string outputDirectory, string fileName, out string prnPath)
        {
            prnPath = string.Empty;
            try
            {
                Directory.CreateDirectory(outputDirectory);
                prnPath = Path.Combine(outputDirectory, $"{fileName}.prn");

                lock (_sync)
                {
                    Label.PrinterPort = prnPath;

                    var success = Label.Print("1");

                    if (!success)
                        return false;
                }

                return File.Exists(prnPath);
            }
            catch
            {
                return false;
            }
        }

        public IReadOnlyList<string> GetTemplateVariables()
        {
            var variables = new List<string>();

            lock (_sync)
            {
                for (int i = 1; i <= Label.Variables.Count; i++)
                {
                    var variable = Label.Variables.Item(i);

                    if (variable != null)
                        variables.Add(variable.Name);
                }
            }

            return variables;

        }



        public void SetVariable(string variableName, string value)
        {
            lock (_sync)
            {
                var variable = Label.Variables.FindByName(variableName);

                if (variable == null)
                    return;

                variable.SetValue(value);
            }
        }

        public void SetVariables(IReadOnlyDictionary<string, string> variables)
        {
            lock (_sync)
            {
                foreach (var variable in variables)
                {
                    var v = Label.Variables.FindByName(variable.Key);
                    if (v != null)
                        v.SetValue(variable.Value);
                }
            }
        }
        public void Dispose()
        {
            CloseTemplate();

            if (_app != null)
            {
                try
                {
                    _app.Quit();
                    _app.Free();
                }
                catch { }

                if (OperatingSystem.IsWindows())
                {
                    try
                    {
                        Marshal.FinalReleaseComObject(_app);
                    }
                    catch { }
                }

                _app = null;
            }
        }
    }
}
