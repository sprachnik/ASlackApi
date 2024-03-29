﻿using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using SlackApi.App.JsonConverters;
using SlackApi.Core.Settings;
using SlackApi.Domain.SlackDTOs;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace SlackApi.App.HttpClients
{
    public class SlackApiClient
    {
        private readonly HttpClient _client;
        private readonly ApplicationSettings _applicationSettings;
        private readonly ILogger<SlackApiClient> _logger;
        private readonly AsyncRetryPolicy _policy;
        private readonly string _outgoingOAuthToken;
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            Converters =
            {
                new BlockConverter(),
                new ElementConverter()
            }
        };

        public SlackApiClient(HttpClient client,
            ApplicationSettings applicationSettings,
            ILogger<SlackApiClient> logger)
        {
            if (applicationSettings?.SlackSettings?.OutgoingOAuthToken == null)
                throw new ArgumentNullException(nameof(SlackSettings.OutgoingOAuthToken));

            _outgoingOAuthToken = applicationSettings.SlackSettings.OutgoingOAuthToken;
            _client = client;
            _applicationSettings = applicationSettings;
            _logger = logger;

            _policy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(
                    3,
                    retryAttempt => TimeSpan.FromMilliseconds(new Random().Next(50, 500)),
                    (exception, timeSpan, retries, context) =>
                    {
                        if (3 != retries)
                            return;

                        _logger.LogError(exception.Message);
                    });
        }

        public async Task<SlackApiResponse?> SlackGetRequest(string uri, 
            Dictionary<string, string?> queryParams)
        {
            if (string.IsNullOrEmpty(uri))
                throw new ArgumentNullException(nameof(uri));

            if (queryParams.Any())
                uri = QueryHelpers.AddQueryString(uri, queryParams);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(uri),
                Headers = 
                {
                    Authorization = new AuthenticationHeaderValue("Bearer", _outgoingOAuthToken),
                }
            };

            HttpResponseMessage? response = null;

            await _policy.ExecuteAsync(async () => {
                response = await _client.SendAsync(request);
                response.EnsureSuccessStatusCode();
            });

            if (response == null)
                throw new Exception($"{uri} failed to yield a valid response!");

            var content = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(content))
                throw new Exception($"{uri} failed to yield a valid response!");

            var slackApiResponse = JsonSerializer.Deserialize<SlackApiResponse?>(content, _jsonOptions);

            if (slackApiResponse?.Ok == false)
                _logger.LogError($"SlackPostRequest error(s) returned from Slack: {slackApiResponse.Error}", slackApiResponse);

            return slackApiResponse;
        }

        public async Task<SlackApiResponse?> SlackPostRequest<T>(string uri, T payload) where T : class
        {
            if (string.IsNullOrEmpty(uri))
                throw new Exception("uri cannot be null!");

            var messageString = JsonSerializer.Serialize(payload, _jsonOptions);
            using var stringContent = new StringContent(messageString, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(uri),
                Content = stringContent,
                Headers = {
                    Authorization = new AuthenticationHeaderValue("Bearer", _outgoingOAuthToken),
                }
            };

            HttpResponseMessage? response = null;

            await _policy.ExecuteAsync(async () => {
                response = await _client.SendAsync(request);
                response.EnsureSuccessStatusCode();
            });

            if (response == null)
                throw new Exception($"{uri} failed to yield a valid response!");

            var content = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(content))
                throw new Exception($"{uri} failed to yield a valid response!");

            var slackApiResponse = JsonSerializer.Deserialize<SlackApiResponse?>(content, _jsonOptions);

            if (slackApiResponse?.Ok == false)
                _logger.LogError($"SlackPostRequest error(s) returned from Slack: {slackApiResponse.Error}", slackApiResponse);

            return slackApiResponse;
        }
    }
}
