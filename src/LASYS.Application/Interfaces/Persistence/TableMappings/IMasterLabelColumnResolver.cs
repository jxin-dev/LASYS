using LASYS.Application.Common.Enums;

namespace LASYS.Application.Interfaces.Persistence.TableMappings
{
    public interface IMasterLabelColumnResolver
    {

        (string WidthColumn,
        string HeightColumn,
        string FileColumn,
        string ImageFileColumn,
        string PathColumn,
        string MaterialCodeColumn,
        string MaterialDescriptionColumn,
        string MaterialConsumeQuantityColumn,
        string OcrSupportedColumn) 
            GetColumns(BoxType boxType);
    }
}
