using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApparelMiddlewareGateway.Migrations
{
    /// <inheritdoc />
    public partial class AddTelemetryUniqueRollIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "RollID",
                table: "SpreaderTelemetryRecords",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "OperatorId",
                table: "SpreaderTelemetryRecords",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "CuttingJobNo",
                table: "SpreaderTelemetryRecords",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_SpreaderTelemetryRecords_CuttingJobNo_RollID",
                table: "SpreaderTelemetryRecords",
                columns: new[] { "CuttingJobNo", "RollID" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SpreaderTelemetryRecords_CuttingJobNo_RollID",
                table: "SpreaderTelemetryRecords");

            migrationBuilder.AlterColumn<string>(
                name: "RollID",
                table: "SpreaderTelemetryRecords",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64);

            migrationBuilder.AlterColumn<string>(
                name: "OperatorId",
                table: "SpreaderTelemetryRecords",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64);

            migrationBuilder.AlterColumn<string>(
                name: "CuttingJobNo",
                table: "SpreaderTelemetryRecords",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64);
        }
    }
}
