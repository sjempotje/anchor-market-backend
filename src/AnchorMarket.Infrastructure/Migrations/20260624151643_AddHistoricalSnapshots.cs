using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AnchorMarket.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddHistoricalSnapshots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OrderBookSnapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OutcomeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Bids = table.Column<string>(type: "text", nullable: false),
                    Asks = table.Column<string>(type: "text", nullable: false),
                    BestBid = table.Column<decimal>(type: "numeric", nullable: true),
                    BestAsk = table.Column<decimal>(type: "numeric", nullable: true),
                    Spread = table.Column<decimal>(type: "numeric", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderBookSnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderBookSnapshots_Outcomes_OutcomeId",
                        column: x => x.OutcomeId,
                        principalTable: "Outcomes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TradeFlowSnapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MarketId = table.Column<Guid>(type: "uuid", nullable: false),
                    OutcomeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ExecutedPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    Shares = table.Column<decimal>(type: "numeric", nullable: false),
                    BuyerOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    SellerOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    BidDepthAtTrade = table.Column<decimal>(type: "numeric", nullable: false),
                    AskDepthAtTrade = table.Column<decimal>(type: "numeric", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TradeFlowSnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TradeFlowSnapshots_Outcomes_OutcomeId",
                        column: x => x.OutcomeId,
                        principalTable: "Outcomes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderBookSnapshots_OutcomeId_Timestamp",
                table: "OrderBookSnapshots",
                columns: new[] { "OutcomeId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_TradeFlowSnapshots_MarketId_Timestamp",
                table: "TradeFlowSnapshots",
                columns: new[] { "MarketId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_TradeFlowSnapshots_OutcomeId_Timestamp",
                table: "TradeFlowSnapshots",
                columns: new[] { "OutcomeId", "Timestamp" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderBookSnapshots");

            migrationBuilder.DropTable(
                name: "TradeFlowSnapshots");
        }
    }
}
