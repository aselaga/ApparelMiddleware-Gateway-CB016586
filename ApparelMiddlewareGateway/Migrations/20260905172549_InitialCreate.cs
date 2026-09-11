using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApparelMiddlewareGateway.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SpreaderTelemetryRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RollID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ActualPlyHeight = table.Column<int>(type: "int", nullable: false),
                    EndBit = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    OverlapStart = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    OverlapEnd = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    Damage = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    OperatorId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpreaderTelemetryRecords", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SpreaderTelemetryRecords");
        }
    }
}
