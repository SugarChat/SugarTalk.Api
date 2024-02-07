using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Newtonsoft.Json;
using Serilog;
using SugarTalk.Core.Ioc;

namespace SugarTalk.Core.Services.Http;

public interface ISugarTalkHttpClientFactory : IScopedDependency
{
    Task<T> GetAsync<T>(string requestUrl, CancellationToken cancellationToken, 
        TimeSpan? timeout = null, bool beginScope = false, Dictionary<string, string> headers = null, HttpClient innerClient = null);

    Task<T> PostAsync<T>(string requestUrl, HttpContent content, CancellationToken cancellationToken, 
        TimeSpan? timeout = null, bool beginScope = false, Dictionary<string, string> headers = null);
    
    Task<T> PostAsJsonAsync<T>(string requestUrl, object value, CancellationToken cancellationToken, 
        TimeSpan? timeout = null, bool beginScope = false, Dictionary<string, string> headers = null);
    
    Task<T> PutAsync<T>(string requestUrl, HttpContent content, CancellationToken cancellationToken, 
        TimeSpan? timeout = null, bool beginScope = false, Dictionary<string, string> headers = null);
    
    Task<T> DeleteAsync<T>(string requestUrl, CancellationToken cancellationToken, 
        TimeSpan? timeout = null, bool beginScope = false, Dictionary<string, string> headers = null);
    
    Task<T> PostAsMultipartAsync<T>(
        string requestUrl, Dictionary<string, string> formData, Dictionary<string, (byte[], string)> fileData,
        CancellationToken cancellationToken, TimeSpan? timeout = null, bool beginScope = false, Dictionary<string, string> headers = null);
}

public class SugarTalkHttpClientFactory : ISugarTalkHttpClientFactory
{
    private readonly ILifetimeScope _scope;

    public SugarTalkHttpClientFactory(ILifetimeScope scope)
    {
        _scope = scope;
    }
    
    private HttpClient CreateClient(TimeSpan? timeout = null, bool beginScope = false,
        Dictionary<string, string> headers = null, HttpClient innerClient = null)
    {
        if (innerClient != null) return innerClient;

        var scope = beginScope ? _scope.BeginLifetimeScope() : _scope;

        var canResolve = scope.TryResolve(out IHttpClientFactory httpClientFactory);

        var client = canResolve ? httpClientFactory.CreateClient() : new HttpClient();

        if (timeout != null)
            client.Timeout = timeout.Value;

        if (headers == null) return client;

        foreach (var header in headers)
        {
            client.DefaultRequestHeaders.Add(header.Key, header.Value);
        }

        return client;
    }

    public async Task<T> GetAsync<T>(string requestUrl, CancellationToken cancellationToken,
        TimeSpan? timeout = null, bool beginScope = false, Dictionary<string, string> headers = null, HttpClient innerClient = null)
    {
        return await SafelyProcessRequestAsync(requestUrl, async () =>
        {
            var response = await CreateClient(timeout: timeout, beginScope: beginScope, headers: headers)
                .GetAsync(requestUrl, cancellationToken).ConfigureAwait(false);
            
            return await ReadAndLogResponseAsync<T>(requestUrl, HttpMethod.Get, response, cancellationToken).ConfigureAwait(false);
            
        }, cancellationToken).ConfigureAwait(false);
    }

    public async Task<T> PostAsync<T>(string requestUrl, HttpContent content, CancellationToken cancellationToken, 
        TimeSpan? timeout = null, bool beginScope = false, Dictionary<string, string> headers = null)
    {
        return await SafelyProcessRequestAsync(requestUrl, async () =>
        {
            var response = await CreateClient(timeout: timeout, beginScope: beginScope, headers: headers)
                .PostAsync(requestUrl, content, cancellationToken).ConfigureAwait(false);

            return await ReadAndLogResponseAsync<T>(requestUrl, HttpMethod.Post, response, cancellationToken).ConfigureAwait(false);
            
        }, cancellationToken).ConfigureAwait(false);
    }

