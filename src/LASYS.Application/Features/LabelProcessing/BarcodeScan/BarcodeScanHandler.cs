using Dapper;
using LASYS.Application.Common.Results;
using LASYS.Application.Interfaces.Persistence;
using MediatR;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LASYS.Application.Features.LabelProcessing.BarcodeScan
{
    public class BarcodeScanHandler : IRequestHandler<BarcodeScanQuery, Result<BarcodeScanResult>>
    {
        private readonly IDbConnectionFactory _factory;

        public BarcodeScanHandler(IDbConnectionFactory factory)
        {
            _factory = factory;
        }

        public async Task<Result<BarcodeScanResult>> Handle(BarcodeScanQuery request, CancellationToken cancellationToken)
        {
            try
            {
                string barcodeData = request.BarcodeData;
                string checkBarcode = string.Empty;

                // Check if the barcode exists in the database and retrieve relevant information
                string sql = $"SELECT COUNT(*) FROM ppt_lbl_instructn_plns_hst WHERE CB_LBL_INS_CODE = '{{barcodeData}}' OR UB_LBL_INS_CODE = '{{barcodeData}}' AND (CB_LBL_INS_STATUS IN ('For Printing','Printed') OR UB_LBL_INS_STATUS IN ('For Printing','Printed')) ";

                var conn = await _factory.CreateConnectionAsync();
                bool exists = await conn.ExecuteScalarAsync<int>(sql) > 0;

                if (exists)
                {
                    var gs1_128Instruction = await DecodeGS1_128(barcodeData);
                    string lotNoLabel = gs1_128Instruction.LotNo + gs1_128Instruction.AssemblyLineSuffix;
                    bool isScrapped = false;

                    string? itemCodeLabel = await conn.QueryFirstOrDefaultAsync<string>($@" 
                        SELECT ITEM_CODE 
                        FROM ppt_lbl_instructn_plns_hst
                        WHERE (CB_LBL_INS_CODE = '{barcodeData}' 
                        OR UB_LBL_INS_CODE = '{barcodeData}' 
                        OR AUB_LBL_INS_CODE = '{barcodeData}' 
                        OR ACB_LBL_INS_CODE = '{barcodeData}' 
                        OR OUB_LBL_INS_CODE = '{barcodeData}' 
                        OR OCB_LBL_INS_CODE = '{barcodeData}') 
                        AND (CB_LBL_INS_STATUS IN (2,3,4) 
                        OR UB_LBL_INS_STATUS IN (2,3,4) 
                        OR AUB_LBL_INS_STATUS IN (2,3,4) 
                        OR ACB_LBL_INS_STATUS IN (2,3,4) 
                        OR OUB_LBL_INS_STATUS IN (2,3,4) 
                        OR OCB_LBL_INS_STATUS IN (2,3,4))
                    ");

                    bool IsCaseExists = await conn.ExecuteScalarAsync<int>(@"
                        SELECT COUNT(*) FROM prdprnt_case_labels_tcl 
                        WHERE ITEM_CODE = '@ITEM_CODE' AND LOT_NO = '@LOT_NO'
                    ", new { ITEM_CODE = itemCodeLabel, lotNoLabel }) > 0;

                    bool IsUbExists = await conn.ExecuteScalarAsync<int>(@"
                        SELECT COUNT(*) FROM prdprnt_ub_labels_tcl 
                        WHERE ITEM_CODE = '@ITEM_CODE' AND LOT_NO = '@LOT_NO'
                    ", new { ITEM_CODE = itemCodeLabel, lotNoLabel }) > 0;

                    bool IsOubExists = await conn.ExecuteScalarAsync<int>(@"
                        SELECT COUNT(*) FROM prdprnt_oub_labels_tcl 
                        WHERE ITEM_CODE = '@ITEM_CODE' AND LOT_NO = '@LOT_NO'
                    ", new { ITEM_CODE = itemCodeLabel, lotNoLabel }) > 0;

                    bool IsOcbExists = await conn.ExecuteScalarAsync<int>(@"
                        SELECT COUNT(*) FROM prdprnt_ocb_labels_tcl 
                        WHERE ITEM_CODE = '@ITEM_CODE' AND LOT_NO = '@LOT_NO'
                    ", new { ITEM_CODE = itemCodeLabel, lotNoLabel }) > 0;

                    bool IsCbExists = await conn.ExecuteScalarAsync<int>(@"
                        SELECT COUNT(*) FROM prdprnt_cb_labels_tcl 
                        WHERE ITEM_CODE = '@ITEM_CODE' AND LOT_NO = '@LOT_NO'
                    ", new { ITEM_CODE = itemCodeLabel, lotNoLabel }) > 0;

                    bool IsAubExists = await conn.ExecuteScalarAsync<int>(@"
                        SELECT COUNT(*) FROM prdprnt_aub_labels_tcl 
                        WHERE ITEM_CODE = '@ITEM_CODE' AND LOT_NO = '@LOT_NO'
                    ", new { ITEM_CODE = itemCodeLabel, lotNoLabel }) > 0;

                    bool IsAcbExists = await conn.ExecuteScalarAsync<int>(@"
                        SELECT COUNT(*) FROM prdprnt_acb_labels_tcl 
                        WHERE ITEM_CODE = '@ITEM_CODE' AND LOT_NO = '@LOT_NO'
                    ", new { ITEM_CODE = itemCodeLabel, lotNoLabel }) > 0;

                    if(gs1_128Instruction.Packaging == 3 || gs1_128Instruction.Packaging == 2)
                    {
                        checkBarcode = await conn.QueryFirstAsync<string>(@"
                            SELECT UB_LBL_INS_CODE 
                            FROM ppt_lbl_instructn_plns_hst
                            WHERE HISTORY_DATETIME = (SELECT MAX(HISTORY_DATETIME) 
                            FROM PPT_LBL_INSTRUCTN_PLNS_HST WHERE ITEM_CODE = '@ITEM_CODE' 
                            AND LOT_NO = '@LOT_NOT') AND ITEM_CODE = '@ITEM_CODE' AND LOT_NO = '@LOT_NOT'
                        ", new { ITEM_CODE = itemCodeLabel, LOT_NO = lotNoLabel });
                        isScrapped = await conn.ExecuteScalarAsync<int>(@"
                            SELECT count(*) FROM ppt_lbl_instructn_plns_hst   
                            where UB_LBL_INS_STATUS = 4 AND ITEM_CODE = '@ITEM_CODE' AND LOT_NO = '@LOT_NO'
                        ", new { ITEM_CODE = itemCodeLabel, LOT_NO = lotNoLabel }) > 0;
                    }
                    else if(gs1_128Instruction.Packaging == 6)
                    {
                        checkBarcode = await conn.QueryFirstAsync<string>(@"
                            SELECT OUB_LBL_INS_CODE 
                            FROM PPT_LBL_INSTRUCTN_PLNS_HST 
                            WHERE HISTORY_DATETIME = (SELECT MAX(HISTORY_DATETIME) FROM PPT_LBL_INSTRUCTN_PLNS_HST
                            WHERE ITEM_CODE = '@ITEM_CODE' AND LOT_NO = '@LOT_NO') AND ITEM_CODE = '@ITEM_CODE' AND LOT_NO = '@LOT_NO'
                        ", new { ITEM_CODE = itemCodeLabel, LOT_NO = lotNoLabel });
                        isScrapped = await conn.ExecuteScalarAsync<int>(@"
                            SELECT count(*) FROM ppt_lbl_instructn_plns_hst   
                            where OUB_LBL_INS_STATUS = 4 AND ITEM_CODE = '@ITEM_CODE' AND LOT_NO = '@LOT_NO'
                        ", new { ITEM_CODE = itemCodeLabel, LOT_NO = lotNoLabel }) > 0;
                    }
                    else if(gs1_128Instruction.Packaging == 7)
                    {
                        checkBarcode = await conn.QueryFirstAsync<string>(@"
                            SELECT OCB_LBL_INS_CODE 
                            FROM PPT_LBL_INSTRUCTN_PLNS_HST WHERE HISTORY_DATETIME = (
                            SELECT MAX(HISTORY_DATETIME) FROM PPT_LBL_INSTRUCTN_PLNS_HST 
                            WHERE ITEM_CODE = '@ITEM_CODE' AND LOT_NO = '@LOT_NO') 
                            AND ITEM_CODE = '@ITEM_CODE' AND LOT_NO = '@LOT_NO'
                        ", new { ITEM_CODE = itemCodeLabel, LOT_NO = lotNoLabel });
                        isScrapped = await conn.ExecuteScalarAsync<int>(@"
                            SELECT count(*) FROM ppt_lbl_instructn_plns_hst
                            where OCB_LBL_INS_STATUS = 4 AND ITEM_CODE = '@ITEM_CODE' AND LOT_NO = '@LOT_NO'
                        ", new { ITEM_CODE = itemCodeLabel, LOT_NO = lotNoLabel }) > 0;
                    }
                    else if(gs1_128Instruction.Packaging == 8)
                    {
                        checkBarcode = await conn.QueryFirstAsync<string>(@"
                            SELECT AUB_LBL_INS_CODE
                            FROM PPT_LBL_INSTRUCTN_PLNS_HST 
                            WHERE HISTORY_DATETIME = (SELECT MAX(HISTORY_DATETIME) FROM PPT_LBL_INSTRUCTN_PLNS_HST
                            WHERE ITEM_CODE = '@ITEM_CODE' AND LOT_NO = '@LOT_NO') AND ITEM_CODE = '@ITEM_CODE' AND LOT_NO = '@LOT_NO'
                        ", new { ITEM_CODE = itemCodeLabel, LOT_NO = lotNoLabel });
                        isScrapped = await conn.ExecuteScalarAsync<int>(@"
                            SELECT count(*) FROM ppt_lbl_instructn_plns_hst
                            where AUB_LBL_INS_STATUS = 4 AND ITEM_CODE = '@ITEM_CODE' AND LOT_NO = '@LOT_NO'
                        ", new { ITEM_CODE = itemCodeLabel, LOT_NO = lotNoLabel }) > 0;
                    }
                    else if(gs1_128Instruction.Packaging == 9)
                    {
                        checkBarcode = await conn.QueryFirstAsync<string>(@"
                            SELECT ACB_LBL_INS_CODE 
                            FROM PPT_LBL_INSTRUCTN_PLNS_HST WHERE HISTORY_DATETIME = 
                            (SELECT MAX(HISTORY_DATETIME) FROM PPT_LBL_INSTRUCTN_PLNS_HST
                            WHERE ITEM_CODE = '@ITEM_CODE' AND LOT_NO = '@LOT_NO') AND ITEM_CODE = '@ITEM_CODE' AND LOT_NO = '@LOT_NO' 
                        ", new { ITEM_CODE = itemCodeLabel, LOT_NO = lotNoLabel });
                        isScrapped = await conn.ExecuteScalarAsync<int>(@"
                            SELECT count(*) FROM ppt_lbl_instructn_plns_hst
                            where ACB_LBL_INS_STATUS = 4 AND ITEM_CODE = '@ITEM_CODE' AND LOT_NO = '@LOT_NO'
                        ", new { ITEM_CODE = itemCodeLabel, LOT_NO = lotNoLabel }) > 0;
                    }
                    else
                    {
                        checkBarcode = await conn.QueryFirstAsync<string>(@"
                            SELECT CB_LBL_INS_CODE             
                            FROM PPT_LBL_INSTRUCTN_PLNS_HST WHERE HISTORY_DATETIME =
                            (SELECT MAX(HISTORY_DATETIME) FROM PPT_LBL_INSTRUCTN_PLNS_HST 
                            WHERE ITEM_CODE = '@ITEM_CODE' AND LOT_NO = '@LOT_NO') AND ITEM_CODE = '@ITEM_CODE' AND LOT_NO = '@LOT_NO' 
                        ", new { ITEM_CODE = itemCodeLabel, LOT_NO = lotNoLabel });
                        isScrapped = await conn.ExecuteScalarAsync<int>(@"
                            SELECT count(*) FROM ppt_lbl_instructn_plns_hst
                            where CB_LBL_INS_STATUS = 4 AND ITEM_CODE = '@ITEM_CODE' AND LOT_NO = '@LOT_NO'
                        ", new { ITEM_CODE = itemCodeLabel, LOT_NO = lotNoLabel }) > 0;
                    }

                    bool isPaired = await conn.ExecuteScalarAsync<int>(@"
                        SELECT COUNT(*) FROM 
                        PRE_TPC_PRODUCTS_TCL p 
                        WHERE ITEM_CODE = '@ITEM_CODE' and PAIRED_CB_BOX_TYPE = '' and masterlabel_revision_number =
                        (Select MASTER_LABEL_REVISION_NUMBER from ppt_lbl_instructn_plns_hst i 
                        where i.item_code = p.item_code and i.lot_no = '@LOT_NO' 
                        and i.history_datetime = (Select max(history_datetime) from ppt_lbl_instructn_plns_hst l 
                        where l.item_code = i.item_code and l.lot_no = i.lot_no))
                    ", new { ITEM_CODE = itemCodeLabel, LOT_NO = lotNoLabel }) > 0;

                    bool IsUbPrinted = await conn.ExecuteScalarAsync<int>(@"
                        SELECT COUNT(*) FROM prdprnt_ub_labels_tcl 
                        WHERE ITEM_CODE = '@ITEM_CODE' AND LOT_NO = '@LOT_NO'
                    ", new { ITEM_CODE = itemCodeLabel, lotNoLabel }) > 0;

                    bool IsOubPrinted = await conn.ExecuteScalarAsync<int>(@"
                        SELECT COUNT(*) FROM prdprnt_oub_labels_tcl 
                        WHERE ITEM_CODE = '@ITEM_CODE' AND LOT_NO = '@LOT_NO'
                    ", new { ITEM_CODE = itemCodeLabel, lotNoLabel }) > 0;

                    bool IsOcbPrinted = await conn.ExecuteScalarAsync<int>(@"
                        SELECT COUNT(*) FROM prdprnt_ocb_labels_tcl 
                        WHERE ITEM_CODE = '@ITEM_CODE' AND LOT_NO = '@LOT_NO'
                    ", new { ITEM_CODE = itemCodeLabel, lotNoLabel }) > 0;

                    bool IsCbPrinted = await conn.ExecuteScalarAsync<int>(@"
                        SELECT COUNT(*) FROM prdprnt_cb_labels_tcl 
                        WHERE ITEM_CODE = '@ITEM_CODE' AND LOT_NO = '@LOT_NO'
                    ", new { ITEM_CODE = itemCodeLabel, lotNoLabel }) > 0;

                    bool IsAubPrinted = await conn.ExecuteScalarAsync<int>(@"
                        SELECT COUNT(*) FROM prdprnt_aub_labels_tcl 
                        WHERE ITEM_CODE = '@ITEM_CODE' AND LOT_NO = '@LOT_NO'
                    ", new { ITEM_CODE = itemCodeLabel, lotNoLabel }) > 0;

                    bool IsAcbPrinted = await conn.ExecuteScalarAsync<int>(@"
                        SELECT COUNT(*) FROM prdprnt_acb_labels_tcl 
                        WHERE ITEM_CODE = '@ITEM_CODE' AND LOT_NO = '@LOT_NO'
                    ", new { ITEM_CODE = itemCodeLabel, lotNoLabel }) > 0;


                    var result = new BarcodeScanResult
                    {
                        Barcode = barcodeData,
                        BoxType = gs1_128Instruction.Packaging,
                        IsAcbExists = IsAcbExists,
                        IsAubExists = IsAubExists,
                        IsOcbExists = IsOcbExists,
                        IsCbExists = IsCbExists,
                        IsCaseExists = IsCaseExists,
                        IsOubExists = IsOubExists,
                        IsUbExists = IsUbExists,
                        ItemCodeLabel = itemCodeLabel,
                        LotNoLabel = lotNoLabel,
                        IsPaired = isPaired,
                        IsScrapped = isScrapped                        
                    };

                    return Result.Success<BarcodeScanResult>(result);
                }
                else
                {
                    return Result.Failure<BarcodeScanResult>("Barcode not found in the database.");
                }
            }
            catch (Exception ex)
            {
                return Result.Failure<BarcodeScanResult>($"Error processing barcode scan: {ex.Message}");
            }
        }
    
        public async Task<(int Packaging, string LotNo, string AssemblyLineSuffix)> DecodeGS1_128(string barcode)
        {
            // Scan Product Barcode Number
            int startIndex = 0;
            int _startIndex = 0;
            string productBarcodeNumber;
            int packaging = -1;
            int instructionType = -1;
            int instructionStatus = -1;
            string instructionRevision = string.Empty;
            string lotNumber = string.Empty;
            string assemblyLineSuffix = string.Empty;
            int lmiVersionIndex = 0;
            string manufactureDate = string.Empty;
            string ptw = string.Empty;
            string lMVersionNo = string.Empty;

            for (int index = 0; index < barcode.Length; index++)
            {
                if (index > 1 && !barcode[index].ToString().Equals("0"))
                {
                    _startIndex = index;
                    break;
                }
            }

            startIndex = _startIndex + 1;
            productBarcodeNumber = barcode.Substring(startIndex, 16 - startIndex);

            // Get packaging type
            if(!int.TryParse(barcode[_startIndex].ToString(), out packaging))
            {
                packaging = -1;
            }

            if (barcode.Substring(16, 2) == "92")
            {
                // Get intruction type
                int.TryParse(barcode.Substring(18, 2), out instructionType);

                // Get instruction status
                int.TryParse(barcode.Substring(20, 2), out instructionStatus);

                // Get intruction revision
                instructionRevision = barcode.Substring(22, 2);
            }

            // Get lot number
            if(barcode.Length >= 32 && barcode.Substring(24, 2).Equals("10"))
            {
                lotNumber = barcode.Substring(26, 6);
            }
            else
            {
                lotNumber = barcode.Substring(18, 2);
            }

            // Get assembly line suffix
            if(Regex.Match(barcode, "[A-Z][A-Z0*]{0,1}").Success)
            {
                if(barcode.Substring(34, 2).Equals("10"))
                {
                    lotNumber = barcode.Substring(26, 6);
                }
                else if(barcode.Length >= 24 && barcode.Substring(16, 2).Equals("10"))
                {
                    lotNumber = barcode.Substring(18, 6);
                }

                int suffixIndex = barcode.LastIndexOf(lotNumber) + lotNumber.Length;
                assemblyLineSuffix = barcode.Substring(suffixIndex);
            }

            // Separates the Laser Marking Version Number (denoted by '0' preceding (99)) from the assembly line suffix
            var lmiVersionMatch = Regex.Match(assemblyLineSuffix, "[0-9]99");
            if (lmiVersionMatch.Success)
            {
                lmiVersionIndex = lmiVersionMatch.Index;
                assemblyLineSuffix = assemblyLineSuffix.Substring(0, lmiVersionIndex);

                if(!assemblyLineSuffix.Contains("-")) goto suffixDone;
            }

            var eumdrSequenceMatch = Regex.Match(assemblyLineSuffix, "91");
            if (!eumdrSequenceMatch.Success)
            {
                int eumdrSuffixIndex = eumdrSequenceMatch.Index;
                assemblyLineSuffix = assemblyLineSuffix.Substring(0, eumdrSuffixIndex);
            }

        suffixDone:
            // Get manufacture date
            using var conn = await _factory.CreateConnectionAsync();
            var result = await conn.QueryFirstAsync<DateTime>("SELECT MANUFACTURE_DATE FROM ppt_lbl_instructn_plns_tcl WHERE LOT_NO = '@LOT_NO' AND (CB_LBL_INS_CODE = '@LI_CODE' OR UB_LBL_INS_CODE = '@LI_CODE' OR OUB_LBL_INS_CODE = '@LI_CODE' OR OCB_LBL_INS_CODE = '@LI_CODE')", new { LOT_NO = $"{lotNumber}{assemblyLineSuffix}", LI_CODE = barcode });
            manufactureDate = result.ToString("yyyy-MM-dd");

            if(barcode.Substring(2, 1).Equals("1"))
            {
                int _ptwIndex = 0;
                int PTWLengthAdjust = 2;
                int PTWIndexAdjust = 0;
                bool _hasSuffix = HasSuffix(barcode, lotNumber);

                if (_hasSuffix)
                {
                    _ptwIndex = 34;
                }
                else
                {
                    _ptwIndex = 33;
                }

                if(!barcode.Substring(_ptwIndex, 2).Equals("99"))
                {
                    PTWLengthAdjust = 3;
                    PTWIndexAdjust = 0;
                }

                if (barcode.Substring(24, 2).Equals("10"))
                {
                    int assemblyLineLength = 0;

                    if(!barcode.Substring(_ptwIndex, 2).Equals("99"))
                    {
                        assemblyLineLength = 1;
                    }

                    if(barcode.Length >= 32)
                    {
                        lMVersionNo = barcode.Substring(_hasSuffix ? 32 + assemblyLineLength + 1 : 32, 1);
                    }
                }

                if (barcode.Substring(_ptwIndex + PTWIndexAdjust, 2).Equals("99") && barcode.Length >= _ptwIndex)
                {
                    ptw = barcode.Substring(_ptwIndex + PTWLengthAdjust, barcode.Length - (_ptwIndex + PTWIndexAdjust + 2));
                }
            }

            return (packaging, lotNumber, assemblyLineSuffix);
        }

        private bool HasSuffix(string barcode, string lotNo)
        {
            string lotNoWithIndicator = $"10{lotNo}";
            int suffixIndex = barcode.IndexOf(lotNoWithIndicator) + lotNoWithIndicator.Length;
            string suffix = barcode.Substring(suffixIndex, 1);
            return !int.TryParse(suffix, out _);
        }
    }
}
