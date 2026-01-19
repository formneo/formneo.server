using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace formneo.repository.Migrations
{
    /// <inheritdoc />
    public partial class organization2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrgUnits_WorkCompany_WorkCompanyId",
                table: "OrgUnits");

            migrationBuilder.DropForeignKey(
                name: "FK_Positions_WorkCompany_CustomerRefId",
                table: "Positions");

            migrationBuilder.DropIndex(
                name: "IX_Positions_CustomerRefId",
                table: "Positions");

            migrationBuilder.DropIndex(
                name: "IX_OrgUnits_WorkCompanyId",
                table: "OrgUnits");

            migrationBuilder.DropColumn(
                name: "CustomerRefId",
                table: "Positions");

            migrationBuilder.DropColumn(
                name: "WorkCompanyId",
                table: "OrgUnits");

            migrationBuilder.UpdateData(
                table: "Clients",
                keyColumn: "Id",
                keyValue: new Guid("77df6fbd-4160-4cea-8f24-96564b54e5ac"),
                column: "CreatedDate",
                value: new DateTime(2026, 1, 17, 17, 43, 44, 9, DateTimeKind.Utc).AddTicks(1570));

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("1bf2fc2e-0e25-46a8-aa96-8f1480331b5b"),
                column: "CreatedDate",
                value: new DateTime(2026, 1, 17, 17, 43, 44, 9, DateTimeKind.Utc).AddTicks(660));

            migrationBuilder.UpdateData(
                table: "Plant",
                keyColumn: "Id",
                keyValue: new Guid("0779dd43-6047-400d-968d-e6f1b0c3b286"),
                column: "CreatedDate",
                value: new DateTime(2026, 1, 17, 17, 43, 44, 9, DateTimeKind.Utc).AddTicks(1650));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CustomerRefId",
                table: "Positions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "WorkCompanyId",
                table: "OrgUnits",
                type: "uuid",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Clients",
                keyColumn: "Id",
                keyValue: new Guid("77df6fbd-4160-4cea-8f24-96564b54e5ac"),
                column: "CreatedDate",
                value: new DateTime(2026, 1, 17, 17, 7, 25, 329, DateTimeKind.Utc).AddTicks(8040));

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("1bf2fc2e-0e25-46a8-aa96-8f1480331b5b"),
                column: "CreatedDate",
                value: new DateTime(2026, 1, 17, 17, 7, 25, 329, DateTimeKind.Utc).AddTicks(7220));

            migrationBuilder.UpdateData(
                table: "Plant",
                keyColumn: "Id",
                keyValue: new Guid("0779dd43-6047-400d-968d-e6f1b0c3b286"),
                column: "CreatedDate",
                value: new DateTime(2026, 1, 17, 17, 7, 25, 329, DateTimeKind.Utc).AddTicks(8130));

            migrationBuilder.CreateIndex(
                name: "IX_Positions_CustomerRefId",
                table: "Positions",
                column: "CustomerRefId");

            migrationBuilder.CreateIndex(
                name: "IX_OrgUnits_WorkCompanyId",
                table: "OrgUnits",
                column: "WorkCompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrgUnits_WorkCompany_WorkCompanyId",
                table: "OrgUnits",
                column: "WorkCompanyId",
                principalTable: "WorkCompany",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Positions_WorkCompany_CustomerRefId",
                table: "Positions",
                column: "CustomerRefId",
                principalTable: "WorkCompany",
                principalColumn: "Id");
        }
    }
}
