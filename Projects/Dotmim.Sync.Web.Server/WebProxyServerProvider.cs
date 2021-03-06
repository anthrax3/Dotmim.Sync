﻿using System;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;
using Dotmim.Sync.Serialization;
using Dotmim.Sync.Web.Client;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System.IO.Compression;
using System.Text;
using Microsoft.AspNetCore.Hosting;

namespace Dotmim.Sync.Web.Server
{

    /// <summary>
    /// Class used when you have to deal with a Web Server
    /// </summary>
    public class WebProxyServerOrchestrator
    {
        public IMemoryCache Cache { get; }
        public WebServerProperties WebServerProperties { get; }
        public IHostingEnvironment Environment { get; }

        /// <summary>
        /// Default constructor for DI
        /// </summary>
        public WebProxyServerOrchestrator(IMemoryCache cache, WebServerProperties webServerProperties, IHostingEnvironment env)
        {
            this.Cache = cache;
            this.WebServerProperties = webServerProperties;
            this.Environment = env;
        }

        /// <summary>
        /// Gets or Sets a Web server orchestratot that will override the one from cache
        /// </summary>
        public WebServerOrchestrator WebServerOrchestrator { get; set; }


        /// <summary>
        /// Call this method to handle requests on the server, sent by the client
        /// </summary>
        public Task HandleRequestAsync(HttpContext context) =>
            HandleRequestAsync(context, null, CancellationToken.None);

        /// <summary>
        /// Call this method to handle requests on the server, sent by the client
        /// </summary>
        public Task HandleRequestAsync(HttpContext context, Action<RemoteOrchestrator> action) =>
            HandleRequestAsync(context, action, CancellationToken.None);

        /// <summary>
        /// Call this method to handle requests on the server, sent by the client
        /// </summary>
        public Task HandleRequestAsync(HttpContext context, CancellationToken token) =>
            HandleRequestAsync(context, null, token);

