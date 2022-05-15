using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SlackApi.App.Services;
using SlackApi.Domain.DTOs;
using System.Text.Json;
using System.Web;

namespace SlackApi.Controllers
{
    public class SlackController : ControllerBase
    {
        private readonly ISlackInteractiveEventService _slackInteractiveEventService;

        public SlackController(ISlackInteractiveEventService slackInteractiveEventService)
        {
            _slackInteractiveEventService = slackInteractiveEventService;
        }

        [Authorize(Policy = "ValidateSlackRequest")]
        [HttpPost("interactive-events")]
        [Consumes("application/x-www-form-urlencoded")]
        public async Task<ActionResult<SlackResponse>> ProcessInteractiveEvent(
           [FromForm] string payload)
        {
            var decodedPayload = HttpUtility.UrlDecode(payload);
            var interactiveEvent = JsonSerializer.Deserialize<SlackInteractiveEvent>(decodedPayload);
            var response = await _slackInteractiveEventService.ProcessInteractiveEvent(interactiveEvent);
            return response;
        }
    }
}
