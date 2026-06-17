namespace LASYS.Application.Features.BarcodeValidation
{
    public class BarcodeValidationResult
    {
        public bool IsValid { get; }
        public string ErrorMessage { get; }
        public Dictionary<string, string> ApplicationIdentifiers { get; }

        public BarcodeValidationResult(
            bool isValid,
            string errorMessage,
            Dictionary<string, string> applicationIdentifiers)
        {
            IsValid = isValid;
            ErrorMessage = errorMessage;
            ApplicationIdentifiers = applicationIdentifiers ?? [];
        }

        public static BarcodeValidationResult Success(Dictionary<string, string> fields)
        {
            return new BarcodeValidationResult(
                true,
                string.Empty,
                fields);
        }

        public static BarcodeValidationResult Failure(string errorMessage)
        {
            return new BarcodeValidationResult(
                false,
                errorMessage,
                applicationIdentifiers: []);
        }
    }
}
