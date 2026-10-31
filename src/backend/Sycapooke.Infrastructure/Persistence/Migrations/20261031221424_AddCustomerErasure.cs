using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sycapooke.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class AddCustomerErasure : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<DateTime>(
            name: "ErasedAt",
            table: "Members",
            type: "datetime2",
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "ErasedAt",
            table: "Members");
    }
}