        /// <summary>
        /// Call this method to handle requests on the server, sent by the client
        /// </summary>
        public async Task HandleRequestAsync(HttpContext context, Action<RemoteOrchestrator> action, CancellationToken cancellationToken)
        {
            var httpRequest = context.Request;
            var httpResponse = context.Response;
            var serAndsizeString = string.Empty;
            var converter = string.Empty;

            if (httpRequest.Method.ToLowerInvariant() == "get")
            {
                await this.WriteHelloAsync(context, cancellationToken);
                return;
            }

            // Get the serialization and batch size format
            if (TryGetHeaderValue(context.Request.Headers, "dotmim-sync-serialization-format", out var vs))
                serAndsizeString = vs.ToLowerInvariant();

            // Get the serialization and batch size format
            if (TryGetHeaderValue(context.Request.Headers, "dotmim-sync-converter", out var cs))
                converter = cs.ToLowerInvariant();

            if (!TryGetHeaderValue(context.Request.Headers, "dotmim-sync-session-id", out var sessionId))
                throw new HttpHeaderMissingExceptiopn("dotmim-sync-session-id");

            if (!TryGetHeaderValue(context.Request.Headers, "dotmim-sync-step", out string iStep))
                throw new HttpHeaderMissingExceptiopn("dotmim-sync-step");

            var step = (HttpStep)Convert.ToInt32(iStep);
            var readableStream = new MemoryStream();

            try
            {
                // Copty stream to a readable and seekable stream
                // HttpRequest.Body is a HttpRequestStream that is readable but can't be Seek
                await httpRequest.Body.CopyToAsync(readableStream);
                httpRequest.Body.Close();
                httpRequest.Body.Dispose();

                // if Hash is present in header, check hash
                if (TryGetHeaderValue(context.Request.Headers, "dotmim-sync-hash", out string hashStringRequest))
                    HashAlgorithm.SHA256.EnsureHash(readableStream, hashStringRequest);

                // Create a web server remote orchestrator based on cache values
                if (this.WebServerOrchestrator == null)
                    this.WebServerOrchestrator = this.WebServerProperties.CreateWebServerOrchestrator();

                // try get schema from cache
                if (this.Cache.TryGetValue<SyncSet>(WebServerProperties.Key, out var cachedSchema))
                    this.WebServerOrchestrator.Schema = cachedSchema;

                // try get session cache from current sessionId
                if (!this.Cache.TryGetValue<SessionCache>(sessionId, out var sessionCache))
                    sessionCache = new SessionCache();

                // action from user if available
                action?.Invoke(this.WebServerOrchestrator);

                // Get the serializer and batchsize
                (var clientBatchSize, var clientSerializerFactory) = GetClientSerializer(serAndsizeString, this.WebServerOrchestrator);

                // Get converter used by client
                // Can be null
                var clientConverter = GetClientConverter(converter, this.WebServerOrchestrator);
                this.WebServerOrchestrator.ClientConverter = clientConverter;

                byte[] binaryData = null;
                switch (step)
                {
                    case HttpStep.EnsureScopes:
                        var m1 = await clientSerializerFactory.GetSerializer<HttpMessageEnsureScopesRequest>().DeserializeAsync(readableStream);
                        var s1 = await this.WebServerOrchestrator.EnsureScopeAsync(m1, sessionCache, cancellationToken).ConfigureAwait(false);
                        binaryData = await clientSerializerFactory.GetSerializer<HttpMessageEnsureScopesResponse>().SerializeAsync(s1);
                        await this.WebServerOrchestrator.Provider.InterceptAsync(new HttpMessageEnsureScopesResponseArgs(binaryData)).ConfigureAwait(false);
                        break;
                    case HttpStep.SendChanges:
                        var m2 = await clientSerializerFactory.GetSerializer<HttpMessageSendChangesRequest>().DeserializeAsync(readableStream);
                        var s2 = await this.WebServerOrchestrator.ApplyThenGetChangesAsync(m2, sessionCache, clientBatchSize, cancellationToken).ConfigureAwait(false);
                        binaryData = await clientSerializerFactory.GetSerializer<HttpMessageSendChangesResponse>().SerializeAsync(s2);
                        if (s2.Changes != null && s2.Changes.HasRows)
                            await this.WebServerOrchestrator.Provider.InterceptAsync(new HttpMessageSendChangesResponseArgs(binaryData)).ConfigureAwait(false);
                        break;
                    case HttpStep.GetChanges:
                        var m3 = await clientSerializerFactory.GetSerializer<HttpMessageGetMoreChangesRequest>().DeserializeAsync(readableStream);
                        var s3 = await this.WebServerOrchestrator.GetMoreChangesAsync(m3, sessionCache, cancellationToken);
                        binaryData = await clientSerializerFactory.GetSerializer<HttpMessageSendChangesResponse>().SerializeAsync(s3);
                        if (s3.Changes != null && s3.Changes.HasRows)
                            await this.WebServerOrchestrator.Provider.InterceptAsync(new HttpMessageSendChangesResponseArgs(binaryData)).ConfigureAwait(false);
                        break;
                    case HttpStep.GetSnapshot:
                        var m4 = await clientSerializerFactory.GetSerializer<HttpMessageSendChangesRequest>().DeserializeAsync(readableStream);
                        var s4 = await this.WebServerOrchestrator.GetSnapshotAsync(m4, sessionCache, cancellationToken);
                        binaryData = await clientSerializerFactory.GetSerializer<HttpMessageSendChangesResponse>().SerializeAsync(s4);
                        if (s4.Changes != null && s4.Changes.HasRows)
                            await this.WebServerOrchestrator.Provider.InterceptAsync(new HttpMessageSendChangesResponseArgs(binaryData)).ConfigureAwait(false);
                        break;
                }

                // Save schema to cache with a sliding expiration
                this.Cache.Set(WebServerProperties.Key, this.WebServerOrchestrator.Schema, this.WebServerProperties.Options.GetServerCacheOptions());

                // Save session client to cache with a sliding expiration
                this.Cache.Set(sessionId, sessionCache, this.WebServerProperties.Options.GetClientCacheOptions());

                // Adding the serialization format used and session id
                httpResponse.Headers.Add("dotmim-sync-session-id", sessionId.ToString());
                httpResponse.Headers.Add("dotmim-sync-serialization-format", clientSerializerFactory.Key);

                // calculate hash
                var hash = HashAlgorithm.SHA256.Create(binaryData);
                var hashString = Convert.ToBase64String(hash);
                // Add hash to header
                httpResponse.Headers.Add("dotmim-sync-hash", hashString);

                // data to send back, as the response
                byte[] data = this.EnsureCompression(httpRequest, httpResponse, binaryData);

                await httpResponse.Body.WriteAsync(data, 0, data.Length).ConfigureAwait(false);

            }
            catch (Exception ex)
            {
                await WriteExceptionAsync(httpResponse, ex);
            }
            finally
            {
                readableStream.Close();
                readableStream.Dispose();
            }
        }

