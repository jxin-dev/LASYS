using LASYS.Application.Common.Enums;

namespace LASYS.Application.Events
{
    public class LabelEventArgs : EventArgs
    {
        public LabelStatus Status { get; }
        public string Description { get; }
        public LabelEventArgs(LabelStatus status, string? description = null)
        {
            Status = status;
            var info = GetStatusInfo(status);
            Description = description ?? info.Description;
        }

        private (string Message, string Description) GetStatusInfo(LabelStatus status) => status switch
        {
            LabelStatus.TemplateLoading => ("Loading label template.", "The system is loading the label template file."),
            LabelStatus.TemplateLoaded => ("Label template loaded.", "The label template was successfully loaded and is ready for data input."),
            LabelStatus.TemplateLoadFailed => ("Failed to load label template.", "The system could not load the label template file. Please verify the file path and ensure the template exists."),
            _ => ("Unknown label status.", "The system returned an unknown label status.")

        };

    }
}
