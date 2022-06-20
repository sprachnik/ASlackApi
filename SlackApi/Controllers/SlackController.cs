using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SlackApi.App.Services;
using SlackApi.Domain.SlackDTOs;
using System.Text.Json;
using System.Web;

namespace SlackApi.Controllers
{

    public class SlackController : ControllerBase
    {
        private readonly ISlackInteractiveEventService _slackInteractiveEventService;
        private readonly ILogger<SlackController> _logger;

        public SlackController(ISlackInteractiveEventService slackInteractiveEventService,
            ILogger<SlackController> logger)
        {
            _slackInteractiveEventService = slackInteractiveEventService;
            _logger = logger;
        }

        [Authorize(Policy = "ValidateSlackRequest")]
        [HttpPost("interactive-events")]
        [Consumes("application/x-www-form-urlencoded")]
        [Produces("application/json")]
        public async Task<ActionResult<SlackResponse>> ProcessInteractiveEvent(
           [FromForm] string payload)
        {
            var decodedPayload = HttpUtility.UrlDecode(payload);
            var interactiveEvent = JsonSerializer.Deserialize<SlackInteractionPayload>(decodedPayload);
            return await _slackInteractiveEventService.ProcessInteractiveEvent(interactiveEvent);
        }

        [Authorize(Policy = "ValidateSlackRequest")]
        [HttpPost("options-events")]
        [Consumes("application/x-www-form-urlencoded")]
        public async Task<ActionResult<SlackResponse>> ProcessOptionsEvent(
           [FromForm] string payload)
        {
            var decodedPayload = HttpUtility.UrlDecode(payload);
            Console.WriteLine(decodedPayload);
            
            return new SlackResponse();
        }
    }
}
