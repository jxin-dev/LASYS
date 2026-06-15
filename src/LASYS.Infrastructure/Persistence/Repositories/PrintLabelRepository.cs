using Dapper;
using LASYS.Application.Common.Enums;
using LASYS.Application.Common.Models;
using LASYS.Application.Features.BatchPrinting.Models;
using LASYS.Application.Interfaces.Context;
using LASYS.Application.Interfaces.Persistence;
using LASYS.Application.Interfaces.Persistence.Repositories;
using LASYS.Application.Interfaces.Persistence.TableMappings;
using LASYS.Application.Interfaces.Services;

namespace LASYS.Infrastructure.Persistence.Repositories
{
    public sealed class PrintLabelRepository : IPrintLabelRepository
    {
        private readonly IDbConnectionFactory _factory;
        private readonly IPrintTableResolver _printTableNameResolver;
        private readonly IIpAddressProvider _ipAddressProvider;
        private readonly ICurrentUser _currentUser;

        public PrintLabelRepository(IDbConnectionFactory factory, IPrintTableResolver printTableNameResolver, IIpAddressProvider ipAddressProvider, ICurrentUser currentUser)
        {
            _factory = factory;
            _printTableNameResolver = printTableNameResolver;
            _ipAddressProvider = ipAddressProvider;
            _currentUser = currentUser;
        }

