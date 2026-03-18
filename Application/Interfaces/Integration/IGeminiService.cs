using Application.DTOs.Admin;
using Application.DTOs.Catalog;
using Application.DTOs.Integration;
using Application.DTOs.Orders;
using Application.Interfaces.Admin;
using Application.Interfaces.Catalog;
using Application.Interfaces.Integration;
using Application.Interfaces.Orders;
using Application.DTOs;

namespace Application.Interfaces.Integration {
    public interface IGeminiService
    {
        Task<AnalysisResultDto> AnalyzeQueryAsync(string userMessage);
        Task<string> ChatAsync(string userMessage);
        Task<ProjectSuggestionDto?> SuggestProjectAsync(string userRequest, List<ProductInfoDto> products);
    }
}