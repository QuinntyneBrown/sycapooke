using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sycapooke.Api.Auth;
using Sycapooke.Api.Http;
using Sycapooke.Application.Accounts;

namespace Sycapooke.Api.Controllers;

/// <summary>The signed-in member's own private account (L2-004). The member is always the session's; no ID is accepted.</summary>
[ApiController]
[Authorize]
[Route("api/account")]
public sealed class PrivateAccountController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetPrivateAccountQuery(User.MemberId()), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : this.ToProblem(result.Error!);
    }

    [HttpPatch]
    public async Task<IActionResult> Update(UpdatePrivateAccountRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new UpdatePrivateAccountCommand(User.MemberId(), request.DisplayName), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : this.ToProblem(result.Error!);
    }
}

public sealed record UpdatePrivateAccountRequest(string? DisplayName);
