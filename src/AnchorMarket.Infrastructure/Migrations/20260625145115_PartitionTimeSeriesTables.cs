using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AnchorMarket.Infrastructure.Migrations
{
    /// <summary>
    /// Converts the high-volume time-series tables to PostgreSQL range-partitioned tables, keyed on
    /// their timestamp column. Existing rows are preserved via a DEFAULT partition; monthly partitions
    /// are provisioned going forward by the PartitionManager background service.
    /// </summary>
    /// <remarks>
    /// This migration is PostgreSQL-specific and rewrites the tables (rename, recreate as partitioned,
    /// copy, drop). It is safe on an empty database; on a populated database it copies existing rows
    /// into the DEFAULT partition.
    /// </remarks>
    public partial class PartitionTimeSeriesTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            Partition(migrationBuilder,
                table: "PriceHistory", partitionColumn: "Timestamp", pk: "\"Id\", \"Timestamp\"",
                indexName: "IX_PriceHistory_OutcomeId_Timestamp", indexColumns: "\"OutcomeId\", \"Timestamp\"",
                fkName: "FK_PriceHistory_Outcomes_OutcomeId", fkColumn: "OutcomeId", principalTable: "Outcomes");

            Partition(migrationBuilder,
                table: "OrderBookSnapshots", partitionColumn: "Timestamp", pk: "\"Id\", \"Timestamp\"",
                indexName: "IX_OrderBookSnapshots_OutcomeId_Timestamp", indexColumns: "\"OutcomeId\", \"Timestamp\"",
                fkName: "FK_OrderBookSnapshots_Outcomes_OutcomeId", fkColumn: "OutcomeId", principalTable: "Outcomes");

            Partition(migrationBuilder,
                table: "FeedResults", partitionColumn: "ReceivedAt", pk: "\"Id\", \"ReceivedAt\"",
                indexName: "IX_FeedResults_FeedRegistrationId_ReceivedAt", indexColumns: "\"FeedRegistrationId\", \"ReceivedAt\"",
                fkName: "FK_FeedResults_ExternalFeedRegistrations_FeedRegistrationId", fkColumn: "FeedRegistrationId", principalTable: "ExternalFeedRegistrations");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            Departition(migrationBuilder,
                table: "PriceHistory", pk: "\"Id\"",
                indexName: "IX_PriceHistory_OutcomeId", indexColumns: "\"OutcomeId\"",
                fkName: "FK_PriceHistory_Outcomes_OutcomeId", fkColumn: "OutcomeId", principalTable: "Outcomes");

            Departition(migrationBuilder,
                table: "OrderBookSnapshots", pk: "\"Id\"",
                indexName: "IX_OrderBookSnapshots_OutcomeId_Timestamp", indexColumns: "\"OutcomeId\", \"Timestamp\"",
                fkName: "FK_OrderBookSnapshots_Outcomes_OutcomeId", fkColumn: "OutcomeId", principalTable: "Outcomes");

            Departition(migrationBuilder,
                table: "FeedResults", pk: "\"Id\"",
                indexName: "IX_FeedResults_FeedRegistrationId_ReceivedAt", indexColumns: "\"FeedRegistrationId\", \"ReceivedAt\"",
                fkName: "FK_FeedResults_ExternalFeedRegistrations_FeedRegistrationId", fkColumn: "FeedRegistrationId", principalTable: "ExternalFeedRegistrations");
        }

        private static void Partition(MigrationBuilder b, string table, string partitionColumn, string pk,
            string indexName, string indexColumns, string fkName, string fkColumn, string principalTable)
        {
            b.Sql($"""
                ALTER TABLE "{table}" RENAME TO "{table}_old";
                -- Free the constraint/index names (renaming the table does not rename them) so the
                -- new partitioned table can reuse them.
                ALTER TABLE "{table}_old" DROP CONSTRAINT "PK_{table}";
                DROP INDEX IF EXISTS "{indexName}";
                CREATE TABLE "{table}" (
                    LIKE "{table}_old" INCLUDING DEFAULTS,
                    CONSTRAINT "PK_{table}" PRIMARY KEY ({pk})
                ) PARTITION BY RANGE ("{partitionColumn}");
                CREATE TABLE "{table}_default" PARTITION OF "{table}" DEFAULT;
                INSERT INTO "{table}" SELECT * FROM "{table}_old";
                DROP TABLE "{table}_old" CASCADE;
                CREATE INDEX "{indexName}" ON "{table}" ({indexColumns});
                ALTER TABLE "{table}" ADD CONSTRAINT "{fkName}"
                    FOREIGN KEY ("{fkColumn}") REFERENCES "{principalTable}" ("Id") ON DELETE CASCADE;
                """);
        }

        private static void Departition(MigrationBuilder b, string table, string pk,
            string indexName, string indexColumns, string fkName, string fkColumn, string principalTable)
        {
            b.Sql($"""
                ALTER TABLE "{table}" RENAME TO "{table}_part";
                ALTER TABLE "{table}_part" DROP CONSTRAINT "PK_{table}";
                DROP INDEX IF EXISTS "{indexName}";
                CREATE TABLE "{table}" (
                    LIKE "{table}_part" INCLUDING DEFAULTS,
                    CONSTRAINT "PK_{table}" PRIMARY KEY ({pk})
                );
                INSERT INTO "{table}" SELECT * FROM "{table}_part";
                DROP TABLE "{table}_part" CASCADE;
                CREATE INDEX "{indexName}" ON "{table}" ({indexColumns});
                ALTER TABLE "{table}" ADD CONSTRAINT "{fkName}"
                    FOREIGN KEY ("{fkColumn}") REFERENCES "{principalTable}" ("Id") ON DELETE CASCADE;
                """);
        }
    }
}
