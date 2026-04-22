using CommunityToolkit.Mvvm.ComponentModel;

namespace DiceAMillionSaveEditor.UI.ViewModels;

public partial class SavePropertyItem : ObservableObject
{
    [ObservableProperty]
    private string _key = "";

    [ObservableProperty]
    private string _value = "";
}
