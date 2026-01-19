using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace formneo.repository.Migrations
{
    /// <inheritdoc />
    public partial class organization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ProcessTime",
                table: "PCTrack",
                newName: "ProWorkFlowDefinationcessTime");

            migrationBuilder.AddColumn<Guid>(
                name: "ParentPositionId",
                table: "Positions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ManagerId",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OrgUnitId",
                table: "AspNetUsers",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "OrgUnits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    ParentOrgUnitId = table.Column<Guid>(type: "uuid", nullable: true),
                    ManagerId = table.Column<string>(type: "text", nullable: true),
                    WorkCompanyId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_OrgUnits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrgUnits_AspNetUsers_ManagerId",
                        column: x => x.ManagerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OrgUnits_Clients_MainClientId",
                        column: x => x.MainClientId,
                        principalTable: "Clients",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OrgUnits_OrgUnits_ParentOrgUnitId",
                        column: x => x.ParentOrgUnitId,
                        principalTable: "OrgUnits",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OrgUnits_WorkCompany_WorkCompanyId",
                        column: x => x.WorkCompanyId,
                        principalTable: "WorkCompany",
                        principalColumn: "Id");
                });

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
                name: "IX_Positions_ParentPositionId",
                table: "Positions",
                column: "ParentPositionId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_ManagerId",
                table: "AspNetUsers",
                column: "ManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_OrgUnitId",
                table: "AspNetUsers",
                column: "OrgUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_OrgUnits_MainClientId",
                table: "OrgUnits",
                column: "MainClientId");

            migrationBuilder.CreateIndex(
                name: "IX_OrgUnits_ManagerId",
                table: "OrgUnits",
                column: "ManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_OrgUnits_ParentOrgUnitId",
                table: "OrgUnits",
                column: "ParentOrgUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_OrgUnits_WorkCompanyId",
                table: "OrgUnits",
                column: "WorkCompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_AspNetUsers_ManagerId",
                table: "AspNetUsers",
                column: "ManagerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_OrgUnits_OrgUnitId",
                table: "AspNetUsers",
                column: "OrgUnitId",
                principalTable: "OrgUnits",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Positions_Positions_ParentPositionId",
                table: "Positions",
                column: "ParentPositionId",
                principalTable: "Positions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_AspNetUsers_ManagerId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_OrgUnits_OrgUnitId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_Positions_Positions_ParentPositionId",
                table: "Positions");

            migrationBuilder.DropTable(
                name: "OrgUnits");

            migrationBuilder.DropIndex(
                name: "IX_Positions_ParentPositionId",
                table: "Positions");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_ManagerId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_OrgUnitId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ParentPositionId",
                table: "Positions");

            migrationBuilder.DropColumn(
                name: "ManagerId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "OrgUnitId",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "ProWorkFlowDefinationcessTime",
                table: "PCTrack",
                newName: "ProcessTime");

            migrationBuilder.UpdateData(
                table: "Clients",
                keyColumn: "Id",
                keyValue: new Guid("77df6fbd-4160-4cea-8f24-96564b54e5ac"),
                column: "CreatedDate",
                value: new DateTime(2025, 12, 20, 13, 54, 21, 969, DateTimeKind.Utc).AddTicks(9320));

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("1bf2fc2e-0e25-46a8-aa96-8f1480331b5b"),
                column: "CreatedDate",
                value: new DateTime(2025, 12, 20, 13, 54, 21, 969, DateTimeKind.Utc).AddTicks(8220));

            migrationBuilder.UpdateData(
                table: "Plant",
                keyColumn: "Id",
                keyValue: new Guid("0779dd43-6047-400d-968d-e6f1b0c3b286"),
                column: "CreatedDate",
                value: new DateTime(2025, 12, 20, 13, 54, 21, 969, DateTimeKind.Utc).AddTicks(9400));
        }
    }
}
