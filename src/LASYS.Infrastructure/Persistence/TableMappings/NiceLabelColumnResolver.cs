using LASYS.Application.Common.Enums;
using LASYS.Application.Interfaces.Persistence.TableMappings;

namespace LASYS.Infrastructure.Persistence.TableMappings
{
    public sealed class NiceLabelColumnResolver : INiceLabelColumnResolver
    {
        public (string PathColumn, string FileColumn) GetColumnNames(BoxType boxType)
        {
            var prefix = boxType switch
            {
                BoxType.UnitBox => "UB",
                BoxType.AdditionalUnitBox => "AUB",
                BoxType.OuterUnitBox => "OUB",
                BoxType.CartonBox => "CB",
                BoxType.AdditionalCartonBox => "ACB",
                BoxType.OuterCartonBox => "OCB",
                _ => throw new ArgumentOutOfRangeException(nameof(boxType), boxType, "Unsupported BoxType")
            };

            return ($"{prefix}_LABEL_FILE_PATH", $"{prefix}_LABEL_FILE");
        }
    }
}
