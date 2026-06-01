using Dapper;
using LASYS.Application.Common.Results;
using LASYS.Application.Interfaces.Persistence;
using LASYS.Application.Interfaces.Persistence.Repositories;
using MediatR;

namespace LASYS.Application.Features.NiceLabels.GetNiceLabelDetails
{
    public sealed class GetNiceLabelDetailsHandler : IRequestHandler<GetNiceLabelDetailsQuery, Result<NiceLabel>>
    {
        private readonly IDbConnectionFactory _factory;
        private readonly IMasterLabelRepository _masterLabelRepository;

        public GetNiceLabelDetailsHandler(IDbConnectionFactory factory, IMasterLabelRepository masterLabelRepository)
        {
            _factory = factory;
            _masterLabelRepository = masterLabelRepository;
        }

        public async Task<Result<NiceLabel>> Handle(GetNiceLabelDetailsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var niceLabel = await _masterLabelRepository.GetNiceLabelDetailsAsync(request.ItemCode, request.MasterRevision, request.BoxType);
                if (niceLabel is null)
                {
                    return Result.Failure<NiceLabel>("NiceLabel not found");
                }
                return Result.Success(niceLabel);
            }
            catch (Exception ex)
            {
                return Result.Failure<NiceLabel>($"An error occurred while retrieving product details: {ex.Message}");
            }
        }
    }
}