        public async Task<PrintDetails> GetDetailsAsync(string itemCode, string lotNo, string printType, BoxType boxType)
        {
            string tableName = _printTableNameResolver.GetTableName(boxType);
            string sql = boxType == BoxType.CaseLabel ?
                $@"
                SELECT 
                    BATCH_NUMBER,
                    SET_NUMBER,
                    TOTAL_CASE_LABELS_PRINTED,
                    PASSED_CASE_LABELS_PRINTED, 
                    FAILED_CASE_LABELS_PRINTED,
                    SAMPLE_CASE_LABELS_PRINTED 
                FROM PRDPRNT_CASE_LABELS_TCL "" & _
                WHERE ITEM_CODE = @ItemCode AND LOT_NO = @LotNo {{2}} ORDER BY BATCH_NUMBER, SET_NUMBER {{3}}; """
                :
                $@"
                SELECT 
                    ITEM_CODE AS ItemCode,
                    LOT_NO AS LotNo,
                    COUNT(CASE WHEN LABEL_STATUS IN ('Original','Replacement','Additional','Returned') THEN 1 END) AS TotalPassed,
                    COUNT(CASE WHEN LABEL_STATUS IN ('Failed During Printing','Failed After Printing') THEN 1 END) AS TotalFailed,
                    COUNT(CASE WHEN LABEL_STATUS IN ('First', 'Last') THEN 1 END) AS TotalSample,
                    COUNT(CASE WHEN LABEL_STATUS IN 
                        ('First','Last','Original','Replacement','Additional','Failed During Printing','Failed After Printing','Returned') THEN 1 END) AS TotalPrinted,
                    IFNULL(MAX(SEQUENCE_NUMBER) + 1, 1) AS NextSequence,
                    IFNULL(MAX(CASE WHEN LABEL_STATUS IN (@PrintType, 'Failed During Printing','Failed After Printing','First','Last') THEN BATCH_NUMBER END), 1) AS BatchNumber,
                    IFNULL(MAX(SET_NUMBER) + 1, 1) AS SetNumber
                FROM {tableName} 
                WHERE ITEM_CODE = @ItemCode AND LOT_NO = @LotNo;";
            try
            {

                using var connection = await _factory.CreateConnectionAsync();
                var result = await connection.QueryFirstOrDefaultAsync<PrintDetails>(sql, new { itemCode, lotNo, printType });
                return result ?? throw new Exception("Print details not found.");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving print details: {ex.Message}", ex);
            }
        }

        public async Task<bool> SavePrintedLabelAsync(SequenceData sequenceData)
        {
            var ipAddress = _ipAddressProvider.GetLocalIpAddress();
            var tableName = _printTableNameResolver.GetTableName(sequenceData.BoxType);

            string sql = sequenceData.BoxType == BoxType.CaseLabel ?
                $@"

                "
                : $@"
                INSERT INTO {tableName} 
                (
                    ITEM_CODE,
                    LOT_NO,
                    SEQUENCE_NUMBER,
                    PAIRED_TYPE,
                    PRINTER_NAME,
                    BATCH_NUMBER,
                    SET_NUMBER,
                    LABEL_STATUS,
                    ANALYZER_RESULT_STATUS,
                    VERIFIER_RESULT_STATUS,
                    VISUAL_INS_RESULT_STATUS,
                    PRINT_USER_CODE,
                    PRINT_DATETIME,
                    NCPR_CONTROL_CODE,
                    COMMENT,
                    APPROVED_BY_USER_CODE,
                    APPROVED_BY_SECTION_ID,
                    APPROVED_BY_IP_ADDRESS,
                    APPROVED_BY_DATETIME,
                    PARTITION_DATE,
                    CREATED_USER_CODE,
                    CREATED_SECTION_ID,
                    CREATED_IP_ADDRESS,
                    CREATED_DATETIME,
                    LASTUPDATE_USER_CODE,
                    LASTUPDATE_SECTION_ID,
                    LASTUPDATE_IP_ADDRESS,
                    LASTUPDATE_DATETIME
                )
                VALUES
                (
                    @ITEM_CODE,
                    @LOT_NO,
                    @SEQUENCE_NUMBER,
                    @PAIRED_TYPE,
                    @PRINTER_NAME,
                    @BATCH_NUMBER,
                    @SET_NUMBER,
                    @LABEL_STATUS,
                    @ANALYZER_RESULT_STATUS,
                    @VERIFIER_RESULT_STATUS,
                    @VISUAL_INS_RESULT_STATUS,
                    @PRINT_USER_CODE,
                    CURRENT_TIMESTAMP + 0,
                    @NCPR_CONTROL_CODE,
                    @COMMENT,
                    @APPROVED_BY_USER_CODE,
                    @APPROVED_BY_SECTION_ID,
                    @APPROVED_BY_IP_ADDRESS,
                    @APPROVED_BY_DATETIME,
                    DATE_FORMAT(CURRENT_TIMESTAMP,'%Y%m%d'),
                    @CREATED_USER_CODE,
                    @CREATED_SECTION_ID,
                    INET_ATON(@CREATED_IP_ADDRESS),
                    CURRENT_TIMESTAMP + 0,
                    @LASTUPDATE_USER_CODE,
                    @LASTUPDATE_SECTION_ID,
                    INET_ATON(@LASTUPDATE_IP_ADDRESS),
                    CURRENT_TIMESTAMP + 0);";

            
            //"INSERT INTO prdprnt_case_labels_tcl (
            //ITEM_CODE,
            //LOT_NO,
            //BATCH_NUMBER,
            //SET_NUMBER,
            //TOTAL_CASE_LABELS_PRINTED,
            //PASSED_CASE_LABELS_PRINTED, " & _
            //FAILED_CASE_LABELS_PRINTED,
            //SAMPLE_CASE_LABELS_PRINTED, " & _
            //PRINTER_NAME,
            //PARTITION_DATE, " & _
            //CREATED_USER_CODE,
            //CREATED_SECTION_ID, " & _
            //CREATED_IP_ADDRESS,
            //CREATED_DATETIME, " & _
            //LASTUPDATE_USER_CODE,
            //LASTUPDATE_SECTION_ID, " & _
            //LASTUPDATE_IP_ADDRESS,
            //LASTUPDATE_DATETIME, " & _
            //APPROVED_BY_USER_CODE,
            //APPROVED_BY_SECTION_ID, " & _
            //APPROVED_BY_IP_ADDRESS,
            //APPROVED_BY_DATETIME) VALUES " & _
            try
            {
                using var connection = await _factory.CreateConnectionAsync();
                var rowsAffected = await connection.ExecuteAsync(sql, new
                {
                    ITEM_CODE = sequenceData.ItemCode,
                    LOT_NO = sequenceData.LotNo,
                    SEQUENCE_NUMBER = sequenceData.SequenceNumber,
                    PAIRED_TYPE = sequenceData.PairedType,
                    PRINTER_NAME = sequenceData.PrinterName,
                    BATCH_NUMBER = sequenceData.BatchNumber,
                    SET_NUMBER = sequenceData.SetNumber,
                    LABEL_STATUS = sequenceData.LabelStatus,
                    ANALYZER_RESULT_STATUS = sequenceData.AnalyzerResultStatus,
                    VERIFIER_RESULT_STATUS = sequenceData.VerifierResultStatus,
                    VISUAL_INS_RESULT_STATUS = sequenceData.VisualInspectionResultStatus,
                    PRINT_USER_CODE = _currentUser.UserCode,
                    NCPR_CONTROL_CODE = sequenceData.NcprControlCode,
                    COMMENT = sequenceData.Comment,
                    APPROVED_BY_USER_CODE = sequenceData.ApprovedByUserCode,
                    APPROVED_BY_SECTION_ID = sequenceData.ApprovedBySectionId,
                    APPROVED_BY_IP_ADDRESS = sequenceData.ApprovedByIpAddress,
                    APPROVED_BY_DATETIME = sequenceData.ApprovedByDateTime,
                    CREATED_USER_CODE = _currentUser.UserCode,
                    CREATED_SECTION_ID = _currentUser.SectionId,
                    CREATED_IP_ADDRESS = ipAddress,
                    LASTUPDATE_USER_CODE = _currentUser.UserCode,
                    LASTUPDATE_SECTION_ID = _currentUser.SectionId,
                    @LASTUPDATE_IP_ADDRESS = ipAddress
                });

                return rowsAffected > 0;
            }
            catch(Exception ex)
            {
                return false;
            }
        }
    }
}
