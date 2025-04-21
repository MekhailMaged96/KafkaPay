using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KafkaPay.Shared.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOutBoxMessageEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TnxTransactions_TransactionStatus_StatusId",
                table: "TnxTransactions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TransactionStatus",
                table: "TransactionStatus");

            migrationBuilder.RenameTable(
                name: "TransactionStatus",
                newName: "TransactionStatuses");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TransactionStatuses",
                table: "TransactionStatuses",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "OutBoxMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OccuredOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProcessedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Error = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutBoxMessages", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_TnxTransactions_TransactionStatuses_StatusId",
                table: "TnxTransactions",
                column: "StatusId",
                principalTable: "TransactionStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TnxTransactions_TransactionStatuses_StatusId",
                table: "TnxTransactions");

            migrationBuilder.DropTable(
                name: "OutBoxMessages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TransactionStatuses",
                table: "TransactionStatuses");

            migrationBuilder.RenameTable(
                name: "TransactionStatuses",
                newName: "TransactionStatus");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TransactionStatus",
                table: "TransactionStatus",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TnxTransactions_TransactionStatus_StatusId",
                table: "TnxTransactions",
                column: "StatusId",
                principalTable: "TransactionStatus",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
