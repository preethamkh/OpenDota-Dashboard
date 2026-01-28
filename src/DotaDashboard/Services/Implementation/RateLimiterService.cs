namespace DotaDashboard.Services.Implementation;

/// <summary>
/// Rate limiter to ensure we stay within OpenDota API limits:
/// - 60 calls per minute
/// </summary>
public class RateLimiterService(int maxCallsPerMinute = 60)
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly Queue<DateTime> _callTimestamps = new();

    /// <summary>
    /// Waits until a slot is available to make an API call
    /// </summary>
    /// <returns></returns>
    public async Task WaitForSlotAsync()
    {
        // Ensures only one thread can execute this logic at a time
        // Prevents race conditions when multiple API calls happen simultaneously
        await _semaphore.WaitAsync();
        try
        {
            var now = DateTime.UtcNow;
            var oneMinuteAgo = now.AddMinutes(-1);

            // Remove timestamps older than 1 minute
            // Peek - checks oldest timestamp without removing it
            // Sliding window approach
            while (_callTimestamps.Count > 0 && _callTimestamps.Peek() < oneMinuteAgo)
            {
                _callTimestamps.Dequeue();
            }

            // If we're at the rate limit, wait until the oldest call expires
            if (_callTimestamps.Count >= maxCallsPerMinute)
            {
                var oldestCall = _callTimestamps.Peek();
                var waitTime = oldestCall.AddMinutes(1) - now;
                if (waitTime > TimeSpan.Zero)
                {
                    await Task.Delay(waitTime);
                }

                // Clean up again after waiting
                while (_callTimestamps.Count > 0 && _callTimestamps.Peek() < DateTime.UtcNow.AddMinutes(-1))
                {
                    _callTimestamps.Dequeue();
                }
            }

            // Record this call
            _callTimestamps.Enqueue(DateTime.UtcNow);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public int GetCallsInLastMinute()
    {
        var oneMinuteAgo = DateTime.UtcNow.AddMinutes(-1);
        return _callTimestamps.Count(t => t >= oneMinuteAgo);
    }
}