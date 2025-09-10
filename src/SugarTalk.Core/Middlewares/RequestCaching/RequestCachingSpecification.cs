﻿using System;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Mediator.Net.Pipeline;
using Newtonsoft.Json;
using Serilog;
using SugarTalk.Core.Services.Caching;
using SugarTalk.Messages.Enums.Caching;
using SugarTalk.Messages.Requests;

namespace SugarTalk.Core.Middlewares.RequestCaching;

public class RequestCachingSpecification<TContext> : IPipeSpecification<TContext> where TContext : IContext<IMessage>
{
    private readonly ICacheManager _cacheManager;

    public RequestCachingSpecification(ICacheManager cacheManager)
    {
        _cacheManager = cacheManager;
    }

    public bool ShouldExecute(TContext context, CancellationToken cancellationToken)
    {
        return context.Message is ICachingRequest;
    }

    public async Task BeforeExecute(TContext context, CancellationToken cancellationToken)
    {
        if (!ShouldExecute(context, cancellationToken)) return;
        
        var cachingRequest = (ICachingRequest)context.Message;
        var cacheKey = cachingRequest.GetCacheKey();
        
        var cachedJson = await _cacheManager.GetAsync<string>(cacheKey, CachingType.RedisCache, cancellationToken).ConfigureAwait(false);

        if (!string.IsNullOrEmpty(cachedJson))
        {
            context.Result = JsonConvert.DeserializeObject(cachedJson, context.ResultDataType);
            
            Log.Information("Request result from cached. Cache key: {CacheKey}, Result type: {ResultType}", cacheKey, context.ResultDataType.Name);
            
            throw new RequestCachingException();
        }
    }

    public Task Execute(TContext context, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public async Task AfterExecute(TContext context, CancellationToken cancellationToken)
    {
        if (!ShouldExecute(context, cancellationToken)) return;

        var cachingRequest = (ICachingRequest)context.Message;
        var cacheKey = cachingRequest.GetCacheKey();

        Log.Information("Request caching. Cache key: {CacheKey}, Result type: {ResultType}", cacheKey, context.ResultDataType.Name);
        
        await _cacheManager.SetAsync(
            cacheKey, JsonConvert.SerializeObject(context.Result), CachingType.RedisCache, cachingRequest.GetCacheExpiration(), cancellationToken).ConfigureAwait(false);
    }

    public Task OnException(Exception ex, TContext context)
    {
        if (ex is RequestCachingException)
            return Task.CompletedTask;
        
        ExceptionDispatchInfo.Capture(ex).Throw();
        
        throw ex;
    }
}