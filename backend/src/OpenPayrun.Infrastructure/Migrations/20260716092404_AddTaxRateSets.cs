using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenPayrun.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTaxRateSets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TaxRateSets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EffectiveFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    EffectiveTo = table.Column<DateOnly>(type: "date", nullable: true),
                    QppExemption = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    QppYmpe = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    QppYampe = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    QppBaseRate = table.Column<decimal>(type: "decimal(10,6)", precision: 10, scale: 6, nullable: false),
                    QppAdditionalTier1Rate = table.Column<decimal>(type: "decimal(10,6)", precision: 10, scale: 6, nullable: false),
                    QppTier1Rate = table.Column<decimal>(type: "decimal(10,6)", precision: 10, scale: 6, nullable: false),
                    QppTier2Rate = table.Column<decimal>(type: "decimal(10,6)", precision: 10, scale: 6, nullable: false),
                    QppTier1MaxEmployee = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    QppTier2MaxEmployee = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    EiEmployeeRate = table.Column<decimal>(type: "decimal(10,6)", precision: 10, scale: 6, nullable: false),
                    EiEmployerMultiplier = table.Column<decimal>(type: "decimal(10,6)", precision: 10, scale: 6, nullable: false),
                    EiMaxInsurableEarnings = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    EiMaxEmployeePremium = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    QpipEmployeeRate = table.Column<decimal>(type: "decimal(10,6)", precision: 10, scale: 6, nullable: false),
                    QpipEmployerRate = table.Column<decimal>(type: "decimal(10,6)", precision: 10, scale: 6, nullable: false),
                    QpipMaxInsurableEarnings = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    QpipMaxEmployeePremium = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    QpipMaxEmployerPremium = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    FederalBasicPersonalAmount = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    FederalEmploymentAmount = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    FederalLowestRate = table.Column<decimal>(type: "decimal(10,6)", precision: 10, scale: 6, nullable: false),
                    QuebecFederalAbatement = table.Column<decimal>(type: "decimal(10,6)", precision: 10, scale: 6, nullable: false),
                    QuebecBasicPersonalAmount = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    QuebecWorkerDeductionMax = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    QuebecWorkerDeductionRate = table.Column<decimal>(type: "decimal(10,6)", precision: 10, scale: 6, nullable: false),
                    QuebecLowestRate = table.Column<decimal>(type: "decimal(10,6)", precision: 10, scale: 6, nullable: false),
                    FssqSmallEmployerRate = table.Column<decimal>(type: "decimal(10,6)", precision: 10, scale: 6, nullable: false),
                    FssqLargeEmployerRate = table.Column<decimal>(type: "decimal(10,6)", precision: 10, scale: 6, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaxRateSets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TaxBracket",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TaxRateSetId = table.Column<int>(type: "int", nullable: false),
                    BracketType = table.Column<int>(type: "int", nullable: false),
                    UpperBound = table.Column<decimal>(type: "decimal(20,4)", precision: 20, scale: 4, nullable: true),
                    Rate = table.Column<decimal>(type: "decimal(10,6)", precision: 10, scale: 6, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaxBracket", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaxBracket_TaxRateSets_TaxRateSetId",
                        column: x => x.TaxRateSetId,
                        principalTable: "TaxRateSets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TaxBracket_TaxRateSetId",
                table: "TaxBracket",
                column: "TaxRateSetId");

            migrationBuilder.CreateIndex(
                name: "IX_TaxRateSets_EffectiveFrom",
                table: "TaxRateSets",
                column: "EffectiveFrom");

            migrationBuilder.Sql(@"
SET IDENTITY_INSERT [TaxRateSets] ON;
INSERT INTO [TaxRateSets]
    (Id, EffectiveFrom, EffectiveTo,
     QppExemption, QppYmpe, QppYampe, QppBaseRate, QppAdditionalTier1Rate, QppTier1Rate, QppTier2Rate, QppTier1MaxEmployee, QppTier2MaxEmployee,
     EiEmployeeRate, EiEmployerMultiplier, EiMaxInsurableEarnings, EiMaxEmployeePremium,
     QpipEmployeeRate, QpipEmployerRate, QpipMaxInsurableEarnings, QpipMaxEmployeePremium, QpipMaxEmployerPremium,
     FederalBasicPersonalAmount, FederalEmploymentAmount, FederalLowestRate, QuebecFederalAbatement,
     QuebecBasicPersonalAmount, QuebecWorkerDeductionMax, QuebecWorkerDeductionRate, QuebecLowestRate,
     FssqSmallEmployerRate, FssqLargeEmployerRate)
VALUES
-- 2025: blended 14.5% federal first bracket (mid-year 15%→14% rate change per T4127)
(1, '2025-01-01', '2025-12-31',
 3500.00, 71300.00, 81200.00, 0.054000, 0.010000, 0.064000, 0.040000, 4339.20, 396.00,
 0.013100, 1.400000, 65700.00, 860.67,
 0.004940, 0.006920, 98000.00, 484.12, 678.16,
 16129.00, 1501.00, 0.145000, 0.165000,
 18571.00, 1420.00, 0.060000, 0.140000,
 0.016500, 0.042600),
-- 2026: T4127 Jan 2026 / TP-1015.F 2026-01
(2, '2026-01-01', NULL,
 3500.00, 74600.00, 85000.00, 0.053000, 0.010000, 0.063000, 0.040000, 4479.30, 416.00,
 0.013000, 1.400000, 68900.00, 895.70,
 0.004300, 0.006020, 103000.00, 442.90, 620.06,
 16452.00, 1501.00, 0.140000, 0.165000,
 18952.00, 1450.00, 0.060000, 0.140000,
 0.016500, 0.042600);
SET IDENTITY_INSERT [TaxRateSets] OFF;

INSERT INTO [TaxBracket] (TaxRateSetId, BracketType, UpperBound, Rate, SortOrder) VALUES
-- 2025 Federal (BracketType=0)
(1, 0,  57375.00, 0.145000, 1),
(1, 0, 114750.00, 0.205000, 2),
(1, 0, 177882.00, 0.260000, 3),
(1, 0, 253414.00, 0.290000, 4),
(1, 0,      NULL, 0.330000, 5),
-- 2025 Quebec (BracketType=1)
(1, 1,  53255.00, 0.140000, 1),
(1, 1, 106495.00, 0.190000, 2),
(1, 1, 129590.00, 0.240000, 3),
(1, 1,      NULL, 0.257500, 4),
-- 2026 Federal (BracketType=0)
(2, 0,  58523.00, 0.140000, 1),
(2, 0, 117045.00, 0.205000, 2),
(2, 0, 181440.00, 0.260000, 3),
(2, 0, 258482.00, 0.290000, 4),
(2, 0,      NULL, 0.330000, 5),
-- 2026 Quebec (BracketType=1)
(2, 1,  54345.00, 0.140000, 1),
(2, 1, 108680.00, 0.190000, 2),
(2, 1, 132245.00, 0.240000, 3),
(2, 1,      NULL, 0.257500, 4);
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TaxBracket");

            migrationBuilder.DropTable(
                name: "TaxRateSets");
        }
    }
}
