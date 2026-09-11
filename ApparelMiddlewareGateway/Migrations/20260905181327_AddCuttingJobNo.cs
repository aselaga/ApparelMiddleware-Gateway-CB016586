using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApparelMiddlewareGateway.Migrations
{
    /// <inheritdoc />
    public partial class AddCuttingJobNo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CuttingJobNo",
                table: "SpreaderTelemetryRecords",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CuttingJobNo",
                table: "SpreaderTelemetryRecords");
        }
    }
}
