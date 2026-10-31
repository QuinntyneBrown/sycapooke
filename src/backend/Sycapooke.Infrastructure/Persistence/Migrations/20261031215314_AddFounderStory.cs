using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sycapooke.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class AddFounderStory : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "StoryBio",
            table: "Members",
            type: "nvarchar(500)",
            maxLength: 500,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "StoryCity",
            table: "Members",
            type: "nvarchar(60)",
            maxLength: 60,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "StoryRegion",
            table: "Members",
            type: "nvarchar(60)",
            maxLength: 60,
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "StoryBio",
            table: "Members");

        migrationBuilder.DropColumn(
            name: "StoryCity",
            table: "Members");

        migrationBuilder.DropColumn(
            name: "StoryRegion",
            table: "Members");
    }
}
