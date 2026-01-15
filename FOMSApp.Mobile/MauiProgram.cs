using Microsoft.Extensions.Logging;
using FOMSApp.Mobile.Services;
using FOMSApp.Mobile.ViewModels;
using FOMSApp.Mobile.Views;

namespace FOMSApp.Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        // Register services
        builder.Services.AddSingleton<IApiService, ApiService>();
        
        // Register ViewModels
        builder.Services.AddTransient<VaultsViewModel>();
        builder.Services.AddTransient<VaultDetailsViewModel>();
        builder.Services.AddTransient<MidpointsViewModel>();
        builder.Services.AddTransient<CablesViewModel>();
        
        // Register Views
        builder.Services.AddTransient<VaultsPage>();
        builder.Services.AddTransient<VaultDetailsPage>();
        builder.Services.AddTransient<MidpointsPage>();
        builder.Services.AddTransient<CablesPage>();

        return builder.Build();
    }
}
