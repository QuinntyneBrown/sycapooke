using MediatR;
using Microsoft.EntityFrameworkCore;
using Sycapooke.Application.Abstractions;
using Sycapooke.Application.Accounts;
using Sycapooke.Application.Common;
using Sycapooke.Domain.Members;

namespace Sycapooke.Application.Shop;

/// <summary>What the founder edits and the public page shows: name, story and an optional city (ADR general/0010).</summary>
public sealed record FounderStoryDto(string DisplayName, string Story, string City, string Region);

public sealed record GetFounderStoryQuery(Guid FounderId) : IRequest<Result<FounderStoryDto>>;

/// <summary>Saves the name and the public story. The founder comes from the session, never the request.</summary>
public sealed record UpdateFounderStoryCommand(Guid FounderId, string? DisplayName, string? Story, string? City, string? Region) :
    IRequest<Result<FounderStoryDto>>;

public sealed class GetFounderStoryQueryHandler(ISycapookeDbContext db) : IRequestHandler<GetFounderStoryQuery, Result<FounderStoryDto>>
{
    public async Task<Result<FounderStoryDto>> Handle(GetFounderStoryQuery query, CancellationToken cancellationToken)
    {
        var member = await db.Members.AsNoTracking().SingleAsync(m => m.Id == query.FounderId, cancellationToken);
        return new FounderStoryDto(member.DisplayName, member.StoryBio ?? string.Empty, member.StoryCity ?? string.Empty, member.StoryRegion ?? string.Empty);
    }
}

public sealed class UpdateFounderStoryHandler(ISycapookeDbContext db) : IRequestHandler<UpdateFounderStoryCommand, Result<FounderStoryDto>>
{
    public async Task<Result<FounderStoryDto>> Handle(UpdateFounderStoryCommand command, CancellationToken cancellationToken)
    {
        var member = await db.Members.SingleAsync(m => m.Id == command.FounderId, cancellationToken);
        var name = (command.DisplayName ?? string.Empty).Trim();
        var story = (command.Story ?? string.Empty).Trim();
        var city = (command.City ?? string.Empty).Trim();
        var region = (command.Region ?? string.Empty).Trim();
        var errors = new Dictionary<string, FieldError>();
        await MemberRegistrationRules.ValidateDisplayNameAsync(db, name, errors, cancellationToken, excluding: member.Id);
        if (story.Length > Member.StoryBioMaxLength)
        {
            errors["story"] = new FieldError("TOO_LONG", $"Keep your story to {Member.StoryBioMaxLength} characters.");
        }

        if (city.Length > 60)
        {
            errors["city"] = new FieldError("TOO_LONG", "Keep the city to 60 characters.");
        }

        if (region.Length > 60)
        {
            errors["region"] = new FieldError("TOO_LONG", "Keep the state or region to 60 characters.");
        }

        if (errors.Count > 0)
        {
            return Error.Invalid("VALIDATION_FAILED", errors);
        }

        member.Rename(name);
        member.UpdateStory(story.Length == 0 ? null : story, city.Length == 0 ? null : city, region.Length == 0 ? null : region);
        await db.SaveChangesAsync(cancellationToken);
        return new FounderStoryDto(member.DisplayName, story, city, region);
    }
}
