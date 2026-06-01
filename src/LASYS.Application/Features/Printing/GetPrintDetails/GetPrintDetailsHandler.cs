using LASYS.Application.Common.Models;
using LASYS.Application.Common.Results;
using LASYS.Application.Interfaces.Persistence.Repositories;
using MediatR;

namespace LASYS.Application.Features.Printing.GetPrintDetails
{
    public sealed class GetPrintDetailsHandler : IRequestHandler<GetPrintDetailsQuery, Result<PrintDetails>>
    {
        private readonly IPrintLabelRepository _printLabelRepository;
        public GetPrintDetailsHandler(IPrintLabelRepository printLabelRepository)
        {
            _printLabelRepository = printLabelRepository;
        }

        public async Task<Result<PrintDetails>> Handle(GetPrintDetailsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var printDetails = await _printLabelRepository.GetDetailsAsync(request.ItemCode, request.LotNo, request.BoxType);
                if (printDetails is null)
                {
                    return Result.Failure<PrintDetails>("Print details not found");
                }
                return Result.Success(printDetails);
            }
            catch (Exception ex)
            {
                return Result.Failure<PrintDetails>($"An error occurred while retrieving print details: {ex.Message}");
            }

        }
    }
}
