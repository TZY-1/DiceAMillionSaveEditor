using CommunityToolkit.Mvvm.ComponentModel;

namespace DiceAMillionSaveEditor.UI.ViewModels;

public partial class LogEntry : ObservableObject
{
    [ObservableProperty]
    private string _message = "";

    [ObservableProperty]
    private LogLevel _level = LogLevel.Info;
}
