using System.Net.Http.Json;
using CashFlowSA.Application.Common.Interfaces;
using CashFlowSA.Application.Common.Settings;
using CashFlowSA.Domain.Models;
using Microsoft.Extensions.Options;

namespace CashFlowSA.Infrastructure.Services
{
    public class OllamaRiskExplanationService : IRiskExplanationService
    {
        private readonly HttpClient _httpClient;
        private readonly OllamaSettings _settings;

        public OllamaRiskExplanationService(
            HttpClient httpClient,
            IOptions<OllamaSettings> settings)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
        }

        public async Task<AIExplanation> GenerateExplanationAsync(
            RiskAssessment riskAssessment,
            CancellationToken cancellationToken = default)
        {
            var prompt = $"""
                You are a financial risk analyst for CashFlow SA.

                Explain the following invoice risk assessment to an investor.

                Risk score: {riskAssessment.RiskScore}/100
                Risk grade: {riskAssessment.RiskGrade}
                Scoring factors: {riskAssessment.ScoringFactors}

                Return your response using exactly this format:

                EXPLANATION:
                [A clear explanation of why this invoice received this risk score and grade.]

                INVESTMENT SUMMARY:
                [A short 1-2 sentence summary for an investor.]

                Do not change the risk score or grade.
                Do not invent financial information that was not provided.
                """;

            var request = new
            {
                model = _settings.Model,
                messages = new[]
                {
                    new
                    {
                        role = "user",
                        content = prompt
                    }
                },
                stream = false
            };

            var response = await _httpClient.PostAsJsonAsync(
                "/api/chat",
                request,
                cancellationToken);

            response.EnsureSuccessStatusCode();

            var result = await response.Content
                .ReadFromJsonAsync<OllamaChatResponse>(
                    cancellationToken: cancellationToken);

            if (result == null || string.IsNullOrWhiteSpace(result.Message?.Content))
            {
                throw new InvalidOperationException(
                    "Ollama returned an empty response.");
            }

            var content = result.Message.Content;

            var explanation = ExtractSection(
                content,
                "EXPLANATION:",
                "INVESTMENT SUMMARY:");

            var investmentSummary = ExtractSection(
                content,
                "INVESTMENT SUMMARY:",
                null);

            return new AIExplanation
            {
                RiskAssessmentId = riskAssessment.RiskAssessmentId,
                ExplanationText = explanation,
                InvestmentSummary = investmentSummary,
                ModelUsed = _settings.Model,
                IsAvailable = true,
                GeneratedAt = DateTime.UtcNow
            };
        }

        private static string ExtractSection(
            string content,
            string startMarker,
            string? endMarker)
        {
            var startIndex = content.IndexOf(
                startMarker,
                StringComparison.OrdinalIgnoreCase);

            if (startIndex < 0)
            {
                return content.Trim();
            }

            startIndex += startMarker.Length;

            var endIndex = endMarker == null
                ? content.Length
                : content.IndexOf(
                    endMarker,
                    startIndex,
                    StringComparison.OrdinalIgnoreCase);

            if (endIndex < 0)
            {
                endIndex = content.Length;
            }

            return content[startIndex..endIndex].Trim();
        }

        private class OllamaChatResponse
        {
            public OllamaMessage? Message { get; set; }
        }

        private class OllamaMessage
        {
            public string Content { get; set; } = string.Empty;
        }
    }
}