using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using DiceAMillionSaveEditor.Logic.Interfaces;
using DiceAMillionSaveEditor.Logic.Services;
using DiceAMillionSaveEditor.UI.ViewModels;

namespace DiceAMillionSaveEditor.UI;

public partial class App : Application
{
    public static IHost? AppHost { get; private set; }

    public App()
    {
        AppHost = Host.CreateDefaultBuilder()
            .ConfigureServices((hostContext, services) =>
            {
                // Register Domain Services
                services.AddSingleton<IBase64Encoder, Base64EncoderService>();
                services.AddSingleton<ISaveGameProvider, SaveGameProvider>();
                services.AddSingleton<IBackupService, BackupService>();
                services.AddSingleton<ISteamAchievementService, SteamAchievementService>();
                services.AddSingleton<IAchievementMapper, AchievementMapper>();
                
                // Register Rules for JSON Updates
                services.AddSingleton<IAchievementRule, DiceAMillionSaveEditor.Logic.Rules.DiceRule>();
                services.AddSingleton<IAchievementRule, DiceAMillionSaveEditor.Logic.Rules.RingRule>();
                services.AddSingleton<IAchievementRule, DiceAMillionSaveEditor.Logic.Rules.CardRule>();
                services.AddSingleton<IAchievementRule, DiceAMillionSaveEditor.Logic.Rules.CharRule>();
                services.AddSingleton<IAchievementRule, DiceAMillionSaveEditor.Logic.Rules.BossRule>();
                services.AddSingleton<IAchievementRule, DiceAMillionSaveEditor.Logic.Rules.ChallengeRule>();
                services.AddSingleton<IAchievementRule, DiceAMillionSaveEditor.Logic.Rules.PhoneStateRule>();

                services.AddSingleton<IJsonModifier, JsonModifierService>();

                // Register ViewModels
                services.AddTransient<MainViewModel>();

                // Register Views
                services.AddTransient<MainWindow>();
            })
            .Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        await AppHost!.StartAsync();

        var mainWindow = AppHost.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();

        base.OnStartup(e);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        await AppHost!.StopAsync();
        AppHost.Dispose();

        base.OnExit(e);
    }
}
