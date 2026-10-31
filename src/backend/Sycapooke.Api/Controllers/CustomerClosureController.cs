using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sycapooke.Api.Auth;
using Sycapooke.Api.Http;
using Sycapooke.Application.Accounts;

namespace Sycapooke.Api.Controllers;

/// <summary>A customer closes their own account (L2-005). The member is always the session's.</summary>
[ApiController]
[Authorize]
public sealed class CustomerClosureController(ISender sender) : ControllerBase
{
    [HttpPost("api/account/closure")]
    public async Task<IActionResult> Close(CloseAccountRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CloseCustomerAccountCommand(User.MemberId(), request.Password), cancellationToken);
        if (!result.IsSuccess)
        {
            return this.ToProblem(result.Error!);
        }

        SessionCookies.Clear(Response);
        return NoContent();
    }
}

public sealed record CloseAccountRequest(string? Password);
