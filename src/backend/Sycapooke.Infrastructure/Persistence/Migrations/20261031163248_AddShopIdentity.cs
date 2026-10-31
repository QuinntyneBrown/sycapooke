using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sycapooke.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class AddShopIdentity : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "ShopIdentity",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false),
                FounderMemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ShopIdentity", x => x.Id);
                table.CheckConstraint("CK_ShopIdentity_Singleton", "[Id] = 1");
                table.ForeignKey(
                    name: "FK_ShopIdentity_Members_FounderMemberId",
                    column: x => x.FounderMemberId,
                    principalTable: "Members",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "UX_ShopIdentity_FounderMemberId",
            table: "ShopIdentity",
            column: "FounderMemberId",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "ShopIdentity");
    }
}
