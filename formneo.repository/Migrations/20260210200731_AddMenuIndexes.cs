using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace formneo.repository.Migrations
{
    /// <inheritdoc />
    public partial class AddMenuIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Clients",
                keyColumn: "Id",
                keyValue: new Guid("77df6fbd-4160-4cea-8f24-96564b54e5ac"),
                column: "CreatedDate",
                value: new DateTime(2026, 2, 10, 20, 7, 29, 726, DateTimeKind.Utc).AddTicks(2160));

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("1bf2fc2e-0e25-46a8-aa96-8f1480331b5b"),
                column: "CreatedDate",
                value: new DateTime(2026, 2, 10, 20, 7, 29, 726, DateTimeKind.Utc).AddTicks(1350));

            migrationBuilder.UpdateData(
                table: "Plant",
                keyColumn: "Id",
                keyValue: new Guid("0779dd43-6047-400d-968d-e6f1b0c3b286"),
                column: "CreatedDate",
                value: new DateTime(2026, 2, 10, 20, 7, 29, 726, DateTimeKind.Utc).AddTicks(2250));

            migrationBuilder.CreateIndex(
                name: "IX_Menus_IsDelete_IsTenantOnly",
                table: "Menus",
                columns: new[] { "IsDelete", "IsTenantOnly" });

            migrationBuilder.CreateIndex(
                name: "IX_Menus_IsDelete_ParentMenuId_Order",
                table: "Menus",
                columns: new[] { "IsDelete", "ParentMenuId", "Order" });

            migrationBuilder.CreateIndex(
                name: "IX_Menus_MenuCode",
                table: "Menus",
                column: "MenuCode");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeAssignment_ActiveLookup",
                table: "EmployeeAssignments",
                columns: new[] { "UserId", "AssignmentType", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeAssignments_OrgUnitId",
                table: "EmployeeAssignments",
                column: "OrgUnitId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Menus_IsDelete_IsTenantOnly",
                table: "Menus");

            migrationBuilder.DropIndex(
                name: "IX_Menus_IsDelete_ParentMenuId_Order",
                table: "Menus");

            migrationBuilder.DropIndex(
                name: "IX_Menus_MenuCode",
                table: "Menus");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeAssignment_ActiveLookup",
                table: "EmployeeAssignments");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeAssignments_OrgUnitId",
                table: "EmployeeAssignments");

            migrationBuilder.UpdateData(
                table: "Clients",
                keyColumn: "Id",
                keyValue: new Guid("77df6fbd-4160-4cea-8f24-96564b54e5ac"),
                column: "CreatedDate",
                value: new DateTime(2026, 1, 31, 10, 58, 35, 730, DateTimeKind.Utc).AddTicks(1100));

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("1bf2fc2e-0e25-46a8-aa96-8f1480331b5b"),
                column: "CreatedDate",
                value: new DateTime(2026, 1, 31, 10, 58, 35, 730, DateTimeKind.Utc).AddTicks(340));

            migrationBuilder.UpdateData(
                table: "Plant",
                keyColumn: "Id",
                keyValue: new Guid("0779dd43-6047-400d-968d-e6f1b0c3b286"),
                column: "CreatedDate",
                value: new DateTime(2026, 1, 31, 10, 58, 35, 730, DateTimeKind.Utc).AddTicks(1170));
        }
    }
}
