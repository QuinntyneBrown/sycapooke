using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sycapooke.Api.Auth;
using Sycapooke.Api.Http;
using Sycapooke.Application.Orders;

namespace Sycapooke.Api.Controllers;

/// <summary>
/// The founder's side of an order problem (L2-036): read it and decide it. Founder only;
/// 403 FOUNDER_REQUIRED for anyone else (L2-010).
/// </summary>
[ApiController]
[Authorize(Policy = Policies.CanSell)]
public sealed class ShopProblemsController(ISender sender) : ControllerBase
{
    [HttpGet("api/shop/problems/{reference}")]
    public async Task<IActionResult> Get(string reference, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetProblemDetailQuery(reference), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : this.ToProblem(result.Error!);
    }

    [HttpPost("api/shop/problems/{reference}/resolution")]
    public async Task<IActionResult> Resolve(string reference, ResolutionRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ResolveProblemCommand(User.MemberId(), reference, request.Outcome, request.Reason), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : this.ToProblem(result.Error!);
    }
}

public sealed record ResolutionRequest(string? Outcome, string? Reason);
