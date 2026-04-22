using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DiceAMillionSaveEditor.UI.ViewModels;

public partial class SavePropertyItem : ObservableObject
{
    [ObservableProperty]
    private string _key = "";

    [ObservableProperty]
    private string _value = "";

    public IList<string> ValueOptions { get; set; } = Array.Empty<string>();

    public bool HasPredefinedValueOptions => ValueOptions.Count > 0;
}