        private async Task WriteHelloAsync(HttpContext context, CancellationToken cancellationToken)
        {
            var httpRequest = context.Request;
            var httpResponse = context.Response;
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("<H1>Sync Configuration:</H1>");

            if (this.Environment.IsDevelopment())
            {
                var s = JsonConvert.SerializeObject(this.WebServerProperties, Formatting.Indented);

                stringBuilder.AppendLine("<div>Web Server properties:</div>");
                stringBuilder.AppendLine("<script src='https://cdn.jsdelivr.net/gh/google/code-prettify@master/loader/run_prettify.js'></script>");
                stringBuilder.AppendLine("<pre class='prettyprint'>");
                stringBuilder.AppendLine(s);
                stringBuilder.AppendLine("</pre>");
            }
            else
            {
                stringBuilder.AppendLine("<div>Server is configured to Production mode. No options displayed.</div>");
            }

            await httpResponse.WriteAsync(stringBuilder.ToString(), cancellationToken);


        }

        /// <summary>
        /// Ensure we have a Compression setting or not
        /// </summary>
        private byte[] EnsureCompression(HttpRequest httpRequest, HttpResponse httpResponse, byte[] binaryData)
        {
            string encoding = httpRequest.Headers["Accept-Encoding"];

            // Compress data if client accept Gzip / Deflate
            if (!string.IsNullOrEmpty(encoding) && (encoding.Contains("gzip") || encoding.Contains("deflate")))
            {
                httpResponse.Headers.Add("Content-Encoding", "gzip");

                using (var writeSteam = new MemoryStream())
                {
                    using (var compress = new GZipStream(writeSteam, CompressionMode.Compress))
                    {
                        compress.Write(binaryData, 0, binaryData.Length);
                    }

                    return writeSteam.ToArray();
                }

            }

            return binaryData;
        }

        /// <summary>
        /// Get an instance of SyncMemoryProvider depending on session id. If the entry for session id does not exists, create a new one
        /// </summary>
        //private static WebServerOrchestrator GetCachedOrchestrator(HttpContext context, string syncSessionId)
        //{
        //    WebServerOrchestrator remoteOrchestrator;

        //    var cache = context.RequestServices.GetService<IMemoryCache>();
        //    if (cache == null)
        //        throw new HttpCacheNotConfiguredException();

        //    if (string.IsNullOrWhiteSpace(syncSessionId))
        //        throw new ArgumentNullException(nameof(syncSessionId));

        //    // get the sync provider associated with the session id
        //    remoteOrchestrator = cache.Get(syncSessionId) as WebServerOrchestrator;

        //    return remoteOrchestrator;
        //}

        /// <summary>
        /// Add a new instance of SyncMemoryProvider, created by DI
        /// </summary>
        /// <returns></returns>
        //private static WebServerOrchestrator AddNewOrchestratorToCacheFromDI(HttpContext context, string syncSessionId)
        //{
        //    var cache = context.RequestServices.GetService<IMemoryCache>();

        //    if (cache == null)
        //        throw new HttpCacheNotConfiguredException();

        //    var remoteOrchestrator = DependencyInjection.GetNewOrchestrator();
        //    cache.Set(syncSessionId, remoteOrchestrator, TimeSpan.FromHours(1));

