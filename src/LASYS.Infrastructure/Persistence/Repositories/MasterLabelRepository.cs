using Dapper;
using LASYS.Application.Common.Enums;
using LASYS.Application.Common.Models;
using LASYS.Application.Features.NiceLabels;
using LASYS.Application.Interfaces.Persistence;
using LASYS.Application.Interfaces.Persistence.Repositories;
using LASYS.Application.Interfaces.Persistence.TableMappings;

namespace LASYS.Infrastructure.Persistence.Repositories
{
    public sealed class MasterLabelRepository : IMasterLabelRepository
    {
        private readonly IDbConnectionFactory _factory;
        private readonly INiceLabelColumnResolver _niceLabelColumnResolver;
        private readonly IMasterLabelColumnResolver _masterLabelColumnResolver;

        public MasterLabelRepository(IDbConnectionFactory factory, INiceLabelColumnResolver niceLabelColumnResolver, IMasterLabelColumnResolver masterLabelColumnResolver)
        {
            _factory = factory;
            _niceLabelColumnResolver = niceLabelColumnResolver;
            _masterLabelColumnResolver = masterLabelColumnResolver;
        }

        public async Task<MasterLabelDetails> GetDetailsAsync(string itemCode, uint masterRevision, BoxType boxType)
        {
            var columns = _masterLabelColumnResolver.GetColumns(boxType);

            var sql = @$"
            SELECT
                ITEM_CODE AS ItemCode,
                MASTERLABEL_REVISION_NUMBER AS MasterLabelRevisionNumber,
                PLANT_CODE AS PlantCode,
                ITEM_GROUP_TYPE_CODE AS ItemGroupTypeCode,
                MARKET_CODE AS MarketCode,
                MASTERLABEL_STATUS AS MasterLabelStatus,
                MASTERLABEL_REVISION_DATE AS MasterLabelRevisionDate,
                MASTERLABEL_REVISION_DETAILS AS MasterLabelRevisionDetails,
                MASTERLABEL_EFFECTIVITY_DATE AS MasterLabelEffectivityDate,
                UDI AS Udi,
                UDI_DATE AS UdiDate,
                ROCHE AS Roche,
                {columns.WidthColumn} AS LabelWidth,
                {columns.HeightColumn} AS LabelHeight,
                {columns.FileColumn} AS LabelFile,
                {columns.ImageFileColumn} AS ImageFile,
                {columns.PathColumn} AS FilePath,
                {columns.MaterialCodeColumn} AS MaterialCode,
                {columns.MaterialDescriptionColumn} AS MaterialDescription,
                {columns.MaterialConsumeQuantityColumn} AS MaterialConsumeQuantity,
                {columns.OcrSupportedColumn} AS IsOcrSupported
            FROM pre_masterlabels_tcl
            WHERE
            ITEM_CODE = @itemCode
            AND MASTERLABEL_REVISION_NUMBER = @masterRevision;";
            try
            {
                using var connection = await _factory.CreateConnectionAsync();
                var result = await connection.QueryFirstOrDefaultAsync<MasterLabelDetails>(sql, new { itemCode, masterRevision });

                return result?.WithBoxType(boxType) ?? throw new KeyNotFoundException(
                    $"Master label not found for ItemCode={itemCode}, Revision={masterRevision}, BoxType={boxType}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving master label details for ItemCode={itemCode}, Revision={masterRevision}, BoxType={boxType}", ex);
            }
        }

        public async Task<NiceLabel> GetNiceLabelDetailsAsync(string itemCode, uint masterRevision, BoxType boxType)
        {
            try
            {
                var (pathColumn, fileColumn) = _niceLabelColumnResolver.GetColumnNames(boxType);

                var sql = $@"
                SELECT
                    {pathColumn} AS NiceLabelPath,
                    {fileColumn} AS NiceLabelFile
                FROM pre_masterlabels_tcl
                WHERE ITEM_CODE = @itemCode AND MASTERLABEL_REVISION_NUMBER = @masterRevision;";

                using var connection = await _factory.CreateConnectionAsync();
                var result = await connection.QueryFirstOrDefaultAsync<NiceLabel>(sql, new { itemCode, masterRevision });
                return result ??
                    throw new Exception("No nice label details found for the specified item code, master revision, and box type.");

            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving nice label details: {ex.Message}", ex);
            }
        }
    }
}
