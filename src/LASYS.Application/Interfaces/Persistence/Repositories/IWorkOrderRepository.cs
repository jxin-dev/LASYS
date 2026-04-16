using LASYS.Domain.Instruction;
using LASYS.Domain.Product;
using MySqlConnector;
using System.Data;

namespace LASYS.Application.Interfaces.Persistence.Repositories
{
    public interface IWorkOrderRepository
    {
        Task<bool> CheckExistsAsync(string sql, params MySqlParameter[] parameters);
        Task<string> GetItemStringAsync(string sql, params MySqlParameter[] parameters);
        Task<DataTable> GetListAsync(string sql, params MySqlParameter[] parameters);
        Task<IEnumerable<T>> GetListAsync<T>(string sql, params MySqlParameter[] parameters);

        Task<bool> CheckInstructionBarcodeExistsAsync(string barcode);
        Task<string> GetItemCodeByBarcodeAsync(string barcode);

        Task<bool> CheckIsScrappedAsync(string statusColumn, string itemCode, string lotNo);
        Task<bool> CheckIsPairedAsync(string itemCode, string lotNo);

        Task<string> GetUpdatedUbBarcodeAsync(string itemCode, string lotNo);
        Task<int> GetLatestRevisionAsync(string itemCode, string lotNo);

        Task<IEnumerable<ProductMaster>> GetProductDataAsync(string itemCode, int revision);

        Task<InstructionPrintSnapshot> GetInstructionPrintSnapshotAsync(string instructionColumn,
                                                         string targetColumn,
                                                         string printTable,
                                                         string itemCode,
                                                         string lotNo,
                                                         string instructionCode);

        Task<long> GetMaxSequenceNumberAsync(string printTable, string itemCode, string lotNo);

        Task<bool> CheckPrintRecordExistsAsync(string tableName, string itemCode, string lotNo);

        Task<int> SavePrintRecordAsync(string printTable, DataRow row,
                                       string approvedUser, int approvedSection,
                                       string ipAddress);
    }
}
