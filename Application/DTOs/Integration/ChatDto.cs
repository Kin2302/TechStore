namespace Application.DTOs.Integration {
    public class ChatRequestDto
    {
        public string Message { get; set; } = "";
    }

    public class ChatResponseDto
    {
        public string Reply { get; set; } = "";
        public AnalysisResultDto? Analysis { get; set; }
        public int ProductsFound { get; set; }
    }

    public class AnalysisResultDto
    {
        public List<string> Keywords { get; set; } = new();
        public List<string> Categories { get; set; } = new();
        public string Intent { get; set; } = "question";
    }

    public class ProjectSuggestionDto
    {
        public string ProjectName { get; set; } = "";
        public string Description { get; set; } = "";
        public List<ComponentSuggestionDto> Components { get; set; } = new();
        public decimal TotalBudget { get; set; }
        public string Difficulty { get; set; } = "";
        public string Tips { get; set; } = "";
        public List<string> MissingItems { get; set; } = new();
    }

    public class ComponentSuggestionDto
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = "";
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string Reason { get; set; } = "";
    }
}