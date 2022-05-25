using SlackApi.Domain.Constants;
using SlackApi.Domain.SlackDTOs;

namespace SlackApi.App.Builders
{
    public abstract class BlockKitBuilder : IBlockKitBuilder
    {
        internal readonly List<Block> _blocks = new();

        public virtual IBlockKitBuilder AddBlock(string type, string blockId, Text? text, Accessory? accessory = null)
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

        public virtual IBlockKitBuilder AddUsersSelectBlock(string label = "Select a user", string actionId = "users-select-action")
        {
            _blocks.Add(new Block
            {
                BlockId = Guid.NewGuid().ToString(),
                Type = BlockType.Section,
                Text = new Text
                {
                    BlockText = label,
                    Type = TextType.Markdown
                },
                Accessory = new Accessory
                {
                    Type = AccessoryType.UsersSelect,
                    Placeholder = new Placeholder
                    {
                        Type = TextType.PlainText,
                        Text = "Select a user",
                        Emoji = true
                    },
                    ActionId = actionId
                }
            });

            return this;
        }

        public virtual ISlackRequest ConstructRequest()
        {
            throw new NotImplementedException();
        }
    }
}
