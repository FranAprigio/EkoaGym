using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EkoaGym.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkoutSessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SessionSetLogs_Users_UserId",
                table: "SessionSetLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_SessionSetLogs_WorkoutPlans_PlanId",
                table: "SessionSetLogs");

            migrationBuilder.DropIndex(
                name: "IX_SessionSetLogs_PlanId",
                table: "SessionSetLogs");

            migrationBuilder.DropIndex(
                name: "IX_SessionSetLogs_UserId_PlanId_ExerciseId_CompletedAt",
                table: "SessionSetLogs");

            migrationBuilder.DropColumn(
                name: "PlanId",
                table: "SessionSetLogs");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "SessionSetLogs");

            migrationBuilder.RenameColumn(
                name: "WeightKg",
                table: "SessionSetLogs",
                newName: "ActualWeightKg");

            migrationBuilder.RenameColumn(
                name: "Reps",
                table: "SessionSetLogs",
                newName: "ActualReps");

            migrationBuilder.CreateTable(
                name: "WorkoutSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkoutSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkoutSessions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkoutSessions_WorkoutPlans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "WorkoutPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SessionSetLogs_SessionId_ExerciseId",
                table: "SessionSetLogs",
                columns: new[] { "SessionId", "ExerciseId" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutSessions_PlanId",
                table: "WorkoutSessions",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutSessions_UserId_StartedAt",
                table: "WorkoutSessions",
                columns: new[] { "UserId", "StartedAt" });

            migrationBuilder.AddForeignKey(
                name: "FK_SessionSetLogs_WorkoutSessions_SessionId",
                table: "SessionSetLogs",
                column: "SessionId",
                principalTable: "WorkoutSessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SessionSetLogs_WorkoutSessions_SessionId",
                table: "SessionSetLogs");

            migrationBuilder.DropTable(
                name: "WorkoutSessions");

            migrationBuilder.DropIndex(
                name: "IX_SessionSetLogs_SessionId_ExerciseId",
                table: "SessionSetLogs");

            migrationBuilder.RenameColumn(
                name: "ActualWeightKg",
                table: "SessionSetLogs",
                newName: "WeightKg");

            migrationBuilder.RenameColumn(
                name: "ActualReps",
                table: "SessionSetLogs",
                newName: "Reps");

            migrationBuilder.AddColumn<Guid>(
                name: "PlanId",
                table: "SessionSetLogs",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "SessionSetLogs",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_SessionSetLogs_PlanId",
                table: "SessionSetLogs",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionSetLogs_UserId_PlanId_ExerciseId_CompletedAt",
                table: "SessionSetLogs",
                columns: new[] { "UserId", "PlanId", "ExerciseId", "CompletedAt" });

            migrationBuilder.AddForeignKey(
                name: "FK_SessionSetLogs_Users_UserId",
                table: "SessionSetLogs",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SessionSetLogs_WorkoutPlans_PlanId",
                table: "SessionSetLogs",
                column: "PlanId",
                principalTable: "WorkoutPlans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
