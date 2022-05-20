using SlackApi.Domain.DTOs;

namespace SlackApi.App.Builders
{
    public abstract class BlockKitBuilder : IBlockKitBuilder
    {
        internal readonly List<Block> _blocks = new();

        public BlockKitBuilder()
        {

        }

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

        public virtual ISlackRequest ConstructRequest()
        {
            throw new NotImplementedException();
        }
    }
}
