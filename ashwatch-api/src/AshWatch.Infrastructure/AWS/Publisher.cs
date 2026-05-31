using System.Text.Json;
using Amazon;
using Amazon.Runtime.CredentialManagement;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using AshWatch.Application.Contracts;
using AshWatch.Domain.Entities;
using Microsoft.Extensions.Configuration;

namespace AshWatch.Infrastructure.AWS;

public class Publisher(IConfiguration configuration) : IPublisher
{
    public async Task<string> PublishAsync(Log log, CancellationToken ct = default)
    {
        AmazonSimpleNotificationServiceClient snsClient = new(RegionEndpoint.SAEast1);
        
        PublishRequest request = new()
        {
            TopicArn = configuration["AWS:SNSTopicArn"],
            Message = JsonSerializer.Serialize(log),
            Subject = "AshWatchLogging",
            MessageGroupId = log.ProjectId.ToString(),
            MessageDeduplicationId = Guid.NewGuid().ToString()
        };

        try
        {
            var response = await snsClient.PublishAsync(request, ct);
            return response.MessageId.ToString();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}