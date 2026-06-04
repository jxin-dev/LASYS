using LASYS.Application.Common.Results;
using LASYS.Application.Interfaces.Context;
using LASYS.Application.Interfaces.Persistence.Repositories;
using LASYS.Application.Interfaces.Persistence.TableMappings;
using MediatR;

namespace LASYS.Application.Features.Printing.CreatePrintedLabel
{
    public sealed class CreatePrintedLabelHandler : IRequestHandler<CreatePrintedLabelCommand, Result<Unit>>
    {
        private readonly IPrintLabelRepository _printLabelRepository;
        private readonly IPrintTableResolver _printTableResolver;
        private readonly ICurrentUser _currentUser;

        public CreatePrintedLabelHandler(IPrintLabelRepository printLabelRepository, IPrintTableResolver printTableResolver, ICurrentUser currentUser)
        {
            _printLabelRepository = printLabelRepository;
            _printTableResolver = printTableResolver;
            _currentUser = currentUser;
        }

        public Task<Result<Unit>> Handle(CreatePrintedLabelCommand request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
