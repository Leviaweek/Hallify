using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace HallifyDatabase.Migrations
{
    /// <inheritdoc />
    public partial class AddStartData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Halls_HallId",
                schema: "public",
                table: "Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_BookingServices_Bookings_BookingId",
                schema: "public",
                table: "BookingServices");

            migrationBuilder.DropForeignKey(
                name: "FK_BookingServices_HallServices_HallServiceId",
                schema: "public",
                table: "BookingServices");

            migrationBuilder.DropForeignKey(
                name: "FK_HallServices_Halls_HallId",
                schema: "public",
                table: "HallServices");

            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                schema: "public",
                table: "HallServices",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<decimal>(
                name: "HourlyRate",
                schema: "public",
                table: "Halls",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<decimal>(
                name: "PriceAtBooking",
                schema: "public",
                table: "BookingServices",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalPrice",
                schema: "public",
                table: "Bookings",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.InsertData(
                schema: "public",
                table: "Halls",
                columns: new[] { "Id", "Capacity", "HourlyRate", "IsDeleted", "Name" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), 50, 2000m, false, "Зал А" },
                    { new Guid("22222222-2222-2222-2222-222222222222"), 100, 3500m, false, "Зал B" },
                    { new Guid("33333333-3333-3333-3333-333333333333"), 30, 1500m, false, "Зал C" }
                });

            migrationBuilder.InsertData(
                schema: "public",
                table: "HallServices",
                columns: new[] { "Id", "HallId", "IsDeleted", "Name", "Price" },
                values: new object[,]
                {
                    { new Guid("a1111111-1111-1111-1111-111111111111"), new Guid("11111111-1111-1111-1111-111111111111"), false, "Проєктор", 500m },
                    { new Guid("a2222222-2222-2222-2222-222222222222"), new Guid("11111111-1111-1111-1111-111111111111"), false, "Wi-Fi", 300m },
                    { new Guid("a3333333-3333-3333-3333-333333333333"), new Guid("11111111-1111-1111-1111-111111111111"), false, "Звук", 700m },
                    { new Guid("b1111111-1111-1111-1111-111111111111"), new Guid("22222222-2222-2222-2222-222222222222"), false, "Проєктор", 500m },
                    { new Guid("b2222222-2222-2222-2222-222222222222"), new Guid("22222222-2222-2222-2222-222222222222"), false, "Wi-Fi", 300m },
                    { new Guid("b3333333-3333-3333-3333-333333333333"), new Guid("22222222-2222-2222-2222-222222222222"), false, "Звук", 700m },
                    { new Guid("c1111111-1111-1111-1111-111111111111"), new Guid("33333333-3333-3333-3333-333333333333"), false, "Проєктор", 500m },
                    { new Guid("c2222222-2222-2222-2222-222222222222"), new Guid("33333333-3333-3333-3333-333333333333"), false, "Wi-Fi", 300m },
                    { new Guid("c3333333-3333-3333-3333-333333333333"), new Guid("33333333-3333-3333-3333-333333333333"), false, "Звук", 700m }
                });

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Halls_HallId",
                schema: "public",
                table: "Bookings",
                column: "HallId",
                principalSchema: "public",
                principalTable: "Halls",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BookingServices_Bookings_BookingId",
                schema: "public",
                table: "BookingServices",
                column: "BookingId",
                principalSchema: "public",
                principalTable: "Bookings",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BookingServices_HallServices_HallServiceId",
                schema: "public",
                table: "BookingServices",
                column: "HallServiceId",
                principalSchema: "public",
                principalTable: "HallServices",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_HallServices_Halls_HallId",
                schema: "public",
                table: "HallServices",
                column: "HallId",
                principalSchema: "public",
                principalTable: "Halls",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Halls_HallId",
                schema: "public",
                table: "Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_BookingServices_Bookings_BookingId",
                schema: "public",
                table: "BookingServices");

            migrationBuilder.DropForeignKey(
                name: "FK_BookingServices_HallServices_HallServiceId",
                schema: "public",
                table: "BookingServices");

            migrationBuilder.DropForeignKey(
                name: "FK_HallServices_Halls_HallId",
                schema: "public",
                table: "HallServices");

            migrationBuilder.DeleteData(
                schema: "public",
                table: "HallServices",
                keyColumn: "Id",
                keyValue: new Guid("a1111111-1111-1111-1111-111111111111"));

            migrationBuilder.DeleteData(
                schema: "public",
                table: "HallServices",
                keyColumn: "Id",
                keyValue: new Guid("a2222222-2222-2222-2222-222222222222"));

            migrationBuilder.DeleteData(
                schema: "public",
                table: "HallServices",
                keyColumn: "Id",
                keyValue: new Guid("a3333333-3333-3333-3333-333333333333"));

            migrationBuilder.DeleteData(
                schema: "public",
                table: "HallServices",
                keyColumn: "Id",
                keyValue: new Guid("b1111111-1111-1111-1111-111111111111"));

            migrationBuilder.DeleteData(
                schema: "public",
                table: "HallServices",
                keyColumn: "Id",
                keyValue: new Guid("b2222222-2222-2222-2222-222222222222"));

            migrationBuilder.DeleteData(
                schema: "public",
                table: "HallServices",
                keyColumn: "Id",
                keyValue: new Guid("b3333333-3333-3333-3333-333333333333"));

            migrationBuilder.DeleteData(
                schema: "public",
                table: "HallServices",
                keyColumn: "Id",
                keyValue: new Guid("c1111111-1111-1111-1111-111111111111"));

            migrationBuilder.DeleteData(
                schema: "public",
                table: "HallServices",
                keyColumn: "Id",
                keyValue: new Guid("c2222222-2222-2222-2222-222222222222"));

            migrationBuilder.DeleteData(
                schema: "public",
                table: "HallServices",
                keyColumn: "Id",
                keyValue: new Guid("c3333333-3333-3333-3333-333333333333"));

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Halls",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Halls",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"));

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Halls",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"));

            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                schema: "public",
                table: "HallServices",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "HourlyRate",
                schema: "public",
                table: "Halls",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "PriceAtBooking",
                schema: "public",
                table: "BookingServices",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalPrice",
                schema: "public",
                table: "Bookings",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Halls_HallId",
                schema: "public",
                table: "Bookings",
                column: "HallId",
                principalSchema: "public",
                principalTable: "Halls",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BookingServices_Bookings_BookingId",
                schema: "public",
                table: "BookingServices",
                column: "BookingId",
                principalSchema: "public",
                principalTable: "Bookings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BookingServices_HallServices_HallServiceId",
                schema: "public",
                table: "BookingServices",
                column: "HallServiceId",
                principalSchema: "public",
                principalTable: "HallServices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_HallServices_Halls_HallId",
                schema: "public",
                table: "HallServices",
                column: "HallId",
                principalSchema: "public",
                principalTable: "Halls",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
