﻿namespace SlackApi.App.HttpClients
{
    /// <summary>
    /// https://api.slack.com/methods
    /// </summary>
    public static class SlackEndpoints
    {
        /// <summary>
        /// https://api.slack.com/methods/views.open
        /// </summary>
        public const string ViewsOpenUrl = "https://slack.com/api/views.open";

        /// <summary>
        /// https://api.slack.com/methods/views.publish
        /// </summary>
        public const string ViewPublishUrl = "https://api.slack.com/methods/views.publish";

        /// <summary>
        /// https://api.slack.com/methods/views.update
        /// </summary>
        public const string ViewsUpdateUrl = "https://slack.com/api/views.update";

        /// <summary>
        /// https://api.slack.com/methods/views.push
        /// </summary>
        public const string ViewsPushUrl = "https://slack.com/api/views.push";
    }
}
