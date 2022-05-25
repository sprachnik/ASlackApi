using Microsoft.AspNetCore.Mvc;
using SlackApi.App.MemStore;
using SlackApi.Domain.BadgeDTOs;
using SlackApi.Domain.Constants;
using SlackApi.Repository.Cache;
using SlackApi.Repository.SQL;
using SlackApi.Repository.TableStorage;

namespace SlackApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SettingsController : ControllerBase
    {
        private readonly ILogger<SettingsController> _logger;
        private readonly IDapperRepository _repository;
        private readonly ITableStorageService _tableStorageService;
        private readonly ICache _cache;
        private readonly ITableStorageMemStore _tableStorageMemStore;

        public SettingsController(ILogger<SettingsController> logger,
            IDapperRepository repository,
            ITableStorageService tableStorageService,
            ICache cache,
            ITableStorageMemStore tableStorageMemStore)
        {
            _logger = logger;
            _repository = repository;
            _tableStorageService = tableStorageService;
            _cache = cache;
            _tableStorageMemStore = tableStorageMemStore;
        }

        [HttpGet]
        public async Task<ActionResult> InitMemStore()
        {
            await _tableStorageMemStore.Init<BadgeTableEntity>(TableStorageTable.Badges);

            return NoContent();
        }
    }
}