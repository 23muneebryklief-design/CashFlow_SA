using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CashFlowSA.Application.Common.Interfaces;
using CashFlowSA.Domain.Models;

namespace CashFlowSA.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class AIController : ControllerBase
    {
        private readonly IRiskScoringService _riskScoringService;
        private readonly IRiskExplanationService _riskExplanationService;

        public AIController(
            IRiskScoringService riskScoringService,
            IRiskExplanationService riskExplanationService)
        {
            _riskScoringService = riskScoringService;
            _riskExplanationService = riskExplanationService;
        }

        [HttpPost("test")]
        public async Task<IActionResult> TestOllama(
            CancellationToken cancellationToken)
        {
            // ---------------------------------------------------------
            // TEMPORARY TEST DATA
            // ---------------------------------------------------------
            // This represents the invoice and SME information that
            // Ollama will actually analyse.
            //
            // Later we will replace this with data loaded from SQL.
            // ---------------------------------------------------------

            var invoice = new Invoice
            {
                InvoiceId = Guid.NewGuid(),
                SMEId = Guid.NewGuid(),

                InvoiceNumber = "INV-TEST-001",
                DebtorName = "ABC Manufacturing",
                DebtorContactDetails = "finance@abcmfg.co.za",

                Amount = 50000m,

                IssueDate = DateTime.UtcNow.AddDays(-10),
                DueDate = DateTime.UtcNow.AddDays(20),

                Status = Domain.Models.Enums.InvoiceStatus.Submitted
            };

            var sme = new SME
            {
                SMEId = invoice.SMEId,

                CompanyName = "Test Engineering (Pty) Ltd",
                ContactPerson = "John Smith",
                CompanyEmail = "john@testengineering.co.za",
                CompanyPhoneNumber = "0110000000",

                RegistrationDate = DateTime.UtcNow.AddYears(-5),

                RegistrationNumber = "2021/123456/07",

                Industry = Domain.Models.Enums.IndustryType.Other,

                Address = "Johannesburg, South Africa",
                TaxNumber = "9999999999",

                KYCApplications =
                {
                    new KYCApplication
                    {
                        ApplicationId = Guid.NewGuid(),
                        SMEId = invoice.SMEId,
                        Status = Domain.Models.Enums.KycStatus.Verified,
                    }
                }
            };

            // ---------------------------------------------------------
            // STEP 1:
            // LET OLLAMA CALCULATE THE RISK
            // ---------------------------------------------------------

            var aiRiskResult =
                await _riskScoringService.CalculateRiskAsync(
                    invoice,
                    sme,
                    cancellationToken);

            // ---------------------------------------------------------
            // STEP 2:
            // Convert the AI result into our RiskAssessment model
            // ---------------------------------------------------------

            var riskAssessment = new RiskAssessment
            {
                RiskAssessmentId = Guid.NewGuid(),

                InvoiceId = invoice.InvoiceId,

                RiskScore = aiRiskResult.RiskScore,

                RiskGrade = aiRiskResult.RiskGrade,

                ScoringFactors = aiRiskResult.ScoringFactors
            };

            // ---------------------------------------------------------
            // STEP 3:
            // LET OLLAMA EXPLAIN THE CALCULATION
            // ---------------------------------------------------------

            var explanation =
                await _riskExplanationService.GenerateExplanationAsync(
                    riskAssessment,
                    cancellationToken);

            // ---------------------------------------------------------
            // RETURN BOTH THE CALCULATION AND EXPLANATION
            // ---------------------------------------------------------

            return Ok(new
            {
                invoice = new
                {
                    invoice.InvoiceNumber,
                    invoice.DebtorName,
                    invoice.Amount,
                    invoice.IssueDate,
                    invoice.DueDate
                },

                sme = new
                {
                    sme.CompanyName,
                    sme.RegistrationDate,
                    sme.Industry
                },

                aiRiskCalculation = new
                {
                    aiRiskResult.RiskScore,
                    aiRiskResult.RiskGrade,
                    aiRiskResult.ScoringFactors
                },

                aiExplanation = explanation
            });
        }
    }
}