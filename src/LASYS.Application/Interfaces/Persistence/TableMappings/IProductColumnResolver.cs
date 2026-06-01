using LASYS.Application.Common.Enums;

namespace LASYS.Application.Interfaces.Persistence.TableMappings
{
    public interface IProductColumnResolver
    {
        (string ReportTypeCode, string BarcodeNumber, string Quantity) GetColumnNames(BoxType boxType);
    }
}
