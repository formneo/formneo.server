using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace formneo.repository.Migrations
{
    /// <inheritdoc />
    public partial class organizatiın4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_OrgUnits_OrgUnitId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Positions_PositionId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_WorkCompany_WorkCompanyId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_OrgUnitId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_PositionId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_WorkCompanyId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "OrgUnitId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "PositionId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "SAPDepartmentText",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "SAPPositionText",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "WorkCompanyId",
                table: "AspNetUsers",
                newName: "PositionsId");

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

            migrationBuilder.CreateIndex(
                name: "IX_WorkCompany_UserAppId",
                table: "WorkCompany",
                column: "UserAppId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_PositionsId",
                table: "AspNetUsers",
                column: "PositionsId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Positions_PositionsId",
                table: "AspNetUsers",
                column: "PositionsId",
                principalTable: "Positions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkCompany_AspNetUsers_UserAppId",
                table: "WorkCompany",
                column: "UserAppId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Positions_PositionsId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkCompany_AspNetUsers_UserAppId",
                table: "WorkCompany");

            migrationBuilder.DropIndex(
                name: "IX_WorkCompany_UserAppId",
                table: "WorkCompany");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_PositionsId",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "PositionsId",
                table: "AspNetUsers",
                newName: "WorkCompanyId");

            migrationBuilder.AddColumn<Guid>(
                name: "OrgUnitId",
                table: "AspNetUsers",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PositionId",
                table: "AspNetUsers",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SAPDepartmentText",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SAPPositionText",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Clients",
                keyColumn: "Id",
                keyValue: new Guid("77df6fbd-4160-4cea-8f24-96564b54e5ac"),
                column: "CreatedDate",
                value: new DateTime(2026, 1, 18, 17, 45, 45, 747, DateTimeKind.Utc).AddTicks(6070));

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("1bf2fc2e-0e25-46a8-aa96-8f1480331b5b"),
                column: "CreatedDate",
                value: new DateTime(2026, 1, 18, 17, 45, 45, 747, DateTimeKind.Utc).AddTicks(5210));

            migrationBuilder.UpdateData(
                table: "Plant",
                keyColumn: "Id",
                keyValue: new Guid("0779dd43-6047-400d-968d-e6f1b0c3b286"),
                column: "CreatedDate",
                value: new DateTime(2026, 1, 18, 17, 45, 45, 747, DateTimeKind.Utc).AddTicks(6150));

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_OrgUnitId",
                table: "AspNetUsers",
                column: "OrgUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_PositionId",
                table: "AspNetUsers",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_WorkCompanyId",
                table: "AspNetUsers",
                column: "WorkCompanyId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_OrgUnits_OrgUnitId",
                table: "AspNetUsers",
                column: "OrgUnitId",
                principalTable: "OrgUnits",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Positions_PositionId",
                table: "AspNetUsers",
                column: "PositionId",
                principalTable: "Positions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_WorkCompany_WorkCompanyId",
                table: "AspNetUsers",
                column: "WorkCompanyId",
                principalTable: "WorkCompany",
                principalColumn: "Id");
        }
    }
}
