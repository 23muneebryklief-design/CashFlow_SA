namespace CashFlowSA.Application.Common.Settings
{
    public class OllamaSettings
    {
        public string BaseUrl { get; set; } = "http://localhost:11434";
        public string Model { get; set; } = "qwen3:8b";
    }
}