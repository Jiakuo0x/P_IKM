using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace App.Migrations
{
    /// <inheritdoc />
    public partial class ud01 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ParameterMappings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TemplateMappingId = table.Column<int>(type: "integer", nullable: false),
                    DocuSignDataType = table.Column<int>(type: "integer", nullable: false),
                    DocuSignDataName = table.Column<string>(type: "text", nullable: false),
                    BestSignDataName = table.Column<string>(type: "text", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp(0) with time zone", precision: 0, nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "timestamp(0) with time zone", precision: 0, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParameterMappings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TaskLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TaskId = table.Column<int>(type: "integer", nullable: false),
                    Step = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Log = table.Column<string>(type: "text", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp(0) with time zone", precision: 0, nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "timestamp(0) with time zone", precision: 0, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tasks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DocuSignEnvelopeId = table.Column<string>(type: "text", nullable: false),
                    BestSignContractId = table.Column<string>(type: "text", nullable: true),
                    CurrentStep = table.Column<int>(type: "integer", nullable: false),
                    Counter = table.Column<int>(type: "integer", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp(0) with time zone", precision: 0, nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "timestamp(0) with time zone", precision: 0, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tasks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TemplateMappings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DocuSignTemplateId = table.Column<string>(type: "text", nullable: false),
                    BestSignTemplateId = table.Column<string>(type: "text", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp(0) with time zone", precision: 0, nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "timestamp(0) with time zone", precision: 0, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TemplateMappings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ParameterMappings_TemplateMappingId",
                table: "ParameterMappings",
                column: "TemplateMappingId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskLogs_TaskId",
                table: "TaskLogs",
                column: "TaskId");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_DocuSignEnvelopeId",
                table: "Tasks",
                column: "DocuSignEnvelopeId");

            migrationBuilder.CreateIndex(
                name: "IX_TemplateMappings_DocuSignTemplateId",
                table: "TemplateMappings",
                column: "DocuSignTemplateId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ParameterMappings");

            migrationBuilder.DropTable(
                name: "TaskLogs");

            migrationBuilder.DropTable(
                name: "Tasks");

            migrationBuilder.DropTable(
                name: "TemplateMappings");
        }
    }
}
