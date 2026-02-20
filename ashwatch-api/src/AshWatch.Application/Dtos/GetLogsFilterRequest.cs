namespace AshWatch.Application.Dtos;

public class GetLogsFilterRequest
{
    public int TenantId { get; set; }
    public int ProjectId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
