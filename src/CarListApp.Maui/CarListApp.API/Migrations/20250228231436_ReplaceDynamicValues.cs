using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarListApp.API.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceDynamicValues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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
                columns: new[] { "ConcurrencyStamp", "EmailConfirmed", "PasswordHash", "SecurityStamp" },
                values: new object[] { "82a1d823-1c77-4308-89e3-d735d6be42f1", false, "AQAAAAIAAYagAAAAEBsM92zuE2k1V0PZboRE/hij3ucp5lrW1RsaaJtCnVa6c3Bo9YMgJjlfAAuIwInDFw==", "c16026b1-eb5b-432c-a039-f0816a3f3db9" });

            migrationBuilder.UpdateData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: 1,
                column: "Vin",
                value: "TW123");

            migrationBuilder.UpdateData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: 4,
                column: "Vin",
                value: "ABCTW4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "3f4631bd-f907-4409-b416-ba356312e659",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "6c61d3e9-ce64-4e0c-8231-53f41bc8eddb", "AQAAAAIAAYagAAAAEDQ8P/OMKkQYDqqFzxTd202PtoPhcL32GTbOyhrRdWnCQoS6P8xhzhkRATfqV2+cfQ==", "2c1704f3-5106-4dbc-851f-e6ca9de3a4f7" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "408aa945-3d84-4421-8342-7269ec64d949",
                columns: new[] { "ConcurrencyStamp", "EmailConfirmed", "PasswordHash", "SecurityStamp" },
                values: new object[] { "b9b6e01f-2664-4bbc-9cda-2a29f0f6fd44", true, "AQAAAAIAAYagAAAAEOSFB0RAbnO42r3+28sBam/TzEpg1FNCj3eC3tu6nBzNdZNkxtackbtwpk1KrmQn1Q==", "7a00f71d-74e3-4eb0-ba83-e45d2b697b62" });

            migrationBuilder.UpdateData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: 1,
                column: "Vin",
                value: "ABC");

            migrationBuilder.UpdateData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: 4,
                column: "Vin",
                value: "ABC4");
        }
    }
}
