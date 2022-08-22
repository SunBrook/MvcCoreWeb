using System;
using System.Text.Json;
using Microsoft.Extensions.Caching.Redis;
using Microsoft.Extensions.Caching.Distributed;


namespace MvcCoreWeb.Tools
{
    public class RedisCacheHelper
    {
        private static RedisCache _redisCache = null;
        private static RedisCacheOptions options = null;

        /// <summary>
        /// 初始化 Redis
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="instanceName"></param>
        public RedisCacheHelper(string connectionString, string instanceName)
        {
            options = new RedisCacheOptions
            {
                Configuration = connectionString,
                InstanceName = instanceName,
            };
            _redisCache = new RedisCache(options);
        }

        public static bool SetStringValue(string key, string value, int? exprieTime = null)
        {
            try
            {
                if (exprieTime.HasValue)
                {
                    _redisCache.SetString(key, value, new DistributedCacheEntryOptions
                    {
                        AbsoluteExpiration = DateTime.Now.AddSeconds(exprieTime.Value)
                    });
                }
                else
                {
                    _redisCache.SetString(key, value);
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static string GetStringValue(string key)
        {
            try
            {
                return _redisCache.GetString(key);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static T Get<T>(string key)
        {
            try
            {
                string value = GetStringValue(key);
                if (string.IsNullOrEmpty(value))
                {
                    return default;
                }
                else
                {
                    var obj = JsonSerializer.Deserialize<T>(value);
                    return obj;
                }
            }
            catch (Exception)
            {
                return default;
            }
        }

        public static bool Set<T>(string key, T value, int? exprieTime = null)
        {
            try
            {
                var valueStr = JsonSerializer.Serialize(value);
                return SetStringValue(key, valueStr, exprieTime);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool Remove(string key)
        {
            try
            {
                _redisCache.Remove(key);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 刷新数据
        /// </summary>
        /// <returns></returns>
        public static bool Refresh(string key)
        {
            try
            {
                _redisCache.Refresh(key);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="exprieTime"></param>
        /// <returns></returns>
        public static bool Replace(string key, string value, int? exprieTime = null)
        {
            try
            {
                return SetStringValue(key, value, exprieTime);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool Replace<T>(string key, T value, int? exprieTime = null)
        {
            try
            {
                var valueStr = JsonSerializer.Serialize(value);
                return SetStringValue(key, valueStr, exprieTime);
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
