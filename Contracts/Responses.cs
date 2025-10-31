public record ErrorResponse(int StatusCode, string Error, string Message);

public record StatsDto(
    int Total,
    int Completed,
    int Pending,
    int WindowDays,
    string[] Labels,
    int[] CreatedPerDay,
    int[] CompletedPerDay,
    int[] PendingSeries
);
