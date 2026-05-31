using AshWatch.Domain.Entities;

namespace AshWatch.Application.Contracts;

public interface IPublisher
{
    public Task<string> PublishAsync(Log log, CancellationToken ct = default);
}