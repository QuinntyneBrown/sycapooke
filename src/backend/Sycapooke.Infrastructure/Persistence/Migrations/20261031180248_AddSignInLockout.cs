using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sycapooke.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class AddSignInLockout : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "FailedSignIns",
            table: "Members",
            type: "int",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<DateTime>(
            name: "FailedSignInsSince",
            table: "Members",
            type: "datetime2",
            nullable: true);

        migrationBuilder.AddColumn<DateTime>(
            name: "LockedUntil",
            table: "Members",
            type: "datetime2",
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "FailedSignIns",
            table: "Members");

        migrationBuilder.DropColumn(
            name: "FailedSignInsSince",
            table: "Members");

        migrationBuilder.DropColumn(
            name: "LockedUntil",
            table: "Members");
    }
}
