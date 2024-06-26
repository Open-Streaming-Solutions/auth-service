﻿using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace AuthorizationService.Services;

public interface ICachingService
{
	public Task SetRecordAsync(string key, string value,
		TimeSpan? absoluteExpireTime = null,
		TimeSpan? unusedExpireTime = null, CancellationToken? cancellationToken = null);
    
	public Task SetRecordAsync<T>(string key, T value,
		TimeSpan? absoluteExpireTime = null,
		TimeSpan? unusedExpireTime = null, CancellationToken? cancellationToken = null);
    
	public Task<string?> GetRecordAsync(string key, CancellationToken? cancellationToken = null);
    
	public Task<T?> GetRecordAsync<T>(string key, CancellationToken? cancellationToken = null);
    
	public Task RemoveRecord(string key, CancellationToken? cancellationToken = null);
}

public class CachingService(IDistributedCache distributedCache) : ICachingService
{
    private readonly IDistributedCache _distributedCache = distributedCache;

    public async Task SetRecordAsync(string key, string value, TimeSpan? absoluteExpireTime = null,
        TimeSpan? unusedExpireTime = null, CancellationToken? cancellationToken = null)
    {
        var options = new DistributedCacheEntryOptions
        {
            SlidingExpiration = unusedExpireTime,
            AbsoluteExpirationRelativeToNow = absoluteExpireTime ?? TimeSpan.FromSeconds(60)
        };
        
        await _distributedCache.SetStringAsync(key, value, options, cancellationToken ?? default);
    }
    
    public async Task SetRecordAsync<T>(string key, T value, TimeSpan? absoluteExpireTime = null,
        TimeSpan? unusedExpireTime = null, CancellationToken? cancellationToken = null)
    {
        var options = new DistributedCacheEntryOptions
        {
            SlidingExpiration = unusedExpireTime,
            AbsoluteExpirationRelativeToNow = absoluteExpireTime ?? TimeSpan.FromSeconds(60)
        };
        
        var jsonData = JsonSerializer.Serialize(value);
        await _distributedCache.SetStringAsync(key, jsonData, options, cancellationToken ?? default);
    }
    
    public async Task<string?> GetRecordAsync(string key, CancellationToken? cancellationToken = null)
    {
        return await _distributedCache.GetStringAsync(key, cancellationToken ?? default);
    }
    
    public async Task<T?> GetRecordAsync<T>(string key, CancellationToken? cancellationToken = null)
    {
        var jsonData = await _distributedCache.GetStringAsync(key, cancellationToken ?? default);
        
        return jsonData is null
            ? default
            : JsonSerializer.Deserialize<T>(jsonData);
    }
    
    public async Task RemoveRecord(string key, CancellationToken? cancellationToken = null)
    {
        await _distributedCache.RemoveAsync(key, cancellationToken ?? default);
    }
}
