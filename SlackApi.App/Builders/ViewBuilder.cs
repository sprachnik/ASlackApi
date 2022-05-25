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

        public override ViewBuilder AddBlock(string type, string blockId, Text? text, Accessory? accessory = null)
        {
            base.AddBlock(type, blockId, text, accessory);
            return this;
        }

        public override ViewBuilder AddUsersSelectBlock(string label, string actionId)
        {
            base.AddUsersSelectBlock(label, actionId);
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
