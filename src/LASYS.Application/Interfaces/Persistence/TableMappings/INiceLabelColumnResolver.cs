using LASYS.Application.Common.Enums;

namespace LASYS.Application.Interfaces.Persistence.TableMappings
{
    public interface INiceLabelColumnResolver
    {
        (string PathColumn, string FileColumn) GetColumnNames(BoxType boxType);
    }
}
