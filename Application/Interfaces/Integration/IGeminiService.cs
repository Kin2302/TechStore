using Application.DTOs.Catalog;
using Application.DTOs.Integration;

namespace Application.Interfaces.Integration
{
    public interface IGeminiService
    {
        Task<string> ChatAsync(string userMessage, bool isAdmin = false, string? userId = null);
        Task<AnalysisResultDto> AnalyzeQueryAsync(string userMessage);
        Task<ProjectSuggestionDto?> SuggestProjectAsync(string userRequest, List<ProductInfoDto> products);
    }
}
