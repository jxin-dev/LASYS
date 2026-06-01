using Dapper;
using LASYS.Application.Common.Enums;
using LASYS.Application.Common.Results;
using LASYS.Application.Interfaces.Persistence;
using MediatR;

namespace LASYS.Application.Features.PrintLabels.Queries.GetNiceLabelFile
{
    public sealed class GetNiceLabelFileHandler : IRequestHandler<GetNiceLabelFileQuery, Result<byte[]?>>
    {
        private readonly IDbConnectionFactory _factory;
        public GetNiceLabelFileHandler(IDbConnectionFactory factory)
        {
            _factory = factory;
        }

        public async Task<Result<byte[]?>> Handle(GetNiceLabelFileQuery request, CancellationToken cancellationToken)
        {
            try
            {
                string column = GetColumnName(request.BoxType);

                if (string.IsNullOrEmpty(column))
                {
                    return Result.Failure<byte[]?>("Invalid box type.");
                }

                var sql = $@"SELECT {column} FROM pre_masterlabels_tcl
                    WHERE ITEM_CODE = @ItemCode
                    AND MASTERLABEL_REVISION_NUMBER = @MasterLabelRevNumber;";

                using var connection = await _factory.CreateConnectionAsync();
                var result = await connection.QueryFirstOrDefaultAsync<byte[]?>(sql,
                    new
                    {
                        request.ItemCode,
                        request.MasterLabelRevNumber,
                    });
                return Result.Success(result);
            }
            catch (Exception ex)
            {
                return Result.Failure<byte[]?>($"An error occurred while retrieving the label file: {ex.Message}");
            }
        }

        private static string GetColumnName(BoxType boxType) => boxType switch
        {
            BoxType.UnitBox => "UB_LABEL_FILE",
            BoxType.AdditionalUnitBox => "AUB_LABEL_FILE",
            BoxType.OuterUnitBox => "OUB_LABEL_FILE",
            BoxType.CartonBox => "CB_LABEL_FILE",
            BoxType.AdditionalCartonBox => "ACB_LABEL_FILE",
            BoxType.OuterCartonBox => "OCB_LABEL_FILE",
            _ => throw new ArgumentOutOfRangeException(nameof(boxType))
        };
    }
}
