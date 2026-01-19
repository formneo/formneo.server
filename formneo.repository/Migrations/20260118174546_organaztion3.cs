using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace formneo.repository.Migrations
{
    /// <inheritdoc />
    public partial class organaztion3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_AspNetUsers_ManagerId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_ManagerId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "mainManagerUserAppId",
                table: "UserTenants");

            migrationBuilder.DropColumn(
                name: "manager1",
                table: "UserTenants");

            migrationBuilder.DropColumn(
                name: "manager2",
                table: "UserTenants");

            migrationBuilder.DropColumn(
                name: "Department",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ManagerId",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<string>(
                name: "ManagerId",
                table: "TicketDepartment",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "EmployeeAssignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    OrgUnitId = table.Column<Guid>(type: "uuid", nullable: true),
                    PositionId = table.Column<Guid>(type: "uuid", nullable: true),
                    ManagerId = table.Column<string>(type: "text", nullable: true),
                    StartDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    AssignmentType = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    MainClientId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false),
                    UniqNumber = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeAssignments_AspNetUsers_ManagerId",
                        column: x => x.ManagerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EmployeeAssignments_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EmployeeAssignments_Clients_MainClientId",
                        column: x => x.MainClientId,
                        principalTable: "Clients",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EmployeeAssignments_OrgUnits_OrgUnitId",
                        column: x => x.OrgUnitId,
                        principalTable: "OrgUnits",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EmployeeAssignments_Positions_PositionId",
                        column: x => x.PositionId,
                        principalTable: "Positions",
                        principalColumn: "Id");
                });

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
                name: "IX_EmployeeAssignments_MainClientId",
                table: "EmployeeAssignments",
                column: "MainClientId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeAssignments_ManagerId",
                table: "EmployeeAssignments",
                column: "ManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeAssignments_OrgUnitId_EndDate",
                table: "EmployeeAssignments",
                columns: new[] { "OrgUnitId", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeAssignments_PositionId",
                table: "EmployeeAssignments",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeAssignments_StartDate_EndDate",
                table: "EmployeeAssignments",
                columns: new[] { "StartDate", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeAssignments_UserId",
                table: "EmployeeAssignments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeAssignments_UserId_EndDate",
                table: "EmployeeAssignments",
                columns: new[] { "UserId", "EndDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmployeeAssignments");

            migrationBuilder.AddColumn<Guid>(
                name: "mainManagerUserAppId",
                table: "UserTenants",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "manager1",
                table: "UserTenants",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "manager2",
                table: "UserTenants",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ManagerId",
                table: "TicketDepartment",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "Department",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ManagerId",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

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

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_ManagerId",
                table: "AspNetUsers",
                column: "ManagerId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_AspNetUsers_ManagerId",
                table: "AspNetUsers",
                column: "ManagerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
