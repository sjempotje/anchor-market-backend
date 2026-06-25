using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AnchorMarket.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddExternalFeeds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExternalFeedRegistrations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MarketId = table.Column<Guid>(type: "uuid", nullable: false),
                    AdapterType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Config = table.Column<string>(type: "text", nullable: false),
                    PollingIntervalMs = table.Column<int>(type: "integer", nullable: false),
                    TimeoutMs = table.Column<int>(type: "integer", nullable: false),
                    ApiUrl = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    AuthToken = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ResolutionGranularitySeconds = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalFeedRegistrations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExternalFeedRegistrations_Markets_MarketId",
                        column: x => x.MarketId,
                        principalTable: "Markets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FeedResults",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FeedRegistrationId = table.Column<Guid>(type: "uuid", nullable: false),
                    RawJson = table.Column<string>(type: "text", nullable: false),
                    ParsedValue = table.Column<decimal>(type: "numeric", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    ErrorMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ReceivedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeedResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FeedResults_ExternalFeedRegistrations_FeedRegistrationId",
                        column: x => x.FeedRegistrationId,
                        principalTable: "ExternalFeedRegistrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExternalFeedRegistrations_MarketId_IsActive",
                table: "ExternalFeedRegistrations",
                columns: new[] { "MarketId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_FeedResults_FeedRegistrationId_ReceivedAt",
                table: "FeedResults",
                columns: new[] { "FeedRegistrationId", "ReceivedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FeedResults");

            migrationBuilder.DropTable(
                name: "ExternalFeedRegistrations");
        }
    }
}
