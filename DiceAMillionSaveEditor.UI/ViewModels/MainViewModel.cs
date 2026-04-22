using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;
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
    private readonly SavePropertyFilterEngine _propertyFilterEngine;
    private readonly EditablePropertyMapper _propertyMapper;
    private readonly JsonFormatter _jsonFormatter;

    [ObservableProperty]
    private string _steamProfileUrl = "";

    [ObservableProperty]
    private string _saveFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        @"dice_a_million\default\data_1.sav");

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
    public ObservableCollection<FilterRuleItem> FilterRules { get; } = new();
    public ICollectionView FilteredProperties { get; }
    public ObservableCollection<LogEntry> LogEntries { get; } = new();
    public ObservableCollection<SteamAchievement> FetchedAchievements { get; } = new();
    public ICollectionView FilteredAchievements { get; }
    public IReadOnlyList<string> FilterFields { get; } = new[] { "Key", "Value" };
    public IReadOnlyList<string> FilterOperators { get; } = new[] { "=", "!=" };

    public MainViewModel(
        ISaveGameProvider saveProvider,
        IBase64Encoder encoder,
        IBackupService backupService,
        ISteamAchievementService steamService,
        IJsonModifier jsonModifier)
        : this(
            saveProvider,
            encoder,
            backupService,
            steamService,
            jsonModifier,
            new SavePropertyFilterEngine(),
            new EditablePropertyMapper(),
            new JsonFormatter())
    {
    }

    internal MainViewModel(
        ISaveGameProvider saveProvider,
        IBase64Encoder encoder,
        IBackupService backupService,
        ISteamAchievementService steamService,
        IJsonModifier jsonModifier,
        SavePropertyFilterEngine propertyFilterEngine,
        EditablePropertyMapper propertyMapper,
        JsonFormatter jsonFormatter)
    {
        _saveProvider = saveProvider;
        _encoder = encoder;
        _backupService = backupService;
        _steamService = steamService;
        _jsonModifier = jsonModifier;
        _propertyFilterEngine = propertyFilterEngine;
        _propertyMapper = propertyMapper;
        _jsonFormatter = jsonFormatter;

        FilteredProperties = CollectionViewSource.GetDefaultView(EditableProperties);
        FilteredProperties.Filter = FilterPropertyItem;
        FilteredAchievements = CollectionViewSource.GetDefaultView(FetchedAchievements);
        FilteredAchievements.Filter = FilterAchievementItem;

        EditableProperties.CollectionChanged += OnEditablePropertiesCollectionChanged;
        FilterRules.CollectionChanged += OnFilterRulesCollectionChanged;
        AddFilterRule();

        Log("Ready.", LogLevel.Info);
    }

    partial void OnSearchFilterChanged(string value)
    {
        RefreshFilteredPropertiesSafe();
    }

    partial void OnAchievementSearchFilterChanged(string value)
    {
        FilteredAchievements.Refresh();
    }

    private bool FilterPropertyItem(object obj)
    {
        if (obj is not SavePropertyItem item)
        {
            return false;
        }

        return _propertyFilterEngine.Matches(item, SearchFilter, FilterRules);
    }

    private bool FilterAchievementItem(object obj)
    {
        if (string.IsNullOrWhiteSpace(AchievementSearchFilter))
        {
            return true;
        }

        if (obj is not SteamAchievement item)
        {
            return false;
        }

        return item.Name.Contains(AchievementSearchFilter, StringComparison.OrdinalIgnoreCase)
               || item.ApiName.Contains(AchievementSearchFilter, StringComparison.OrdinalIgnoreCase)
               || item.Description.Contains(AchievementSearchFilter, StringComparison.OrdinalIgnoreCase);
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
            LoadedDecodedJson = _jsonFormatter.PrettyPrint(json);

            var dictionary = _jsonModifier.DecodeJsonToDictionary(json);
            ReplaceEditableProperties(dictionary);

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
                return;
            }

            Log("Save directory could not be found.", LogLevel.Warning);
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
            foreach (var achievement in achievementsList)
            {
                FetchedAchievements.Add(achievement);
            }

            var unlockedCount = achievementsList.Count(a => a.Unlocked);
            Log($"{unlockedCount} unlocked achievements found.", LogLevel.Success);

            var currentDict = _propertyMapper.BuildTypedDictionary(EditableProperties);
            var currentJson = _jsonModifier.EncodeDictionaryToJson(currentDict);

            var newJson = _jsonModifier.ApplyAchievementsToJson(currentJson, achievementsList, out var logs);
            foreach (var line in logs)
            {
                LogAchievementMergeLine(line);
            }

            var newDictionary = _jsonModifier.DecodeJsonToDictionary(newJson);
            ReplaceEditableProperties(newDictionary);
            ModifiedJson = _jsonFormatter.PrettyPrint(newJson);

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
            var currentDict = _propertyMapper.BuildTypedDictionary(EditableProperties);
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

    [RelayCommand]
    private void AddFilterRule()
    {
        FilterRules.Add(new FilterRuleItem());
    }

    [RelayCommand]
    private void RemoveFilterRule(FilterRuleItem? rule)
    {
        if (rule is null)
        {
            return;
        }

        FilterRules.Remove(rule);
        if (FilterRules.Count == 0)
        {
            AddFilterRule();
        }
    }

    private void ReplaceEditableProperties(Dictionary<string, object> dictionary)
    {
        _suspendJsonPreviewUpdates = true;
        EditableProperties.Clear();

        foreach (var item in _propertyMapper.CreateEditableItems(dictionary))
        {
            EditableProperties.Add(item);
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
        if (e.PropertyName is not nameof(SavePropertyItem.Key) and not nameof(SavePropertyItem.Value))
        {
            return;
        }

        RefreshFilteredPropertiesSafe();
        RefreshModifiedJsonPreview();
    }

    private void OnFilterRulesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems != null)
        {
            foreach (var oldItem in e.OldItems.OfType<FilterRuleItem>())
            {
                oldItem.PropertyChanged -= OnFilterRuleChanged;
            }
        }

        if (e.NewItems != null)
        {
            foreach (var newItem in e.NewItems.OfType<FilterRuleItem>())
            {
                newItem.PropertyChanged += OnFilterRuleChanged;
            }
        }

        RefreshFilteredPropertiesSafe();
    }

    private void OnFilterRuleChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(FilterRuleItem.Field) or nameof(FilterRuleItem.Operator) or nameof(FilterRuleItem.Pattern))
        {
            RefreshFilteredPropertiesSafe();
        }
    }

    private bool CanRefreshFilteredPropertiesNow()
    {
        if (FilteredProperties is not IEditableCollectionView editableView)
        {
            return true;
        }

        return !editableView.IsAddingNew && !editableView.IsEditingItem;
    }

    private void RefreshFilteredPropertiesSafe()
    {
        if (CanRefreshFilteredPropertiesNow())
        {
            FilteredProperties.Refresh();
            return;
        }

        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher is null)
        {
            return;
        }

        dispatcher.BeginInvoke(new Action(() =>
        {
            if (CanRefreshFilteredPropertiesNow())
            {
                FilteredProperties.Refresh();
            }
        }), DispatcherPriority.Background);
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
            var currentDict = _propertyMapper.BuildTypedDictionary(EditableProperties);
            var compactJson = _jsonModifier.EncodeDictionaryToJson(currentDict);
            ModifiedJson = _jsonFormatter.PrettyPrint(compactJson);
        }
        catch (Exception ex)
        {
            ModifiedJson = $"JSON preview unavailable: {ex.Message}";
        }
    }

    private void LogAchievementMergeLine(string line)
    {
        if (line.StartsWith("[SUCCESS]"))
        {
            Log(line, LogLevel.Success);
            return;
        }

        if (line.StartsWith("[WARNING]"))
        {
            Log(line, LogLevel.Warning);
            return;
        }

        if (line.StartsWith("[ERROR]"))
        {
            Log(line, LogLevel.Error);
            return;
        }

        Log(line, LogLevel.Info);
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
}
