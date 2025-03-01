using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarListApp.API.Migrations
{
    /// <inheritdoc />
    public partial class FixDynamicTestData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "3f4631bd-f907-4409-b416-ba356312e659",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "463dc2af-ca3b-4020-a5a6-7df5da5fbd73", "AQAAAAIAAYagAAAAEJNqRzk4QmvB6fb2b0dGhRKiOxTcj4/iRRB3RoMU/FP1eFqhsuFA0Ov1iqTPIYfLbQ==", "34ced730-ffbf-4969-a391-a2dc0937bc52" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "408aa945-3d84-4421-8342-7269ec64d949",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "82a1d823-1c77-4308-89e3-d735d6be42f1", "AQAAAAIAAYagAAAAEBsM92zuE2k1V0PZboRE/hij3ucp5lrW1RsaaJtCnVa6c3Bo9YMgJjlfAAuIwInDFw==", "c16026b1-eb5b-432c-a039-f0816a3f3db9" });
        }
    }
}
