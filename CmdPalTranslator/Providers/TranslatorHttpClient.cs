using Microsoft.Extensions.Http;
using Polly;
using Polly.Extensions.Http;
using System;
using System.Net;
using System.Net.Http;

namespace CmdPalTranslator.Providers
{
    internal static class TranslatorHttpClient
    {
        public static HttpClient Create()
        {
            SocketsHttpHandler socketHandler = new()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
            };

            IAsyncPolicy<HttpResponseMessage> retryPolicy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(2, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

            PolicyHttpMessageHandler policyHandler = new(retryPolicy)
            {
                InnerHandler = socketHandler,
            };

            HttpClient client = new(policyHandler)
            {
                Timeout = TimeSpan.FromSeconds(30),
            };
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) CmdPalTranslator/1.0");
            return client;
        }
    }
}
