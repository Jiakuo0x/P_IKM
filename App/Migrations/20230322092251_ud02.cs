using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace App.Migrations
{
    /// <inheritdoc />
    public partial class ud02 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ParameterMappings");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "TaskLogs");

            migrationBuilder.AddColumn<string>(
                name: "ParameterMappingsString",
                table: "TemplateMappings",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ParameterMappingsString",
                table: "TemplateMappings");

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "TaskLogs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ParameterMappings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BestSignDataName = table.Column<string>(type: "text", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp(0) with time zone", precision: 0, nullable: false),
                    DocuSignDataName = table.Column<string>(type: "text", nullable: false),
                    DocuSignDataType = table.Column<int>(type: "integer", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "timestamp(0) with time zone", precision: 0, nullable: false),
                    TemplateMappingId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParameterMappings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ParameterMappings_TemplateMappingId",
                table: "ParameterMappings",
                column: "TemplateMappingId");
        }
    }
}
