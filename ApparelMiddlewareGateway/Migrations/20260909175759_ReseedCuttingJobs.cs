using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ApparelMiddlewareGateway.Migrations
{
    /// <inheritdoc />
    public partial class ReseedCuttingJobs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Rolls",
                keyColumns: new[] { "CuttingJobNo", "RollID" },
                keyValues: new object[] { "JOB-1001", "R-1045" });

            migrationBuilder.DeleteData(
                table: "Rolls",
                keyColumns: new[] { "CuttingJobNo", "RollID" },
                keyValues: new object[] { "JOB-1001", "R-1046" });

            migrationBuilder.DeleteData(
                table: "Rolls",
                keyColumns: new[] { "CuttingJobNo", "RollID" },
                keyValues: new object[] { "JOB-1001", "R-1047" });

            migrationBuilder.DeleteData(
                table: "Rolls",
                keyColumns: new[] { "CuttingJobNo", "RollID" },
                keyValues: new object[] { "JOB-1002", "R-2050" });

            migrationBuilder.DeleteData(
                table: "Rolls",
                keyColumns: new[] { "CuttingJobNo", "RollID" },
                keyValues: new object[] { "JOB-1002", "R-2051" });

            migrationBuilder.DeleteData(
                table: "CuttingJobs",
                keyColumn: "CuttingJobNo",
                keyValue: "JOB-1001");

            migrationBuilder.DeleteData(
                table: "CuttingJobs",
                keyColumn: "CuttingJobNo",
                keyValue: "JOB-1002");

            migrationBuilder.InsertData(
                table: "CuttingJobs",
                columns: new[] { "CuttingJobNo", "Status", "TargetLine" },
                values: new object[,]
                {
                    { "CJ-9920", "Active", "Spreader_01" },
                    { "CJ-9921", "Active", "Spreader_02" }
                });

            migrationBuilder.InsertData(
                table: "Rolls",
                columns: new[] { "CuttingJobNo", "RollID", "Status" },
                values: new object[,]
                {
                    { "CJ-9920", "R-1045", "Pending" },
                    { "CJ-9920", "R-1046", "Pending" },
                    { "CJ-9920", "R-1047", "Pending" },
                    { "CJ-9921", "R-2050", "Pending" },
                    { "CJ-9921", "R-2051", "Pending" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Rolls",
                keyColumns: new[] { "CuttingJobNo", "RollID" },
                keyValues: new object[] { "CJ-9920", "R-1045" });

            migrationBuilder.DeleteData(
                table: "Rolls",
                keyColumns: new[] { "CuttingJobNo", "RollID" },
                keyValues: new object[] { "CJ-9920", "R-1046" });

            migrationBuilder.DeleteData(
                table: "Rolls",
                keyColumns: new[] { "CuttingJobNo", "RollID" },
                keyValues: new object[] { "CJ-9920", "R-1047" });

            migrationBuilder.DeleteData(
                table: "Rolls",
                keyColumns: new[] { "CuttingJobNo", "RollID" },
                keyValues: new object[] { "CJ-9921", "R-2050" });

            migrationBuilder.DeleteData(
                table: "Rolls",
                keyColumns: new[] { "CuttingJobNo", "RollID" },
                keyValues: new object[] { "CJ-9921", "R-2051" });

            migrationBuilder.DeleteData(
                table: "CuttingJobs",
                keyColumn: "CuttingJobNo",
                keyValue: "CJ-9920");

            migrationBuilder.DeleteData(
                table: "CuttingJobs",
                keyColumn: "CuttingJobNo",
                keyValue: "CJ-9921");

            migrationBuilder.InsertData(
                table: "CuttingJobs",
                columns: new[] { "CuttingJobNo", "Status", "TargetLine" },
                values: new object[,]
                {
                    { "JOB-1001", "Active", "Spreader_01" },
                    { "JOB-1002", "Active", "Spreader_02" }
                });

            migrationBuilder.InsertData(
                table: "Rolls",
                columns: new[] { "CuttingJobNo", "RollID", "Status" },
                values: new object[,]
                {
                    { "JOB-1001", "R-1045", "Pending" },
                    { "JOB-1001", "R-1046", "Pending" },
                    { "JOB-1001", "R-1047", "Pending" },
                    { "JOB-1002", "R-2050", "Pending" },
                    { "JOB-1002", "R-2051", "Pending" }
                });
        }
    }
}
