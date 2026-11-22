namespace InteractiveItinerary.DTO
{
    /// <summary>
    /// Consistent structured error payload returned for 400/403/404 API responses.
    /// </summary>
    public record ApiErrorResponse(
        int Status,
        string Error,
        string Message,
        IDictionary<string, string[]>? Errors = null);
}
