using System.Text.Json;
using Elsa.QuizSocket.Events;
using Microsoft.AspNetCore.SignalR;
using StackExchange.Redis;

namespace Elsa.QuizSocket;

public interface IRedisSubscriptionService
{
    Task StartAsync();
    Task StopAsync();
}

public class RedisSubscriptionService : IRedisSubscriptionService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IHubContext<QuizHub> _hubContext;
    private readonly IQuizConnectionManager _connectionManager;
    private readonly ILogger<RedisSubscriptionService> _logger;
    private ISubscriber? _subscriber;

    public RedisSubscriptionService(
        IConnectionMultiplexer redis,
        IHubContext<QuizHub> hubContext,
        IQuizConnectionManager connectionManager,
        ILogger<RedisSubscriptionService> logger)
    {
        _redis = redis;
        _hubContext = hubContext;
        _connectionManager = connectionManager;
        _logger = logger;
    }

    public async Task StartAsync()
    {
        try
        {
            _subscriber = _redis.GetSubscriber();
            
            // Subscribe to points updates pattern
            await _subscriber.SubscribeAsync(
                new RedisChannel("quiz:*:points_updates", RedisChannel.PatternMode.Pattern),
                async (channel, message) =>
                {
                    try
                    {
                        var scoreUpdate = JsonSerializer.Deserialize<QuizQuestionAnsweredEvent>(message);
                        if (scoreUpdate != null)
                        {
                            await HandlePointsUpdateAsync(scoreUpdate);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing points update from Redis");
                    }
                });

            _logger.LogInformation("Redis subscription service started successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start Redis subscription service");
            throw;
        }
    }

    public async Task StopAsync()
    {
        if (_subscriber != null)
        {
            await _subscriber.UnsubscribeAllAsync();
            _logger.LogInformation("Redis subscription service stopped");
        }
    }

    private async Task HandlePointsUpdateAsync(QuizQuestionAnsweredEvent @event)
    {
        try
        {
            var connections = _connectionManager.GetConnectionsForQuiz(@event.QuizId.ToString());
            
            if (connections.Any())
            {
                await _hubContext.Clients.Clients(connections).SendAsync("UserPointsUpdated", new
                {
                    UserId = @event.UserId,
                    QuizId = @event.QuizId,
                    QuestionId = @event.QuestionId,
                    IsCorrect = @event.IsCorrect,
                    PointsEarned = @event.PointsEarned,
                    TotalPointsEarned = @event.TotalPointsEarned
                });

                _logger.LogDebug($"Score update broadcasted to {connections.Count()} connections for quiz {@event.QuizId}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling score update");
        }
    }
}