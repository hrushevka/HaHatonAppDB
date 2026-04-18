using System;
using HacatonApp.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HacatonApp.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260418193000_AddProjectScoringAndCriteriaMaxScore")]
    public partial class AddProjectScoringAndCriteriaMaxScore : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "MaxScore",
                table: "Criterias",
                type: "real",
                nullable: false,
                defaultValue: 10f);

            migrationBuilder.CreateTable(
                name: "ProjectReviews",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    JuryUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    TotalScore = table.Column<float>(type: "real", nullable: false, defaultValue: 0f),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectReviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectReviews_AspNetUsers_JuryUserId",
                        column: x => x.JuryUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProjectReviews_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectCriterionScores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectReviewId = table.Column<int>(type: "int", nullable: false),
                    CriteriaId = table.Column<int>(type: "int", nullable: false),
                    Score = table.Column<float>(type: "real", nullable: false, defaultValue: 0f)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectCriterionScores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectCriterionScores_Criterias_CriteriaId",
                        column: x => x.CriteriaId,
                        principalTable: "Criterias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectCriterionScores_ProjectReviews_ProjectReviewId",
                        column: x => x.ProjectReviewId,
                        principalTable: "ProjectReviews",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectReviews_JuryUserId",
                table: "ProjectReviews",
                column: "JuryUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectReviews_ProjectId_JuryUserId",
                table: "ProjectReviews",
                columns: new[] { "ProjectId", "JuryUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectCriterionScores_CriteriaId",
                table: "ProjectCriterionScores",
                column: "CriteriaId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectCriterionScores_ProjectReviewId_CriteriaId",
                table: "ProjectCriterionScores",
                columns: new[] { "ProjectReviewId", "CriteriaId" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "ProjectCriterionScores");
            migrationBuilder.DropTable(name: "ProjectReviews");
            migrationBuilder.DropColumn(name: "MaxScore", table: "Criterias");
        }
    }
}
