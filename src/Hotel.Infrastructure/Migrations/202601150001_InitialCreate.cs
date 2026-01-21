using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hotel.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Guests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FirstName = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    LastName = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Phone = table.Column<string>(type: "TEXT", maxLength: 32, nullable: true),
                    IdentityDocument = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Guests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Rooms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Number = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    Type = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    Capacity = table.Column<int>(type: "INTEGER", nullable: false),
                    PricePerNight = table.Column<decimal>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rooms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Reservations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RoomId = table.Column<int>(type: "INTEGER", nullable: false),
                    GuestId = table.Column<int>(type: "INTEGER", nullable: false),
                    CheckIn = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    CheckOut = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    GuestsCount = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reservations_Guests_GuestId",
                        column: x => x.GuestId,
                        principalTable: "Guests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reservations_Rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Guests_Email",
                table: "Guests",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_Number",
                table: "Rooms",
                column: "Number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_GuestId",
                table: "Reservations",
                column: "GuestId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_RoomId_CheckIn_CheckOut",
                table: "Reservations",
                columns: new[] { "RoomId", "CheckIn", "CheckOut" });

            // Seed Rooms
            migrationBuilder.InsertData(
                table: "Rooms",
                columns: new[] { "Id", "Number", "Type", "Capacity", "PricePerNight", "IsActive" },
                values: new object[,]
                {
                    { 1, "101", "Standard", 2, 250m, true },
                    { 2, "102", "Standard", 3, 320m, true },
                    { 3, "201", "Deluxe", 2, 450m, true },
                    { 4, "202", "Deluxe", 4, 600m, true },
                    { 5, "301", "Suite", 4, 900m, true },
                    { 6, "999", "Maintenance", 1, 0m, false }
                });

            // Seed Guests
            migrationBuilder.InsertData(
                table: "Guests",
                columns: new[] { "Id", "FirstName", "LastName", "Email", "Phone", "IdentityDocument" },
                values: new object[,]
                {
                    { 1, "Jan", "Kowalski", "jan.kowalski@example.com", "500600700", "ABC123456" },
                    { 2, "Anna", "Nowak", "anna.nowak@example.com", null, null },
                    { 3, "Piotr", "Zielinski", "piotr.zielinski@example.com", "123123123", "XYZ987654" }
                });

            // Seed one future reservation
            migrationBuilder.InsertData(
                table: "Reservations",
                columns: new[] { "Id", "RoomId", "GuestId", "CheckIn", "CheckOut", "GuestsCount", "TotalPrice", "Status", "RowVersion" },
                values: new object[] { 1, 1, 1, "2030-01-10", "2030-01-12", 2, 500m, "Active", new byte[0] });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Reservations");
            migrationBuilder.DropTable(name: "Guests");
            migrationBuilder.DropTable(name: "Rooms");
        }
    }
}
