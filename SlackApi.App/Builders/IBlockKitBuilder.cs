using SlackApi.Domain.SlackDTOs;

namespace SlackApi.App.Builders
{
    public interface IBlockKitBuilder
    {

        ISlackRequest ConstructRequest();
    }
}
