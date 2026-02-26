using AshWatch.Application.Dtos;
using AshWatch.Domain.Entities;

namespace AshWatch.Application.Contracts;

public interface ILogQueueProducer
{
    public Task ProduceAsync(Log log, CancellationToken ct = default);
}