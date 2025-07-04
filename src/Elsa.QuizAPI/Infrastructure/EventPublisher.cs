using System.Text.Json;
using StackExchange.Redis;

namespace Elsa.QuizAPI.Infrastructure;

public interface IEventPublisher
{
    Task PublishScoreUpdateAsync(ScoreUpdateEvent scoreUpdate);
}

public class RedisEventPublisher : IEventPublisher
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisEventPublisher> _logger;
    
    public RedisEventPublisher(IConnectionMultiplexer redis, ILogger<RedisEventPublisher> logger)
    {
        _redis = redis;
        _logger = logger;
    }
    
    public async Task PublishScoreUpdateAsync(ScoreUpdateEvent scoreUpdate)
    {
        try
        {
            var database = _redis.GetDatabase();
            var channel = $"quiz:{scoreUpdate.QuizId}:score_updates";
            var message = JsonSerializer.Serialize(scoreUpdate);
            
            await database.PublishAsync(channel, message);
            _logger.LogDebug($"Published score update for user {scoreUpdate.UserId} in quiz {scoreUpdate.QuizId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish score update");
        }
    }
}

public class ScoreUpdateEvent
{
    public string UserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string QuizId { get; set; } = string.Empty;
    public int NewScore { get; set; }
    public int PointsEarned { get; set; }
    public bool IsCorrect { get; set; }
    public string QuestionId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}