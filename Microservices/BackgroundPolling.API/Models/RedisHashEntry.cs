using StackExchange.Redis;

namespace BackgroundPolling.API.Models
{
    public class RedisHashEntry
    {
        public RedisValue Name { get; }
        public RedisValue Value { get; }
        public RedisValue Key { get; }
    }
}
