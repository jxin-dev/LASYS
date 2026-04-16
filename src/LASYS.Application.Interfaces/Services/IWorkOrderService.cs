using LASYS.Application.Common.Enums;
using LASYS.Application.Common.Models;
using System.Threading.Tasks;

namespace LASYS.Application.Interfaces.Services
{
    public interface IWorkOrderService
    {
        bool IsValidBarcode(string barcode);

        LASYS.Application.Common.Models.Gs1BarcodeData ParseGs1Barcode(string rawBarcode);

        System.Threading.Tasks.Task<(LASYS.Application.Common.Models.BoxTypeSelection? Selection, string ErrorMessage)> BarcodeScan(string barcode);

        System.Threading.Tasks.Task<(LASYS.Application.Common.Models.BoxTypeSelection? Selection, string ErrorMessage)> CheckScannedData(string barcode);

        LASYS.Application.Common.Models.PrintData BuildPrintData(string itemCode, string lotNo,
                                string instructionCode, LASYS.Application.Common.Enums.BoxType labelType,
                                LASYS.Application.Common.Enums.LabelPrintType labelStatus);

        LASYS.Application.Common.Models.PrintData MapSelectionToPrintData(LASYS.Application.Common.Models.BoxTypeSelection selection);

        // WorkOrderService methods
        Task<string> GetWorkOrderDetails(string workOrderId);
        
        Task<bool> UpdateWorkOrderStatus(string workOrderId, string status);

        Task<string> CreateWorkOrder(string itemCode, string lotNo, string instructionCode, BoxType labelType);
        
        Task<bool> DeleteWorkOrder(string workOrderId);
    }
}