        //    return remoteOrchestrator;
        //}


        //private static WebServerOrchestrator AddNewOrchestratorToCache(HttpContext context, CoreProvider provider, SyncSetup setup, string sessionId, WebServerOptions options = null)
        //{
        //    WebServerOrchestrator remoteOrchestrator;
        //    var cache = context.RequestServices.GetService<IMemoryCache>();

        //    if (cache == null)
        //        throw new HttpCacheNotConfiguredException();

        //    remoteOrchestrator = new WebServerOrchestrator(provider, options, setup);

        //    cache.Set(sessionId, remoteOrchestrator, TimeSpan.FromHours(1));
        //    return remoteOrchestrator;
        //}

        //private static WebServerOrchestrator AddNewWebServerOrchestratorToCache(HttpContext context, WebServerOrchestrator webServerOrchestrator, string sessionId)
        //{
        //    var cache = context.RequestServices.GetService<IMemoryCache>();

        //    if (cache == null)
        //        throw new HttpCacheNotConfiguredException();

        //    cache.Set(sessionId, webServerOrchestrator, TimeSpan.FromHours(1));
        //    return webServerOrchestrator;
        //}



        private (int clientBatchSize, ISerializerFactory clientSerializer) GetClientSerializer(string serAndsizeString, WebServerOrchestrator serverOrchestrator)
        {
            try
            {
                if (string.IsNullOrEmpty(serAndsizeString))
                    throw new Exception();

                var serAndsize = JsonConvert.DeserializeAnonymousType(serAndsizeString, new { f = "", s = 0 });

                var clientBatchSize = serAndsize.s;
                var clientSerializerFactory = serverOrchestrator.Options.Serializers[serAndsize.f];

                return (clientBatchSize, clientSerializerFactory);
            }
            catch
            {
                throw new HttpSerializerNotConfiguredException(serverOrchestrator.Options.Serializers.Select(sf => sf.Key));
            }
        }

        private IConverter GetClientConverter(string cs, WebServerOrchestrator serverOrchestrator)
        {
            try
            {
                if (string.IsNullOrEmpty(cs))
                    return null;

                var clientConverter = serverOrchestrator.Options.Converters.First(c => c.Key == cs);

                return clientConverter;
            }
            catch
            {
                throw new HttpConverterNotConfiguredException(serverOrchestrator.Options.Converters.Select(sf => sf.Key));
            }
        }



        /// <summary>
        /// Clean up memory cache object for specified session id
        /// </summary>
        private static void Cleanup(object memoryCache, string syncSessionId)
        {
            if (memoryCache == null || string.IsNullOrWhiteSpace(syncSessionId)) return;
            Task.Run(() =>
            {
                try
                {
                    (memoryCache as IMemoryCache)?.Remove(syncSessionId);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            });
        }

        /// <summary>
        /// Write exception to output message
        /// </summary>
        public async Task WriteExceptionAsync(HttpResponse httpResponse, Exception ex)
        {
            // Check if it's an unknown error, not managed (yet)
            if (!(ex is SyncException syncException))
                syncException = new SyncException(ex);


            var webException = new WebSyncException
            {
                Message = syncException.Message,
                SyncStage = syncException.SyncStage,
                TypeName = syncException.TypeName,
                DataSource = syncException.DataSource,
                InitialCatalog = syncException.InitialCatalog,
                Number = syncException.Number,
                Side = syncException.Side
            };

            var webXMessage = JsonConvert.SerializeObject(webException);

            httpResponse.StatusCode = StatusCodes.Status400BadRequest;
            httpResponse.ContentLength = webXMessage.Length;
            await httpResponse.WriteAsync(webXMessage);
        }


        public static bool TryGetHeaderValue(IHeaderDictionary n, string key, out string header)
        {
            if (n.TryGetValue(key, out var vs))
            {
                header = vs[0];
                return true;
            }

            header = null;
            return false;
        }

        //public Stream GetBody(HttpRequest r) => r.Body;
        //public Stream GetBody(HttpResponse r) => r.Body;

    }

}
