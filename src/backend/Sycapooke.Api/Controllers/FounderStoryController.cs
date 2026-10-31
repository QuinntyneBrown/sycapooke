using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sycapooke.Api.Auth;
using Sycapooke.Api.Http;
using Sycapooke.Application.Shop;

namespace Sycapooke.Api.Controllers;

/// <summary>The founder edits what her public page shows. Only the founder may call it (L2-010).</summary>
[ApiController]
[Authorize(Policy = Policies.CanSell)]
[Route("api/shop/story")]
public sealed class FounderStoryController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetFounderStoryQuery(User.MemberId()), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : this.ToProblem(result.Error!);
    }

    [HttpPut]
    public async Task<IActionResult> Put(UpdateFounderStoryRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new UpdateFounderStoryCommand(User.MemberId(), request.DisplayName, request.Story, request.City, request.Region), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : this.ToProblem(result.Error!);
    }
}

public sealed record UpdateFounderStoryRequest(string? DisplayName, string? Story, string? City, string? Region);
