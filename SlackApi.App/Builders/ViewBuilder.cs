using SlackApi.Domain.SlackDTOs;

namespace SlackApi.App.Builders
{
    public class ViewBuilder : BlockKitBuilder, IBlockKitBuilder
    {
        private readonly SlackViewPayload _view;
        private readonly SlackViewRequest _request;

        public ViewBuilder(string type, 
            string? incomingCallbackId = null, 
            string? triggerId = null)
        {
            _view = new SlackViewPayload
            {
                Type = type,
                CallbackId = incomingCallbackId
            };
            _request = new SlackViewRequest 
            { 
                TriggerId = triggerId,
                View = _view
            };
        }

        public ViewBuilder AddTitle(string text, string type = "plain_text")
        {
            if (_view.Title != null)
                return this;

            _view.Title = new Title
            {
                Text = text,
                Type = type
            };

            return this;
        }

        public override ViewBuilder AddInputBlock(string blockId,
            Element? element = null,
            Label? label = null)
        {
            base.AddInputBlock(blockId, element, label);
            return this;
        }

        public override ViewBuilder AddAccessoryBlock(string type, 
            string blockId, 
            Text? text = null, 
            Accessory? accessory = null)
        {
            base.AddAccessoryBlock(type, blockId, text, accessory);
            return this;
        }

        public override ViewBuilder AddImageBlock(string blockId,
            string imageUrl,
            string altText,
            Title? title = null)
        {
            base.AddImageBlock(blockId, imageUrl, altText, title);
            return this;
        }

        public override ViewBuilder AddUsersSelectBlock(string label, string actionId, string blockId)
        {
            base.AddUsersSelectBlock(label, actionId, blockId);
            return this;
        }

        public ViewBuilder AddSubmit(string textType, string text)
        {
            _view.Submit = new Submit
            {
                Type = textType,
                Text = text
            };
            return this;
        }

        public new ISlackRequest ConstructRequest()
        {
            _view.Blocks = _blocks;
            return _request;
        }
    }
}
