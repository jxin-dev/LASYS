using LASYS.Application.Common.Enums;
using LASYS.Application.Interfaces.Persistence.TableMappings;

namespace LASYS.Infrastructure.Persistence.TableMappings
{
    public sealed class PrintTableResolver : IPrintTableResolver
    {
        public string GetTableName(BoxType boxType) =>
            boxType switch
            {
                BoxType.CaseLabel => "prdprnt_case_labels_tcl",
                BoxType.UnitBox => "prdprnt_ub_labels_tcl",
                BoxType.AdditionalUnitBox => "prdprnt_aub_labels_tcl",
                BoxType.OuterUnitBox => "prdprnt_oub_labels_tcl",
                BoxType.CartonBox => "prdprnt_cb_labels_tcl",
                BoxType.AdditionalCartonBox => "prdprnt_acb_labels_tcl",
                BoxType.OuterCartonBox => "prdprnt_ocb_labels_tcl",
                _ => throw new ArgumentOutOfRangeException(nameof(boxType), boxType, "Unsupported BoxType")
            };
    }
}