    public async Task<T> PostAsJsonAsync<T>(string requestUrl, object value, CancellationToken cancellationToken, 
        TimeSpan? timeout = null, bool beginScope = false, Dictionary<string, string> headers = null)
    {
        return await SafelyProcessRequestAsync(requestUrl, async () =>
        {
            var response = await CreateClient(timeout: timeout, beginScope: beginScope, headers: headers)
                .PostAsJsonAsync(requestUrl, value, cancellationToken).ConfigureAwait(false);
            
            return await ReadAndLogResponseAsync<T>(requestUrl, HttpMethod.Post, response, cancellationToken).ConfigureAwait(false);
            
        }, cancellationToken).ConfigureAwait(false);
    }
    
    public async Task<T> PutAsync<T>(string requestUrl, HttpContent content, CancellationToken cancellationToken, 
        TimeSpan? timeout = null, bool beginScope = false, Dictionary<string, string> headers = null)
    {
        return await SafelyProcessRequestAsync(requestUrl, async () =>
        {
            var response = await CreateClient(timeout: timeout, beginScope: beginScope, headers: headers)
                .PutAsync(requestUrl, content, cancellationToken).ConfigureAwait(false);

            return await ReadAndLogResponseAsync<T>(requestUrl, HttpMethod.Put, response, cancellationToken).ConfigureAwait(false);
            
        }, cancellationToken).ConfigureAwait(false);
    }

    public async Task<T> DeleteAsync<T>(string requestUrl, CancellationToken cancellationToken,
        TimeSpan? timeout = null, bool beginScope = false, Dictionary<string, string> headers = null)
    {
        return await SafelyProcessRequestAsync(requestUrl, async () =>
        {
            var response = await CreateClient(timeout: timeout, beginScope: beginScope, headers: headers)
                .DeleteAsync(requestUrl, cancellationToken).ConfigureAwait(false);

            return await ReadAndLogResponseAsync<T>(requestUrl, HttpMethod.Delete, response, cancellationToken).ConfigureAwait(false);
            
        }, cancellationToken).ConfigureAwait(false);
    }

    public async Task<T> PostAsMultipartAsync<T>(
        string requestUrl, Dictionary<string, string> formData, Dictionary<string, (byte[], string)> fileData,
        CancellationToken cancellationToken, TimeSpan? timeout = null, bool beginScope = false, Dictionary<string, string> headers = null)
    {
        return await SafelyProcessRequestAsync(requestUrl, async () =>
        {
            var multipartContent = new MultipartFormDataContent();

            foreach (var data in formData)
            {
                multipartContent.Add(new StringContent(data.Value), data.Key);
            }

            foreach (var file in fileData)
            {
                multipartContent.Add(new ByteArrayContent(file.Value.Item1), file.Key, file.Value.Item2);
            }

            var request = new HttpRequestMessage(HttpMethod.Post, requestUrl)
            {
                Content = multipartContent
            };

            var response = await CreateClient(timeout: timeout, beginScope: beginScope, headers: headers)
                .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);

            return await ReadAndLogResponseAsync<T>(requestUrl, HttpMethod.Post, response, cancellationToken).ConfigureAwait(false);

        }, cancellationToken).ConfigureAwait(false);
    }
    
    private static async Task<T> ReadAndLogResponseAsync<T>(string requestUrl, HttpMethod httpMethod, HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
            return await ReadResponseContentAs<T>(response, cancellationToken).ConfigureAwait(false);

        LogHttpError(requestUrl, httpMethod, response);

        return default;
    }

    private static async Task<T> ReadResponseContentAs<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (typeof(T) == typeof(string))
            return (T)(object) await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        if (typeof(T) == typeof(byte[]))
            return (T)(object) await response.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
        return await response.Content.ReadAsAsync<T>(cancellationToken).ConfigureAwait(false);
    }
    
    private static void LogHttpError(string requestUrl, HttpMethod httpMethod, HttpResponseMessage response)
    {
        Log.Error("SugarTalk http {Method} {Url} error, The response: {ResponseJson}", 
            httpMethod.ToString(), requestUrl, JsonConvert.SerializeObject(response));
    }
    
    private static async Task<T> SafelyProcessRequestAsync<T>(string requestUrl, Func<Task<T>> func, CancellationToken cancellationToken)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            return await func().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error on requesting {RequestUrl}", requestUrl);
            return default;
        }
    }
}