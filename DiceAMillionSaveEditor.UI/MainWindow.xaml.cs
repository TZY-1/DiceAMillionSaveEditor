using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using DiceAMillionSaveEditor.UI.ViewModels;

namespace DiceAMillionSaveEditor.UI;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;

        foreach (var entry in viewModel.LogEntries)
        {
            AppendLogEntry(entry);
        }

        viewModel.LogEntries.CollectionChanged += (_, args) =>
        {
            if (args.Action != NotifyCollectionChangedAction.Add || args.NewItems == null)
            {
                return;
            }

            foreach (var item in args.NewItems)
            {
                if (item is LogEntry entry)
                {
                    AppendLogEntry(entry);
                }
            }
        };
    }

    private void AppendLogEntry(LogEntry entry)
    {
        var paragraph = new Paragraph(new Run(entry.Message))
        {
            Margin = new Thickness(0),
            Foreground = GetBrushForLevel(entry.Level)
        };

        LogRichTextBox.Document.Blocks.Add(paragraph);
        LogRichTextBox.ScrollToEnd();
    }

    private static Brush GetBrushForLevel(LogLevel level)
    {
        return level switch
        {
            LogLevel.Success => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#22c55e")),
            LogLevel.Warning => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f59e0b")),
            LogLevel.Error => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ef4444")),
            _ => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#94a3b8"))
        };
    }
}
