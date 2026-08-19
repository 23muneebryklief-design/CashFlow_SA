using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using CashFlowSA.Application.Common.Interfaces;
using CashFlowSA.Application.Common.Models;
using CashFlowSA.Application.Common.Settings;
using CashFlowSA.Domain.Models;
using Microsoft.Extensions.Options;

namespace CashFlowSA.Infrastructure.Services
{
    public class OllamaRiskScoringService : IRiskScoringService
    {
        private readonly HttpClient _httpClient;
        private readonly OllamaSettings _settings;

        public OllamaRiskScoringService(
            HttpClient httpClient,
            IOptions<OllamaSettings> settings)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
        }

        public async Task<AIRiskScoringResult> CalculateRiskAsync(
            Invoice invoice,
            SME sme,
            CancellationToken cancellationToken = default)
        {
            var tenorDays = Math.Max(
                0,
                (invoice.DueDate - invoice.IssueDate).Days);

            var businessAgeYears =
                Math.Max(
                    0,
                    (DateTime.UtcNow - sme.RegistrationDate).TotalDays / 365.25);

            var rejectedKycCount = sme.KYCApplications
                .Count(k => k.Status == Domain.Models.Enums.KycStatus.Rejected);

            var prompt = $$"""
You are the AI credit risk assessment engine for CashFlow SA,
a South African invoice financing marketplace.

Your job is to assess the investment risk of an invoice.

You MUST calculate:
1. A risk score from 0 to 100.
2. A risk grade from A to E.
3. The factors that influenced the score.
4. A plain-language explanation.
5. An investor-facing investment summary.

IMPORTANT:
- Higher scores mean LOWER investment risk.
- Lower scores mean HIGHER investment risk.
- Do not simply repeat the input.
- Analyse the information and make your own risk judgement.
- Do not invent facts that are not provided.
- If information is limited, acknowledge the limitation.
- Consider all available factors together rather than relying on only one factor.

Grade guidance:
A = Very Low Risk
B = Low Risk
C = Moderate Risk
D = High Risk
E = Very High Risk

The grade must be consistent with the score.

INVOICE INFORMATION
Invoice Number: {invoice.InvoiceNumber}
Debtor: {invoice.DebtorName}
Invoice Amount: R{invoice.Amount:N2}
Issue Date: {invoice.IssueDate:yyyy-MM-dd}
Due Date: {invoice.DueDate:yyyy-MM-dd}
Tenor: {tenorDays} days
Invoice Status: {invoice.Status}

SME INFORMATION
Company: {sme.CompanyName}
Industry: {sme.Industry}
Registration Date: {sme.RegistrationDate:yyyy-MM-dd}
Business Age: {businessAgeYears:F1} years

KYC INFORMATION
Number of KYC applications: {sme.KYCApplications.Count}
Number of previously rejected KYC applications: {rejectedKycCount}

Return ONLY valid JSON using exactly this structure:

{
  "riskScore": 0,
  "riskGrade": "A",
  "scoringFactors": "Brief description of the factors that influenced the score.",
  "explanationText": "Plain-language explanation of the risk assessment.",
  "investmentSummary": "Short investor-facing summary."
}
""";

            var request = new OllamaChatRequest
            {
                Model = _settings.Model,
                Stream = false,
                Messages =
                [
                    new OllamaMessage
                    {
                        Role = "user",
                        Content = prompt
                    }
                ]
            };

            using var response = await _httpClient.PostAsJsonAsync(
                "/api/chat",
                request,
                cancellationToken);

            response.EnsureSuccessStatusCode();

            var ollamaResponse =
                await response.Content.ReadFromJsonAsync<OllamaChatResponse>(
                    cancellationToken: cancellationToken);

            if (ollamaResponse == null ||
                string.IsNullOrWhiteSpace(ollamaResponse.Message?.Content))
            {
                throw new InvalidOperationException(
                    "Ollama returned an empty risk assessment response.");
            }

            var result = ParseResult(ollamaResponse.Message.Content);

            // Application-level validation.
            if (result.RiskScore < 0 || result.RiskScore > 100)
            {
                throw new InvalidOperationException(
                    $"Ollama returned an invalid risk score: {result.RiskScore}");
            }

            result.ModelUsed = _settings.Model;

            return result;
        }

        private static AIRiskScoringResult ParseResult(string content)
        {
            var json = ExtractJson(content);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            options.Converters.Add(new JsonStringEnumConverter());

                        var result =
                JsonSerializer.Deserialize<AIRiskScoringResult>(
                    json,
                    options);

            if (result == null)
            {
                throw new InvalidOperationException(
                    "Unable to parse Ollama risk assessment JSON.");
            }

            return result;
        }

        private static string ExtractJson(string content)
        {
            content = content.Trim();

            // Handle models that accidentally wrap JSON in markdown.
            if (content.StartsWith("```"))
            {
                var firstNewLine = content.IndexOf('\n');

                if (firstNewLine >= 0)
                {
                    content = content[(firstNewLine + 1)..];
                }

                var closingFence = content.LastIndexOf("```");

                if (closingFence >= 0)
                {
                    content = content[..closingFence];
                }
            }

            content = content.Trim();

            var firstBrace = content.IndexOf('{');
            var lastBrace = content.LastIndexOf('}');

            if (firstBrace < 0 || lastBrace <= firstBrace)
            {
                throw new InvalidOperationException(
                    "Ollama response did not contain valid JSON.");
            }

            return content[firstBrace..(lastBrace + 1)];
        }

        private sealed class OllamaChatRequest
        {
            [JsonPropertyName("model")]
            public string Model { get; set; } = string.Empty;

            [JsonPropertyName("messages")]
            public List<OllamaMessage> Messages { get; set; } = [];

            [JsonPropertyName("stream")]
            public bool Stream { get; set; }
        }

        private sealed class OllamaMessage
        {
            [JsonPropertyName("role")]
            public string Role { get; set; } = string.Empty;

            [JsonPropertyName("content")]
            public string Content { get; set; } = string.Empty;
        }

        private sealed class OllamaChatResponse
        {
            [JsonPropertyName("message")]
            public OllamaMessage? Message { get; set; }
        }
    }
}   