public record TaskDto(long Id, string Title, string? Description, bool IsComplete,
                      DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt, DateTimeOffset? CompletedAt,
                      long UserId);

public record TaskCreateDto(string Title, string? Description, bool IsComplete);
public record TaskUpdateDto(string Title, string? Description, bool IsComplete);

public record TasksQuery(int Page = 1, int PageSize = 10, string? Search = null,
                         string Status = "all", string Sort = "createdAt", string Dir = "desc",
                         long? UserId = null);

public record PagedResult<T>(IEnumerable<T> Data, PaginationMeta Pagination);
public record PaginationMeta(int Page, int PageSize, int TotalCount, int TotalPages, bool HasNext, bool HasPrevious);

public static class Mapping
{
    public static TaskDto ToDto(this Domain.TaskItem t) =>
        new(t.Id, t.Title, t.Description, t.IsComplete, t.CreatedAt, t.UpdatedAt, t.CompletedAt, t.UserId);
}
