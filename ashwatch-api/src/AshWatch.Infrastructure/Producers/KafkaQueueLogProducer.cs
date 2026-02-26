using System.Text.Json;
using System.Text.Json.Serialization;
using AshWatch.Application.Contracts;
using AshWatch.Application.Dtos;
using AshWatch.Domain.Entities;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;

namespace AshWatch.Infrastructure.Producers;

public sealed class KafkaLogQueueProducer : ILogQueueProducer, IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly string _topic;

    public KafkaLogQueueProducer(IConfiguration cfg)
    {
        IConfigurationSection section = cfg.GetSection("Kafka");
        _topic = section["Section"] ?? "logs.raw";

        ProducerConfig config = new()
        {
            BootstrapServers = section["BootstrapServers"],
            ClientId = section["ClientId"] ?? "ashwatch-api",
            Acks = Acks.All,
            EnableIdempotence = true,
            MessageSendMaxRetries = 3,
            RetryBackoffMaxMs = 200
        };

        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    public async Task ProduceAsync(Log log, CancellationToken ct = default)
    {
        var key = log.Id;
        var value = JsonSerializer.Serialize(log);

        var result = await _producer.ProduceAsync(_topic, new Message<string, string>
        {
            Key = key.ToString(),
            Value = value
        }, ct);

        Console.WriteLine($"Delivered to: {result.TopicPartitionOffset}");
    }

    public void Dispose()
    {
        _producer.Flush(TimeSpan.FromSeconds(5));
        _producer.Dispose();
    }
}