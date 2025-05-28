using CommunityToolkit.Extensions.DependencyInjection;
using MicaForEveryone.App.Services;
using MicaForEveryone.App.ViewModels;
using MicaForEveryone.CoreUI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Dispatching;
using System;

#if !DEBUG
using Microsoft.Windows.ApplicationModel.Resources;
using System.Globalization;
using System.Net;
using System.Net.NetworkInformation;
#endif

namespace MicaForEveryone.App;

public partial class App
{
    private static IServiceProvider? _services;
    public static IServiceProvider Services
    {
        get
        {
            return _services ??= ConfigureServices();
        }
    }

    private static IServiceProvider ConfigureServices()
    {
        ServiceCollection collection = new();

        collection.AddSingleton<IDispatchingService>(new DispatchingService(DispatcherQueue.GetForCurrentThread()));
        collection.AddSingleton<ILocalizationService>(new LocalizationService());

#if !DEBUG
        string appInsightsString = new ResourceLoader(ResourceLoader.GetDefaultResourceFilePath(), "appsettings").GetString("AppInsightsConnectionString");
        if (!string.IsNullOrEmpty(appInsightsString))
        {
            string domainName = IPGlobalProperties.GetIPGlobalProperties().DomainName;
            string hostName = Dns.GetHostName();

            if (!hostName.EndsWith(domainName, StringComparison.OrdinalIgnoreCase))
            {
                hostName = $"{hostName}.{domainName}";
            }

            collection.AddSingleton<ILoggingService>(new AppInsightsLoggingService(appInsightsString, new AppInsightsLoggingService.AppInsightsTags() { RoleInstance = hostName }));
        }
#endif

        // Check if we are really running packaged.
        collection.AddSingleton<IVersionInfoService, PackagedVersionInfoService>();
        collection.AddSingleton<ISettingsService, PackagedSettingsService>();
        collection.AddSingleton<IStartupService, PackagedStartupService>();
        collection.AddSingleton<IUpdateCheckerService, PackagedUpdateCheckerService>();

        ConfigureServices(collection);

        return collection.BuildServiceProvider();
    }

    [Singleton(typeof(MainAppService))]
    [Singleton(typeof(RuleService), [typeof(IRuleService)])]
    [Singleton(typeof(ThemingService), [typeof(IThemingService)])]
    [Transient(typeof(SettingsViewModel))]
    [Transient(typeof(TrayIconViewModel))]
    [Transient(typeof(AddClassRuleContentDialogViewModel))]
    [Transient(typeof(AddProcessRuleContentDialogViewModel))]
    [Transient(typeof(AppSettingsPageViewModel))]
    private static partial void ConfigureServices(IServiceCollection services);
}
