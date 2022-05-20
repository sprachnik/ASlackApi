using SlackApi.Domain.DTOs;

namespace SlackApi.App.Builders
{
    public interface IBlockKitBuilder
    {
        ISlackRequest ConstructRequest();
    }
}
