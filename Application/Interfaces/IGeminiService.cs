using Application.DTOs;

namespace Application.Interfaces
{
    public interface IGeminiService
    {
        Task<AnalysisResultDto> AnalyzeQueryAsync(string userMessage);
        Task<string> ChatAsync(string userMessage, string productContext);
        Task<ProjectSuggestionDto?> SuggestProjectAsync(string userRequest, List<ProductInfoDto> products);
    }
}