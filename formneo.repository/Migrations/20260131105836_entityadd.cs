using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace formneo.repository.Migrations
{
    /// <inheritdoc />
    public partial class entityadd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FormEntities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EntityName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    EntityDescription = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    TableName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    SchemaName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    NamespacePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ClassName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    AllowCreate = table.Column<bool>(type: "boolean", nullable: false),
                    AllowRead = table.Column<bool>(type: "boolean", nullable: false),
                    AllowUpdate = table.Column<bool>(type: "boolean", nullable: false),
                    AllowDelete = table.Column<bool>(type: "boolean", nullable: false),
                    ApiEndpoint = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DisplayField = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    OrderByField = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ParentEntityId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_FormEntities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FormEntities_Clients_MainClientId",
                        column: x => x.MainClientId,
                        principalTable: "Clients",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FormEntities_FormEntities_ParentEntityId",
                        column: x => x.ParentEntityId,
                        principalTable: "FormEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FormEntityFieldTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TypeName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TypeDescription = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CSharpType = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TypeScriptType = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    SqlServerType = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DefaultComponentType = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Category = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsSystemType = table.Column<bool>(type: "boolean", nullable: false),
                    ValidationOptions = table.Column<string>(type: "text", nullable: true),
                    ComponentOptions = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_FormEntityFieldTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FormEntityFieldTypes_Clients_MainClientId",
                        column: x => x.MainClientId,
                        principalTable: "Clients",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "FormEntityRelations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FormId = table.Column<Guid>(type: "uuid", nullable: false),
                    FormEntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    RelationName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    RelationDescription = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    RelationType = table.Column<int>(type: "integer", nullable: false),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CascadeDelete = table.Column<bool>(type: "boolean", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    ParentRelationId = table.Column<Guid>(type: "uuid", nullable: true),
                    FormDataPath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    FormId1 = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_FormEntityRelations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FormEntityRelations_Clients_MainClientId",
                        column: x => x.MainClientId,
                        principalTable: "Clients",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FormEntityRelations_FormEntities_FormEntityId",
                        column: x => x.FormEntityId,
                        principalTable: "FormEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FormEntityRelations_FormEntityRelations_ParentRelationId",
                        column: x => x.ParentRelationId,
                        principalTable: "FormEntityRelations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FormEntityRelations_Form_FormId",
                        column: x => x.FormId,
                        principalTable: "Form",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FormEntityRelations_Form_FormId1",
                        column: x => x.FormId1,
                        principalTable: "Form",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "FormEntityFields",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FormEntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    FieldName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    FieldDescription = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    FieldTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    ColumnName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PropertyName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false),
                    IsUnique = table.Column<bool>(type: "boolean", nullable: false),
                    IsIndexed = table.Column<bool>(type: "boolean", nullable: false),
                    IsNullable = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    MaxLength = table.Column<int>(type: "integer", nullable: true),
                    MinLength = table.Column<int>(type: "integer", nullable: true),
                    MinValue = table.Column<decimal>(type: "numeric", nullable: true),
                    MaxValue = table.Column<decimal>(type: "numeric", nullable: true),
                    DefaultValue = table.Column<string>(type: "text", nullable: true),
                    RegexPattern = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    RegexErrorMessage = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    DisplayLabel = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Placeholder = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    HelpText = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    RelatedEntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    LookupDisplayField = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LookupValueField = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CustomValidationRules = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_FormEntityFields", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FormEntityFields_Clients_MainClientId",
                        column: x => x.MainClientId,
                        principalTable: "Clients",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FormEntityFields_FormEntities_FormEntityId",
                        column: x => x.FormEntityId,
                        principalTable: "FormEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FormEntityFields_FormEntities_RelatedEntityId",
                        column: x => x.RelatedEntityId,
                        principalTable: "FormEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FormEntityFields_FormEntityFieldTypes_FieldTypeId",
                        column: x => x.FieldTypeId,
                        principalTable: "FormEntityFieldTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FormFieldMappings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FormId = table.Column<Guid>(type: "uuid", nullable: false),
                    FormEntityFieldId = table.Column<Guid>(type: "uuid", nullable: false),
                    FormEntityRelationId = table.Column<Guid>(type: "uuid", nullable: true),
                    FormElementId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    FormFieldName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    FormComponentType = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsReadOnly = table.Column<bool>(type: "boolean", nullable: false),
                    IsAutoMapped = table.Column<bool>(type: "boolean", nullable: false),
                    TransformRules = table.Column<string>(type: "text", nullable: true),
                    ValidationOverride = table.Column<string>(type: "text", nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    MappingNotes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    FormId1 = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_FormFieldMappings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FormFieldMappings_Clients_MainClientId",
                        column: x => x.MainClientId,
                        principalTable: "Clients",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FormFieldMappings_FormEntityFields_FormEntityFieldId",
                        column: x => x.FormEntityFieldId,
                        principalTable: "FormEntityFields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FormFieldMappings_FormEntityRelations_FormEntityRelationId",
                        column: x => x.FormEntityRelationId,
                        principalTable: "FormEntityRelations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FormFieldMappings_Form_FormId",
                        column: x => x.FormId,
                        principalTable: "Form",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FormFieldMappings_Form_FormId1",
                        column: x => x.FormId1,
                        principalTable: "Form",
                        principalColumn: "Id");
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_FormEntities_EntityName",
                table: "FormEntities",
                column: "EntityName");

            migrationBuilder.CreateIndex(
                name: "IX_FormEntities_MainClientId",
                table: "FormEntities",
                column: "MainClientId");

            migrationBuilder.CreateIndex(
                name: "IX_FormEntities_ParentEntityId",
                table: "FormEntities",
                column: "ParentEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_FormEntityFields_FieldTypeId",
                table: "FormEntityFields",
                column: "FieldTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_FormEntityFields_FormEntityId_FieldName",
                table: "FormEntityFields",
                columns: new[] { "FormEntityId", "FieldName" });

            migrationBuilder.CreateIndex(
                name: "IX_FormEntityFields_MainClientId",
                table: "FormEntityFields",
                column: "MainClientId");

            migrationBuilder.CreateIndex(
                name: "IX_FormEntityFields_RelatedEntityId",
                table: "FormEntityFields",
                column: "RelatedEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_FormEntityFieldTypes_MainClientId",
                table: "FormEntityFieldTypes",
                column: "MainClientId");

            migrationBuilder.CreateIndex(
                name: "IX_FormEntityFieldTypes_TypeName",
                table: "FormEntityFieldTypes",
                column: "TypeName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FormEntityRelations_FormEntityId",
                table: "FormEntityRelations",
                column: "FormEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_FormEntityRelations_FormId_FormEntityId",
                table: "FormEntityRelations",
                columns: new[] { "FormId", "FormEntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_FormEntityRelations_FormId1",
                table: "FormEntityRelations",
                column: "FormId1");

            migrationBuilder.CreateIndex(
                name: "IX_FormEntityRelations_MainClientId",
                table: "FormEntityRelations",
                column: "MainClientId");

            migrationBuilder.CreateIndex(
                name: "IX_FormEntityRelations_ParentRelationId",
                table: "FormEntityRelations",
                column: "ParentRelationId");

            migrationBuilder.CreateIndex(
                name: "IX_FormFieldMappings_FormElementId",
                table: "FormFieldMappings",
                column: "FormElementId");

            migrationBuilder.CreateIndex(
                name: "IX_FormFieldMappings_FormEntityFieldId",
                table: "FormFieldMappings",
                column: "FormEntityFieldId");

            migrationBuilder.CreateIndex(
                name: "IX_FormFieldMappings_FormEntityRelationId",
                table: "FormFieldMappings",
                column: "FormEntityRelationId");

            migrationBuilder.CreateIndex(
                name: "IX_FormFieldMappings_FormId_FormFieldName",
                table: "FormFieldMappings",
                columns: new[] { "FormId", "FormFieldName" });

            migrationBuilder.CreateIndex(
                name: "IX_FormFieldMappings_FormId1",
                table: "FormFieldMappings",
                column: "FormId1");

            migrationBuilder.CreateIndex(
                name: "IX_FormFieldMappings_MainClientId",
                table: "FormFieldMappings",
                column: "MainClientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FormFieldMappings");

            migrationBuilder.DropTable(
                name: "FormEntityFields");

            migrationBuilder.DropTable(
                name: "FormEntityRelations");

            migrationBuilder.DropTable(
                name: "FormEntityFieldTypes");

            migrationBuilder.DropTable(
                name: "FormEntities");

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
        }
    }
}
