using Azure.Data.Tables;
using Microsoft.AspNetCore.Mvc;
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

        public SettingsController(ILogger<SettingsController> logger,
            IDapperRepository repository,
            ITableStorageService tableStorageService)
        {
            _logger = logger;
            _repository = repository;
            _tableStorageService = tableStorageService;
        }

        [HttpGet]
        public async Task<ActionResult> Test()
        {
            var test = await _repository.QueryAsync<int>("Select top 10 AddressId from SalesLT.Address");

            var tableEntity = new TableEntity
            {
                PartitionKey = "Text",
                RowKey = "RoeKey"
            };

            await _tableStorageService.UpsertAsync(tableEntity, "table");
            var thing = await _tableStorageService.GetAllByQuery<TableEntity>("table", t => t.PartitionKey == "Text");

            return NoContent();
        }
    }
}