using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarListApp.API.Migrations
{
    /// <inheritdoc />
    public partial class FixToUsingPasswordHasker : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "3f4631bd-f907-4409-b416-ba356312e659",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "fba74cc5-65b0-4490-8b58-ce1d31654817", "AQAAAAIAAYagAAAAEHKFueEzlVR/gQeCUhzdKifSD/E0dCrVVJFIVcZgWcCR8/0HnxbU/tYLF/OxHJPOiQ==", "ab6ce355-bfd7-48a7-9ce7-368794035cec" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "408aa945-3d84-4421-8342-7269ec64d949",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "4732cc35-6e42-434b-a0da-e90f356bcf4a", "AQAAAAIAAYagAAAAEKip5TIhEVm2qL1XUglnwaI/P1y7mvxwYRsgc4nZZEoDkpLuW+fbAmhqvwNjWklvlA==", "b414a1e0-7b21-4b55-846f-eeb4c06cc8ad" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "3f4631bd-f907-4409-b416-ba356312e659",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "5e7bb702-7a05-4f98-92c2-4f0d7643cbc3", "AQAAAAIAAYagAAAAEHhtuDbiUHeV8VRAqp4+SE+kMF6WswWLfqHzUTSkv+muHMdlrz+JUR6EoqtU5bWiNQ==", "c2d32ed9-2445-4e46-b715-9a901b291778" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "408aa945-3d84-4421-8342-7269ec64d949",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "0d1ab9ef-41e2-484c-9da7-8ec21879ddf5", "AQAAAAIAAYagAAAAEHNLnrHGmj7uKoapUFDSrr7oph+XkoDv18MTM8r6mJxyOBFQEDrasHHRswmexG/Y1A==", "4b2c998c-4336-4e0e-bb95-fa4a2cfacfcf" });
        }
    }
}
