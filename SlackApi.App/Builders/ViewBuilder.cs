using SlackApi.Domain.DTOs;

namespace SlackApi.App.Builders
{
    public class ViewBuilder : BlockKitBuilder
    {
        private readonly SlackViewPayload View;
        private readonly SlackViewRequest Request;

        public ViewBuilder(string type, string? callbackId = null, string? triggerId = null)
        {
            View = new SlackViewPayload
            {
                Type = type,
                CallbackId = callbackId
            };
            Request = new SlackViewRequest 
            { 
                TriggerId = triggerId,
                View = View
            };
        }

        public ViewBuilder AddTitle(string text, string type = "plain_text")
        {
            if (View.Title != null)
                return this;

            View.Title = new Title
            {
                Text = text,
                Type = type
            };

            return this;
        }

        public SlackViewRequest ConstructRequest()
        {
            View.Blocks = _blocks;
            return Request;
        }
    }

    public class BlockKitBuilder
    {
        internal readonly List<Block> _blocks = new List<Block>();

        public BlockKitBuilder()
        {

        }

        public BlockKitBuilder AddBlock(string type, string blockId, Text? text, Accessory? accessory = null)
        {
            _blocks.Add(new Block
            {
                Type = type,
                BlockId = blockId,
                Text = text,
                Accessory = accessory
            });

            return this;
        }
    }
}
