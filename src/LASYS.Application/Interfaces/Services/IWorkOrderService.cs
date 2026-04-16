using LASYS.Application.Common.Enums;
using LASYS.Application.Common.Models;

namespace LASYS.Application.Interfaces.Services
{
    public interface IWorkOrderService
    {
        bool IsValidBarcode(string barcode);

        Gs1BarcodeData ParseGs1Barcode(string rawBarcode);

        Task<(BoxTypeSelection? Selection, string ErrorMessage)> BarcodeScan(string barcode);

        Task<(BoxTypeSelection? Selection, string ErrorMessage)> CheckScannedData(string barcode);

        PrintData BuildPrintData(string itemCode, string lotNo,
                                 string instructionCode, BoxType labelType,
                                 LabelPrintType labelStatus);

        PrintData MapSelectionToPrintData(BoxTypeSelection selection);
    }
}
