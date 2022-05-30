using SlackApi.Core.Extensions;
using SlackApi.Domain.SlackDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlackApi.App.Services
{
    public class BlockActionsService : IBlockActionService
    {
        private readonly IBadgeViewModalService _badgeViewModalService;

        public BlockActionsService(IBadgeViewModalService badgeViewModalService)
        {
            _badgeViewModalService = badgeViewModalService;
        }

        public async Task<SlackResponse> ProcessBlockActions(SlackInteractionPayload? interactiveEvent)
        {
            if (interactiveEvent?.Actions == null)
                return new();

            foreach (var action in interactiveEvent.Actions)
            {
                _ = action switch
                {

                    { BlockId: BadgeViewModalService.BadgeSelectBlockId } => await _badgeViewModalService.SelectBadgeAction(interactiveEvent),
                    _ => throw new Exception("No action found!")
                };
            }

            return new();
        }
    }

    public interface IBlockActionService
    {
        Task<SlackResponse> ProcessBlockActions(SlackInteractionPayload? interactiveEvent);
    }
}
