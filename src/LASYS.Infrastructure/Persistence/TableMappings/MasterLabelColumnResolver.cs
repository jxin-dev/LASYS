using LASYS.Application.Common.Enums;
using LASYS.Application.Interfaces.Persistence.TableMappings;

namespace LASYS.Infrastructure.Persistence.TableMappings
{
    public sealed class MasterLabelColumnResolver : IMasterLabelColumnResolver
    {
        public (string WidthColumn, string HeightColumn, string FileColumn, string ImageFileColumn, string PathColumn, string MaterialCodeColumn, string MaterialDescriptionColumn, string MaterialConsumeQuantityColumn, string OcrSupportedColumn) GetColumns(BoxType boxType)
        {
            var prefix = boxType switch
            {
                BoxType.UnitBox => "UB",
                BoxType.AdditionalUnitBox => "AUB",
                BoxType.OuterUnitBox => "OUB",
                BoxType.CartonBox => "CB",
                BoxType.AdditionalCartonBox => "ACB",
                BoxType.OuterCartonBox => "OCB",
                BoxType.CaseLabel => "CASE",
                _ => throw new ArgumentOutOfRangeException(nameof(boxType), boxType, "Unsupported BoxType")
            };
            return 
                (
                    WidthColumn: $"{prefix}_LABEL_FILE_WIDTH",
                    HeightColumn: $"{prefix}_LABEL_FILE_HEIGHT",

                    FileColumn: $"{prefix}_LABEL_FILE",
                    ImageFileColumn: $"{prefix}_LABEL_IMAGE_FILE",
                    PathColumn: $"{prefix}_LABEL_FILE_PATH",

                    MaterialCodeColumn: $"{prefix}_LABEL_MAT_CODE",
                    MaterialDescriptionColumn: $"{prefix}_LABEL_MAT_DESC",
                    MaterialConsumeQuantityColumn: $"{prefix}_LABEL_MAT_CONSUME_QTY",

                    OcrSupportedColumn: $"IS_{prefix}_OCR_SUPPORTED"
                );
        }
    }
}
