using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DiceAMillionSaveEditor.Logic.Interfaces;

namespace DiceAMillionSaveEditor.UI.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly ISaveGameProvider _saveProvider;
    private readonly IBase64Encoder _encoder;
    private readonly IBackupService _backupService;
    private readonly ISteamAchievementService _steamService;
    private readonly IJsonModifier _jsonModifier;

    [ObservableProperty]
    private string _steamProfileUrl = "";

    [ObservableProperty]
    private string _saveFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"dice_a_million\default\data_1.sav");

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _searchFilter = "";

    [ObservableProperty]
    private string _achievementSearchFilter = "";

    [ObservableProperty]
    private string _logText = "";

    [ObservableProperty]
    private string _loadedDecodedJson = "No save loaded yet.";

    [ObservableProperty]
    private string _modifiedJson = "No save loaded yet.";

    private bool _suspendJsonPreviewUpdates;

    public ObservableCollection<SavePropertyItem> EditableProperties { get; } = new();
    public ICollectionView FilteredProperties { get; }
    public ObservableCollection<LogEntry> LogEntries { get; } = new();
    public ObservableCollection<SteamAchievement> FetchedAchievements { get; } = new();
    public ICollectionView FilteredAchievements { get; }

    public MainViewModel(
        ISaveGameProvider saveProvider,
        IBase64Encoder encoder,
        IBackupService backupService,
        ISteamAchievementService steamService,
        IJsonModifier jsonModifier)
    {
        _saveProvider = saveProvider;
        _encoder = encoder;
        _backupService = backupService;
        _steamService = steamService;
        _jsonModifier = jsonModifier;

        FilteredProperties = CollectionViewSource.GetDefaultView(EditableProperties);
        FilteredProperties.Filter = FilterPropertyItem;
        FilteredAchievements = CollectionViewSource.GetDefaultView(FetchedAchievements);
        FilteredAchievements.Filter = FilterAchievementItem;
        EditableProperties.CollectionChanged += OnEditablePropertiesCollectionChanged;

        Log("Ready.", LogLevel.Info);
    }

    partial void OnSearchFilterChanged(string value)
    {
        FilteredProperties.Refresh();
    }

    partial void OnAchievementSearchFilterChanged(string value)
    {
        FilteredAchievements.Refresh();
    }

    private bool FilterPropertyItem(object obj)
    {
        if (string.IsNullOrWhiteSpace(SearchFilter)) return true;
        if (obj is SavePropertyItem item)
        {
            return item.Key.Contains(SearchFilter, StringComparison.OrdinalIgnoreCase)
                || item.Value.Contains(SearchFilter, StringComparison.OrdinalIgnoreCase);
        }
        return false;
    }

    private bool FilterAchievementItem(object obj)
    {
        if (string.IsNullOrWhiteSpace(AchievementSearchFilter)) return true;
        if (obj is SteamAchievement item)
        {
            return item.Name.Contains(AchievementSearchFilter, StringComparison.OrdinalIgnoreCase)
                || item.ApiName.Contains(AchievementSearchFilter, StringComparison.OrdinalIgnoreCase)
                || item.Description.Contains(AchievementSearchFilter, StringComparison.OrdinalIgnoreCase);
        }
        return false;
    }

    [RelayCommand]
    private async Task LoadSaveAsync()
    {
        try
        {
            IsBusy = true;
            Log("Loading save file...");
            
            var base64 = _saveProvider.ReadSaveFile(SaveFilePath);
            var json = _encoder.Decode(base64);
            LoadedDecodedJson = PrettyPrintJson(json);
            
            var dict = _jsonModifier.DecodeJsonToDictionary(json);
            ReplaceEditableProperties(dict);
            
            Log($"Save file loaded. {EditableProperties.Count} properties found.", LogLevel.Success);
        }
        catch (Exception ex)
        {
            Log($"Error loading save: {ex.Message}", LogLevel.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void OpenSaveLocation()
    {
        try
        {
            var dir = Path.GetDirectoryName(SaveFilePath);
            if (!string.IsNullOrEmpty(dir) && Directory.Exists(dir))
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = dir,
                    UseShellExecute = true,
                    Verb = "open"
                });
            }
            else
            {
                Log("Save directory could not be found.", LogLevel.Warning);
            }
        }
        catch (Exception ex)
        {
            Log($"Error opening location: {ex.Message}", LogLevel.Error);
        }
    }

    [RelayCommand]
    private async Task FetchAndApplyAchievementsAsync()
    {
        if (EditableProperties.Count == 0)
        {
            Log("Please load the save file first!", LogLevel.Warning);
            return;
        }

        try
        {
            IsBusy = true;
            Log("Fetching achievements from Steam...");
            var achievements = await _steamService.FetchAchievementsAsync(SteamProfileUrl);
            var achievementsList = achievements.ToList();
            
            FetchedAchievements.Clear();
            foreach (var ach in achievementsList)
            {
                FetchedAchievements.Add(ach);
            }
            
            var count = achievementsList.Count(a => a.Unlocked);
            Log($"{count} unlocked achievements found.", LogLevel.Success);

            var currentDict = BuildTypedDictionary();
            var currentJson = _jsonModifier.EncodeDictionaryToJson(currentDict);

            var newJson = _jsonModifier.ApplyAchievementsToJson(currentJson, achievementsList, out var logs);
            foreach (var l in logs)
            {
                if (l.StartsWith("[SUCCESS]"))
                    Log(l, LogLevel.Success);
                else if (l.StartsWith("[WARNING]"))
                    Log(l, LogLevel.Warning);
                else if (l.StartsWith("[ERROR]"))
                    Log(l, LogLevel.Error);
                else
                    Log(l, LogLevel.Info);
            }

            // Reload into grid
            var newDict = _jsonModifier.DecodeJsonToDictionary(newJson);
            ReplaceEditableProperties(newDict);
            ModifiedJson = PrettyPrintJson(newJson);

            Log("Achievements merged successfully! Review changes and click Save.", LogLevel.Success);
        }
        catch (Exception ex)
        {
            Log($"Error processing Steam achievements: {ex.Message}", LogLevel.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SaveAndEncodeAsync()
    {
        try
        {
            IsBusy = true;
            Log("Creating backup...");
            var backupPath = _backupService.CreateBackup(SaveFilePath);
            Log($"Backup created: {backupPath}", LogLevel.Success);

            Log("Encoding save file...");
            var currentDict = BuildTypedDictionary();
            var currentJson = _jsonModifier.EncodeDictionaryToJson(currentDict);
            var base64 = _encoder.Encode(currentJson);
            
            _saveProvider.WriteSaveFile(SaveFilePath, base64);
            var updatedAt = File.GetLastWriteTime(SaveFilePath);
            Log($"Save file written successfully: {SaveFilePath}", LogLevel.Success);
            Log($"Updated at: {updatedAt:yyyy-MM-dd HH:mm:ss}", LogLevel.Info);
        }
        catch (Exception ex)
        {
            Log($"Error saving: {ex.Message}", LogLevel.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void Log(string msg, LogLevel level = LogLevel.Info)
    {
        var line = $"[{DateTime.Now:HH:mm:ss}] {msg}";
        LogEntries.Add(new LogEntry
        {
            Message = line,
            Level = level
        });

        LogText = string.IsNullOrEmpty(LogText)
            ? line
            : $"{LogText}{Environment.NewLine}{line}";
    }

    private Dictionary<string, object> BuildTypedDictionary()
    {
        return EditableProperties.ToDictionary(
            x => x.Key,
            x => ParseEditableValue(x.Value));
    }

    private static object ParseEditableValue(string value)
    {
        var trimmed = value.Trim();

        if (bool.TryParse(trimmed, out var boolValue))
        {
            return boolValue;
        }

        if (double.TryParse(trimmed, NumberStyles.Float, CultureInfo.InvariantCulture, out var numericValue))
        {
            return numericValue;
        }

        if (double.TryParse(trimmed, NumberStyles.Float, CultureInfo.CurrentCulture, out numericValue))
        {
            return numericValue;
        }

        return value;
    }

    private void ReplaceEditableProperties(Dictionary<string, object> dictionary)
    {
        _suspendJsonPreviewUpdates = true;
        EditableProperties.Clear();

        foreach (var kvp in dictionary.OrderBy(x => x.Key))
        {
            EditableProperties.Add(new SavePropertyItem
            {
                Key = kvp.Key,
                Value = kvp.Value.ToString() ?? ""
            });
        }

        _suspendJsonPreviewUpdates = false;
        RefreshModifiedJsonPreview();
    }

    private void OnEditablePropertiesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems != null)
        {
            foreach (var oldItem in e.OldItems.OfType<SavePropertyItem>())
            {
                oldItem.PropertyChanged -= OnEditablePropertyChanged;
            }
        }

        if (e.NewItems != null)
        {
            foreach (var newItem in e.NewItems.OfType<SavePropertyItem>())
            {
                newItem.PropertyChanged += OnEditablePropertyChanged;
            }
        }

        RefreshModifiedJsonPreview();
    }

    private void OnEditablePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(SavePropertyItem.Key) or nameof(SavePropertyItem.Value))
        {
            RefreshModifiedJsonPreview();
        }
    }

    private void RefreshModifiedJsonPreview()
    {
        if (_suspendJsonPreviewUpdates)
        {
            return;
        }

        if (EditableProperties.Count == 0)
        {
            ModifiedJson = "No save loaded yet.";
            return;
        }

        try
        {
            var currentDict = BuildTypedDictionary();
            var compactJson = _jsonModifier.EncodeDictionaryToJson(currentDict);
            ModifiedJson = PrettyPrintJson(compactJson);
        }
        catch (Exception ex)
        {
            ModifiedJson = $"JSON preview unavailable: {ex.Message}";
        }
    }

    private static string PrettyPrintJson(string json)
    {
        var jsonElement = JsonSerializer.Deserialize<JsonElement>(json);
        return JsonSerializer.Serialize(jsonElement, new JsonSerializerOptions
        {
            WriteIndented = true
        });
    }
}
