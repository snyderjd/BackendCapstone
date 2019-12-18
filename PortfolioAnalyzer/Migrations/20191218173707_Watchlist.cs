using Microsoft.EntityFrameworkCore.Migrations;

namespace PortfolioAnalyzer.Migrations
{
    public partial class Watchlist : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Watchlist",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    UserId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Watchlist", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Watchlist_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WatchlistSecurity",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WatchlistId = table.Column<int>(nullable: false),
                    SecurityId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WatchlistSecurity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WatchlistSecurity_Security_SecurityId",
                        column: x => x.SecurityId,
                        principalTable: "Security",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WatchlistSecurity_Watchlist_WatchlistId",
                        column: x => x.WatchlistId,
                        principalTable: "Watchlist",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "00000000-ffff-ffff-ffff-fffffffffff1",
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "5f17f8b9-c6dc-4cea-9c7f-6d8d0bfda8d3", "AQAAAAEAACcQAAAAEGCFgCaX54mMYLBYJMzSZYlwX1A4sRgIloC87hOKzC/MFEuXa3tUygNRgTjxLSsCdA==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "00000000-ffff-ffff-ffff-ffffffffffff",
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "84072c3e-c847-44dc-a250-741bf5b1d454", "AQAAAAEAACcQAAAAECX/euEY/NVKgeuQ9BYdKuLfZlEK99gPzFxHoy/KCxh8+JUQh2kd1Ehi+2u9mgpeHw==" });

            migrationBuilder.InsertData(
                table: "Watchlist",
                columns: new[] { "Id", "Description", "Name", "UserId" },
                values: new object[] { 1, "Optional description goes here", "Joe's first watchlist", "00000000-ffff-ffff-ffff-fffffffffff1" });

            migrationBuilder.InsertData(
                table: "WatchlistSecurity",
                columns: new[] { "Id", "SecurityId", "WatchlistId" },
                values: new object[] { 1, 1, 1 });

            migrationBuilder.InsertData(
                table: "WatchlistSecurity",
                columns: new[] { "Id", "SecurityId", "WatchlistId" },
                values: new object[] { 2, 2, 1 });

            migrationBuilder.InsertData(
                table: "WatchlistSecurity",
                columns: new[] { "Id", "SecurityId", "WatchlistId" },
                values: new object[] { 3, 3, 1 });

            migrationBuilder.CreateIndex(
                name: "IX_Watchlist_UserId",
                table: "Watchlist",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_WatchlistSecurity_SecurityId",
                table: "WatchlistSecurity",
                column: "SecurityId");

            migrationBuilder.CreateIndex(
                name: "IX_WatchlistSecurity_WatchlistId",
                table: "WatchlistSecurity",
                column: "WatchlistId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WatchlistSecurity");

            migrationBuilder.DropTable(
                name: "Watchlist");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "00000000-ffff-ffff-ffff-fffffffffff1",
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "e7249e85-da0f-488a-89d2-819b5cd011a1", "AQAAAAEAACcQAAAAEG5ZZ0XLhiLdqE7x2vZ3y5y2pIdd3FTMrDflyom3dm6bJLzOR+ZXFOY/QW/GNHmEiQ==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "00000000-ffff-ffff-ffff-ffffffffffff",
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "51854c68-2ec5-4e98-8b64-01c68a8d661e", "AQAAAAEAACcQAAAAEIsCfrOBcw70teTeuKT1vFtKGy+c0pGycXQxWnY0no57OLxXqjyg1AxnG1s9G1ZVhQ==" });
        }
    }
}
