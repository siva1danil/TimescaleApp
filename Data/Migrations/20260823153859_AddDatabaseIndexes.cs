using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDatabaseIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Values_ResultId",
                table: "Values");

            migrationBuilder.CreateIndex(
                name: "IX_Values_ResultId_Date",
                table: "Values",
                columns: new[] { "ResultId", "Date" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_Results_AverageExecutionTime",
                table: "Results",
                column: "AverageExecutionTime");

            migrationBuilder.CreateIndex(
                name: "IX_Results_AverageValue",
                table: "Results",
                column: "AverageValue");

            migrationBuilder.CreateIndex(
                name: "IX_Results_Filename",
                table: "Results",
                column: "Filename",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Results_FirstOperationDate",
                table: "Results",
                column: "FirstOperationDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Values_ResultId_Date",
                table: "Values");

            migrationBuilder.DropIndex(
                name: "IX_Results_AverageExecutionTime",
                table: "Results");

            migrationBuilder.DropIndex(
                name: "IX_Results_AverageValue",
                table: "Results");

            migrationBuilder.DropIndex(
                name: "IX_Results_Filename",
                table: "Results");

            migrationBuilder.DropIndex(
                name: "IX_Results_FirstOperationDate",
                table: "Results");

            migrationBuilder.CreateIndex(
                name: "IX_Values_ResultId",
                table: "Values",
                column: "ResultId");
        }
    }
}
