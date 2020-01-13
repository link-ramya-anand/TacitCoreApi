using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ServiceStack.Redis;

namespace TacitCoreDemo.Services
{
    public static class RedisRepository 
    {
        private static RedisClient _redisClient;
                
        public static RedisClient GetInstance(string connectionString)
        {
            if (_redisClient == null)
            {
                _redisClient = new RedisClient(connectionString);
                _redisClient.FlushAll();
            }
            return _redisClient;
        }
    }
}
