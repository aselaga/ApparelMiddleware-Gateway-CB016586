using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ApparelMiddlewareGateway.Migrations
{
    /// <inheritdoc />
    public partial class AddCuttingJobsAndRolls : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CuttingJobs",
                columns: table => new
                {
                    CuttingJobNo = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    TargetLine = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CuttingJobs", x => x.CuttingJobNo);
                });

            migrationBuilder.CreateTable(
                name: "Rolls",
                columns: table => new
                {
                    CuttingJobNo = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    RollID = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rolls", x => new { x.CuttingJobNo, x.RollID });
                    table.ForeignKey(
                        name: "FK_Rolls_CuttingJobs_CuttingJobNo",
                        column: x => x.CuttingJobNo,
                        principalTable: "CuttingJobs",
                        principalColumn: "CuttingJobNo",
                        onDelete: ReferentialAction.Cascade);
                });

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Rolls");

            migrationBuilder.DropTable(
                name: "CuttingJobs");
        }
    }
}
