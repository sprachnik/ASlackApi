using SlackApi.App.MemStore;
using SlackApi.App.Services;
using SlackApi.App.Transactions;
using SlackApi.Core.Settings;
using SlackApi.Repository.Cache;

namespace SlackApi.App_Start
{
    public static class DependenciesConfig
    {
        public static IServiceCollection RegisterDependencies(
            this IServiceCollection services,
            ApplicationSettings applicationSettings,
            ConnectionStrings connectionStrings)
        {
            // Transient
            services.AddTransient<ISlackInteractiveEventService, SlackInteractiveEventService>();
            services.AddTransient<ISlackAuthService, SlackAuthService>();
            services.AddTransient<IShortCutService, ShortcutService>();
            services.AddTransient<IBadgeViewModalService, BadgeViewModalService>();
            services.AddTransient<ICache, MemCache>();
            services.AddTransient<ITableStorageMemStore, TableStorageMemStore>();
            services.AddTransient<IBlockActionService, BlockActionsService>();
            services.AddTransient<ISlackUserService, SlackUserService>();
            services.AddTransient<IUserTransactionService, UserTransactionService>();
            services.AddTransient<IUserBalanceService, UserBalanceService>();
            services.AddTransient<IViewSubmissionService, ViewSubmissionService>();
            services.AddTransient<IUserNotificationService, UserNotificationService>();

            // Scopes

            // Singleton
            services.AddSingleton(applicationSettings);
            services.AddSingleton(connectionStrings);

            return services;
        }
    }
}
