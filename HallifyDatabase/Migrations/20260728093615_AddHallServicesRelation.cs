using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HallifyDatabase.Migrations
{
    /// <inheritdoc />
    public partial class AddHallServicesRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "public",
                table: "HallServices",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<Guid>(
                name: "HallId",
                schema: "public",
                table: "HallServices",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "public",
                table: "Halls",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateIndex(
                name: "IX_HallServices_HallId",
                schema: "public",
                table: "HallServices",
                column: "HallId");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HallServices_Halls_HallId",
                schema: "public",
                table: "HallServices");

            migrationBuilder.DropIndex(
                name: "IX_HallServices_HallId",
                schema: "public",
                table: "HallServices");

            migrationBuilder.DropColumn(
                name: "HallId",
                schema: "public",
                table: "HallServices");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "public",
                table: "HallServices",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "public",
                table: "Halls",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);
        }
    }
}
