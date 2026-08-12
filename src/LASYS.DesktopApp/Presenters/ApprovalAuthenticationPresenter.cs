using System.Windows.Forms;
using LASYS.Application.Common.Enums;
using LASYS.Application.Features.Authentication.Login;
using LASYS.Application.Features.BatchPrinting.Events;
using LASYS.Application.Interfaces.Context;
using LASYS.Application.Interfaces.Services;
using LASYS.DesktopApp.Core.Interfaces;
using LASYS.DesktopApp.Events;
using LASYS.DesktopApp.Views.Forms;
using LASYS.DesktopApp.Views.Interfaces;
using MediatR;

namespace LASYS.DesktopApp.Presenters
{
    public class ApprovalAuthenticationPresenter
    {
        public ApprovalAuthenticationForm View { get; }
        private readonly IApprovalAuthenticationView _view;
        private readonly IMediator _mediator;
        private readonly ICurrentUser _currentUser;
        private readonly IPermissionService _permissionService;
        public event EventHandler<ApprovalAuthorizedEventArgs>? AuthorizationSucceeded;
        public event EventHandler? AuthorizationCancelled;

        public ApprovalAuthenticationPresenter(IApprovalAuthenticationView view, IMediator mediator, ICurrentUser currentUser, IPermissionService permissionService)
        {
            _view = view;
            _mediator = mediator;

            View = (ApprovalAuthenticationForm)view;

            _view.ApprovalRequested += OnApprovalRequested;
            _view.ApprovalCancelled += OnApprovalCancelled;
            _currentUser = currentUser;
            _permissionService = permissionService;
        }

        private void OnApprovalCancelled(object? sender, EventArgs e)
        {
            _view.CloseApproval();
            AuthorizationCancelled?.Invoke(this, EventArgs.Empty);
        }

        private async void OnApprovalRequested(object? sender, ApprovalCredentialsEventArgs e)
        {
            if (_currentUser.Username.ToLower().Trim() == e.Username.ToLower().Trim())
            {
                _view.ApprovalFailed("You cannot approve your own request.");
                return;
            }

           _view.InvokeOnUI(()=> _view.EnableApproveButton(false));
            var result = await _mediator.Send(new LoginCommand(e.Username, e.Password));

            if (result.IsSuccess)
            {
                var hasAccess = _permissionService.HasAccess("Barcode Label Work Order", AccessLevel.Admin);
                if (!hasAccess)
                {
                    _view.ApprovalFailed("You do not have the required permissions.");
                    _view.InvokeOnUI(() => _view.EnableApproveButton(true));
                    return;
                }
                _view.ApprovalSucceeded();
                var user = result.Value!;

                AuthorizationSucceeded?.Invoke(this, new ApprovalAuthorizedEventArgs(user.UserCode, user.SectionId!));
            }
            else
            {
                _view.ApprovalFailed(result.Error!);
            }
            _view.InvokeOnUI(() => _view.EnableApproveButton(true));
        }
    }
}
