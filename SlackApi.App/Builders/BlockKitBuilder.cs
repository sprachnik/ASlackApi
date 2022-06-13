using SlackApi.Domain.Constants;
using SlackApi.Domain.SlackDTOs;

namespace SlackApi.App.Builders
{
    public abstract class BlockKitBuilder : IBlockKitBuilder
    {
        internal readonly List<Block> _blocks = new();

        public virtual IBlockKitBuilder AddInputBlock(string blockId,
            Element? element = null,
            Label? label = null)
        {
            if (element == null || blockId == null)
                throw new ArgumentNullException(nameof(element));

            _blocks.Add(new Block
            {
                BlockId = blockId,
                Type = BlockType.Input,
                Element = element,
                Label = label
            });

            return this;
        }

        public virtual IBlockKitBuilder AddAccessoryBlock(string type, 
            string blockId, 
            Text? text, 
            Accessory? accessory = null)
        {
            if (type == null || blockId == null)
                throw new ArgumentNullException(nameof(type));

            _blocks.Add(new Block
            {
                Type = type,
                BlockId = blockId,
                Text = text,
                Accessory = accessory
            });

            return this;
        }

        public virtual IBlockKitBuilder AddImageBlock(string blockId,
            string imageUrl,
            string altText,
            Title? title = null)
        {
            if (imageUrl == null || altText == null)
                throw new ArgumentNullException(nameof(imageUrl));

            _blocks.Add(new Block
            {
                Type = BlockType.Image,
                BlockId = blockId,
                ImageUrl = imageUrl,
                ImageAltText = altText,
                Title = title
            });

            return this;
        }

        public virtual IBlockKitBuilder AddUsersSelectBlock(string label = "Select a user", 
            string actionId = "users-select-action",
            string blockId = "users-select-block")
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
