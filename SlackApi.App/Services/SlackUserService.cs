using Microsoft.Extensions.Logging;
using SlackApi.App.HttpClients;
using SlackApi.Core.Extensions;
using SlackApi.Domain.Constants;
using SlackApi.Domain.SlackDTOs;
using SlackApi.Repository.TableStorage;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SlackApi.App.Services
{
    public class SlackUserService : ISlackUserService
    {
        private readonly ITableStorageService _tableStorageService;
        private readonly SlackApiClient _slackApiClient;
        private readonly ILogger<SlackUserService> _logger;

        public SlackUserService(ITableStorageService tableStorageService,
            SlackApiClient slackApiClient,
            ILogger<SlackUserService> logger)
        {
            _tableStorageService = tableStorageService;
            _slackApiClient = slackApiClient;
            _logger = logger;
        }

        public async Task<List<SlackUserTableEntity>> GetWorkspaceUsers(PagedSlackRequestDTO requestDTO)
        {
            if (requestDTO == null || requestDTO?.TeamId == null)
                throw new ArgumentNullException(nameof(requestDTO));

            var slackApiResponse = new SlackApiResponse
            {
                ResponseMetadata = new ResponseMetadata()
            };

            var workspaceUsers = new List<SlackUserTableEntity>();
            var hasErrored = false;

            try
            {
                do
                {
                    slackApiResponse = await _slackApiClient.SlackGetRequest(SlackEndpoints.UsersList,
                        new Dictionary<string, string?>
                        {
                        { "team_id", requestDTO.TeamId },
                        { "cursor", slackApiResponse.ResponseMetadata.NextCursor },
                        { "limit", requestDTO.Limit.ToString() },
                        { "include_locale", requestDTO.Locale.ToString() }
                        });

                    if (slackApiResponse == null
                        || slackApiResponse?.Ok == false
                        || slackApiResponse?.Members == null
                        || slackApiResponse.Members.IsNullOrEmpty())
                        throw new Exception($"Unable to retrieve users for team {requestDTO.TeamId}!");

                    workspaceUsers.AddRange(slackApiResponse
                        .Members
                        .Where(m => m.TeamId != null && m.Id != null)
                        .Select(m => new SlackUserTableEntity($"{m.TeamId}", $"{m.Id}")
                        {
                            RealName = m.RealName,
                            DisplayName = m.Profile?.DisplayName,
                            Email = m.Profile?.Email,
                            Image512 = m.Profile?.Image512,
                            IsBot = m.IsBot
                        }));

                } while (slackApiResponse?.ResponseMetadata?.NextCursor != null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                hasErrored = true;
            }
            
            if (requestDTO.IsSavingOnFetch == true)
                await _tableStorageService.UpsertCollectionAsync(workspaceUsers, TableStorageTable.TeamUsers);

            if (hasErrored)
                return await _tableStorageService
                    .GetAllByPartitionKeyAsync<SlackUserTableEntity>(TableStorageTable.TeamUsers, requestDTO.TeamId);

            return workspaceUsers;
        }
    }

    public interface ISlackUserService
    {
        Task<List<SlackUserTableEntity>> GetWorkspaceUsers(PagedSlackRequestDTO requestDTO);
    }

    public class PagedSlackRequestDTO
    {

        [JsonPropertyName("team_id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? TeamId { get; set; }

        [JsonPropertyName("cursor")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Cursor { get; set; }

        [JsonPropertyName("include_locale")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? Locale { get; set; }

        [JsonPropertyName("accessory")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int Limit { get; set; } = 150;

        public bool IsSavingOnFetch { get; set; } = false;
    }
}
