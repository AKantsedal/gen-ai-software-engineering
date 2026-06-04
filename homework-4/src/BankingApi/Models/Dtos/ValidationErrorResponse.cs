namespace BankingApi.Models.Dtos;

public class ValidationErrorResponse
{
    public string Error { get; set; } = "Validation failed";
    public List<ValidationDetail> Details { get; set; } = new();
}

public class ValidationDetail
{
    public string Field { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
