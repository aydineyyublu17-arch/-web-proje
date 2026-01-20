using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PrintMarket.Migrations
{
    /// <inheritdoc />
    public partial class DeleteTifdrukCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM Categories WHERE Id = 1010");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
