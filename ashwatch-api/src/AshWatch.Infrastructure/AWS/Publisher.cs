using System.Text.Json;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using AshWatch.Application.Contracts;
using AshWatch.Domain.Entities;
using Microsoft.Extensions.Configuration;

namespace AshWatch.Infrastructure.AWS;

public class Publisher(IAmazonSimpleNotificationService snsClient, IConfiguration configuration) : IPublisher
{
    public async Task<string> PublishAsync(Log log, CancellationToken ct = default)
    {
        PublishRequest request = new()
        {
            TopicArn = configuration["AWS:SNSTopicArn"],
            Message = JsonSerializer.Serialize(log),
            Subject = "AshWatchLogging",
            MessageGroupId = log.ProjectId.ToString(),
            MessageDeduplicationId = Guid.NewGuid().ToString()
        };

        var response = await snsClient.PublishAsync(request, ct);
        return response.MessageId;
    }
}
