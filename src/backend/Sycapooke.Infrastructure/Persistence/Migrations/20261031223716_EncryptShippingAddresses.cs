using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sycapooke.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class EncryptShippingAddresses : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "Zip",
            table: "SavedAddresses",
            type: "nvarchar(100)",
            maxLength: 100,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(10)",
            oldMaxLength: 10);

        migrationBuilder.AlterColumn<string>(
            name: "State",
            table: "SavedAddresses",
            type: "nvarchar(100)",
            maxLength: 100,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(2)",
            oldMaxLength: 2);

        migrationBuilder.AlterColumn<string>(
            name: "Line2",
            table: "SavedAddresses",
            type: "nvarchar(700)",
            maxLength: 700,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(100)",
            oldMaxLength: 100,
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "Line1",
            table: "SavedAddresses",
            type: "nvarchar(700)",
            maxLength: 700,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(100)",
            oldMaxLength: 100);

        migrationBuilder.AlterColumn<string>(
            name: "FullName",
            table: "SavedAddresses",
            type: "nvarchar(700)",
            maxLength: 700,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(100)",
            oldMaxLength: 100);

        migrationBuilder.AlterColumn<string>(
            name: "City",
            table: "SavedAddresses",
            type: "nvarchar(400)",
            maxLength: 400,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(60)",
            oldMaxLength: 60);

        migrationBuilder.AlterColumn<string>(
            name: "ShippingZip",
            table: "Orders",
            type: "nvarchar(100)",
            maxLength: 100,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(10)",
            oldMaxLength: 10);

        migrationBuilder.AlterColumn<string>(
            name: "ShippingState",
            table: "Orders",
            type: "nvarchar(100)",
            maxLength: 100,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(2)",
            oldMaxLength: 2);

        migrationBuilder.AlterColumn<string>(
            name: "ShippingLine2",
            table: "Orders",
            type: "nvarchar(700)",
            maxLength: 700,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(100)",
            oldMaxLength: 100,
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "ShippingLine1",
            table: "Orders",
            type: "nvarchar(700)",
            maxLength: 700,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(100)",
            oldMaxLength: 100);

        migrationBuilder.AlterColumn<string>(
            name: "ShippingFullName",
            table: "Orders",
            type: "nvarchar(700)",
            maxLength: 700,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(100)",
            oldMaxLength: 100);

        migrationBuilder.AlterColumn<string>(
            name: "ShippingCity",
            table: "Orders",
            type: "nvarchar(400)",
            maxLength: 400,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(60)",
            oldMaxLength: 60);

        migrationBuilder.AlterColumn<string>(
            name: "ShippingZip",
            table: "Checkouts",
            type: "nvarchar(100)",
            maxLength: 100,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(10)",
            oldMaxLength: 10,
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "ShippingState",
            table: "Checkouts",
            type: "nvarchar(100)",
            maxLength: 100,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(2)",
            oldMaxLength: 2,
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "ShippingLine2",
            table: "Checkouts",
            type: "nvarchar(700)",
            maxLength: 700,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(100)",
            oldMaxLength: 100,
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "ShippingLine1",
            table: "Checkouts",
            type: "nvarchar(700)",
            maxLength: 700,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(100)",
            oldMaxLength: 100,
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "ShippingFullName",
            table: "Checkouts",
            type: "nvarchar(700)",
            maxLength: 700,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(100)",
            oldMaxLength: 100,
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "ShippingCity",
            table: "Checkouts",
            type: "nvarchar(400)",
            maxLength: 400,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(60)",
            oldMaxLength: 60,
            oldNullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "Zip",
            table: "SavedAddresses",
            type: "nvarchar(10)",
            maxLength: 10,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(100)",
            oldMaxLength: 100);

        migrationBuilder.AlterColumn<string>(
            name: "State",
            table: "SavedAddresses",
            type: "nvarchar(2)",
            maxLength: 2,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(100)",
            oldMaxLength: 100);

        migrationBuilder.AlterColumn<string>(
            name: "Line2",
            table: "SavedAddresses",
            type: "nvarchar(100)",
            maxLength: 100,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(700)",
            oldMaxLength: 700,
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "Line1",
            table: "SavedAddresses",
            type: "nvarchar(100)",
            maxLength: 100,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(700)",
            oldMaxLength: 700);

        migrationBuilder.AlterColumn<string>(
            name: "FullName",
            table: "SavedAddresses",
            type: "nvarchar(100)",
            maxLength: 100,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(700)",
            oldMaxLength: 700);

        migrationBuilder.AlterColumn<string>(
            name: "City",
            table: "SavedAddresses",
            type: "nvarchar(60)",
            maxLength: 60,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(400)",
            oldMaxLength: 400);

        migrationBuilder.AlterColumn<string>(
            name: "ShippingZip",
            table: "Orders",
            type: "nvarchar(10)",
            maxLength: 10,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(100)",
            oldMaxLength: 100);

        migrationBuilder.AlterColumn<string>(
            name: "ShippingState",
            table: "Orders",
            type: "nvarchar(2)",
            maxLength: 2,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(100)",
            oldMaxLength: 100);

        migrationBuilder.AlterColumn<string>(
            name: "ShippingLine2",
            table: "Orders",
            type: "nvarchar(100)",
            maxLength: 100,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(700)",
            oldMaxLength: 700,
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "ShippingLine1",
            table: "Orders",
            type: "nvarchar(100)",
            maxLength: 100,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(700)",
            oldMaxLength: 700);

        migrationBuilder.AlterColumn<string>(
            name: "ShippingFullName",
            table: "Orders",
            type: "nvarchar(100)",
            maxLength: 100,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(700)",
            oldMaxLength: 700);

        migrationBuilder.AlterColumn<string>(
            name: "ShippingCity",
            table: "Orders",
            type: "nvarchar(60)",
            maxLength: 60,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(400)",
            oldMaxLength: 400);

        migrationBuilder.AlterColumn<string>(
            name: "ShippingZip",
            table: "Checkouts",
            type: "nvarchar(10)",
            maxLength: 10,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(100)",
            oldMaxLength: 100,
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "ShippingState",
            table: "Checkouts",
            type: "nvarchar(2)",
            maxLength: 2,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(100)",
            oldMaxLength: 100,
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "ShippingLine2",
            table: "Checkouts",
            type: "nvarchar(100)",
            maxLength: 100,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(700)",
            oldMaxLength: 700,
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "ShippingLine1",
            table: "Checkouts",
            type: "nvarchar(100)",
            maxLength: 100,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(700)",
            oldMaxLength: 700,
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "ShippingFullName",
            table: "Checkouts",
            type: "nvarchar(100)",
            maxLength: 100,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(700)",
            oldMaxLength: 700,
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "ShippingCity",
            table: "Checkouts",
            type: "nvarchar(60)",
            maxLength: 60,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(400)",
            oldMaxLength: 400,
            oldNullable: true);
    }
}
