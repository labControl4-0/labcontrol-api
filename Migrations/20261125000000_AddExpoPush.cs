using Microsoft.EntityFrameworkCore.Migrations;

namespace LabControlApi.Migrations;

public partial class AddExpoPush : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "PushDevices",
            columns: table => new
            {
                Token = table.Column<string>(type: "text", nullable: false),
                UserId = table.Column<Guid>(type: "uuid", nullable: false),
                RegistrationId = table.Column<Guid>(type: "uuid", nullable: false),
                Platform = table.Column<string>(type: "text", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PushDevices", device => device.Token);
                table.ForeignKey("FK_PushDevices_Users_UserId", device => device.UserId,
                    "Users", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "PushDeliveries",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                EventId = table.Column<Guid>(type: "uuid", nullable: false),
                UserId = table.Column<Guid>(type: "uuid", nullable: false),
                RegistrationId = table.Column<Guid>(type: "uuid", nullable: false),
                NextAttemptAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                Attempts = table.Column<int>(type: "integer", nullable: false),
                TicketId = table.Column<string>(type: "text", nullable: true),
                Outcome = table.Column<string>(type: "text", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PushDeliveries", delivery => delivery.Id);
                table.ForeignKey("FK_PushDeliveries_MachineEvents_EventId", delivery => delivery.EventId,
                    "MachineEvents", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex("IX_PushDevices_UserId", "PushDevices", "UserId");
        migrationBuilder.CreateIndex("IX_PushDevices_RegistrationId", "PushDevices", "RegistrationId", unique: true);
        migrationBuilder.CreateIndex("IX_PushDeliveries_EventId", "PushDeliveries", "EventId");
        migrationBuilder.CreateIndex("IX_PushDeliveries_Outcome_NextAttemptAt", "PushDeliveries",
            new[] { "Outcome", "NextAttemptAt" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("PushDeliveries");
        migrationBuilder.DropTable("PushDevices");
    }
}
