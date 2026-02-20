using LASYS.Domain.OCR;

namespace LASYS.Application.Interfaces;

public interface ILabelRepository
{
    Task<LabelProduct?> GetByItemCodeAsync(string itemCode);
    Task<IEnumerable<LabelProduct>> GetActiveLabelsAsync();
    Task<bool> UpsertAsync(LabelProduct product);
    Task<bool> DeleteAsync(string itemCode);
    Task<bool> SetActiveStatusAsync(string itemCode, bool isActive);
}