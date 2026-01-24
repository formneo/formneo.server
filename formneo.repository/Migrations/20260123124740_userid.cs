using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace formneo.repository.Migrations
{
    /// <inheritdoc />
    public partial class userid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FormUser",
                table: "FormItems");

            migrationBuilder.DropColumn(
                name: "ApproveUser",
                table: "ApproveItems");

            migrationBuilder.RenameColumn(
                name: "ApprovedUser_Runtime",
                table: "ApproveItems",
                newName: "ApprovedUser_RuntimeId");

            migrationBuilder.AddColumn<string>(
                name: "FormUserId",
                table: "FormItems",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ApproveUserId",
                table: "ApproveItems",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Clients",
                keyColumn: "Id",
                keyValue: new Guid("77df6fbd-4160-4cea-8f24-96564b54e5ac"),
                column: "CreatedDate",
                value: new DateTime(2026, 1, 23, 12, 47, 40, 151, DateTimeKind.Utc).AddTicks(8940));

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("1bf2fc2e-0e25-46a8-aa96-8f1480331b5b"),
                column: "CreatedDate",
                value: new DateTime(2026, 1, 23, 12, 47, 40, 151, DateTimeKind.Utc).AddTicks(8130));

            migrationBuilder.UpdateData(
                table: "Plant",
                keyColumn: "Id",
                keyValue: new Guid("0779dd43-6047-400d-968d-e6f1b0c3b286"),
                column: "CreatedDate",
                value: new DateTime(2026, 1, 23, 12, 47, 40, 151, DateTimeKind.Utc).AddTicks(9020));

            migrationBuilder.CreateIndex(
                name: "IX_FormItems_FormUserId",
                table: "FormItems",
                column: "FormUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ApproveItems_ApprovedUser_RuntimeId",
                table: "ApproveItems",
                column: "ApprovedUser_RuntimeId");

            migrationBuilder.CreateIndex(
                name: "IX_ApproveItems_ApproveUserId",
                table: "ApproveItems",
                column: "ApproveUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ApproveItems_AspNetUsers_ApproveUserId",
                table: "ApproveItems",
                column: "ApproveUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ApproveItems_AspNetUsers_ApprovedUser_RuntimeId",
                table: "ApproveItems",
                column: "ApprovedUser_RuntimeId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FormItems_AspNetUsers_FormUserId",
                table: "FormItems",
                column: "FormUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApproveItems_AspNetUsers_ApproveUserId",
                table: "ApproveItems");

            migrationBuilder.DropForeignKey(
                name: "FK_ApproveItems_AspNetUsers_ApprovedUser_RuntimeId",
                table: "ApproveItems");

            migrationBuilder.DropForeignKey(
                name: "FK_FormItems_AspNetUsers_FormUserId",
                table: "FormItems");

            migrationBuilder.DropIndex(
                name: "IX_FormItems_FormUserId",
                table: "FormItems");

            migrationBuilder.DropIndex(
                name: "IX_ApproveItems_ApprovedUser_RuntimeId",
                table: "ApproveItems");

            migrationBuilder.DropIndex(
                name: "IX_ApproveItems_ApproveUserId",
                table: "ApproveItems");

            migrationBuilder.DropColumn(
                name: "FormUserId",
                table: "FormItems");

            migrationBuilder.DropColumn(
                name: "ApproveUserId",
                table: "ApproveItems");

            migrationBuilder.RenameColumn(
                name: "ApprovedUser_RuntimeId",
                table: "ApproveItems",
                newName: "ApprovedUser_Runtime");

            migrationBuilder.AddColumn<string>(
                name: "FormUser",
                table: "FormItems",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApproveUser",
                table: "ApproveItems",
                type: "text",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Clients",
                keyColumn: "Id",
                keyValue: new Guid("77df6fbd-4160-4cea-8f24-96564b54e5ac"),
                column: "CreatedDate",
                value: new DateTime(2026, 1, 19, 8, 28, 28, 116, DateTimeKind.Utc).AddTicks(3450));

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("1bf2fc2e-0e25-46a8-aa96-8f1480331b5b"),
                column: "CreatedDate",
                value: new DateTime(2026, 1, 19, 8, 28, 28, 116, DateTimeKind.Utc).AddTicks(2620));

            migrationBuilder.UpdateData(
                table: "Plant",
                keyColumn: "Id",
                keyValue: new Guid("0779dd43-6047-400d-968d-e6f1b0c3b286"),
                column: "CreatedDate",
                value: new DateTime(2026, 1, 19, 8, 28, 28, 116, DateTimeKind.Utc).AddTicks(3540));
        }
    }
}
