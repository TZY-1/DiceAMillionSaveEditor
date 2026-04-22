using CommunityToolkit.Mvvm.ComponentModel;

namespace DiceAMillionSaveEditor.UI.ViewModels;

public partial class FilterRuleItem : ObservableObject
{
    [ObservableProperty]
    private string _field = "Key";

    [ObservableProperty]
    private string _operator = "=";

    [ObservableProperty]
    private string _pattern = "";
}
