using System;
using System.IO;
using DiceAMillionSaveEditor.Logic.Interfaces;

namespace DiceAMillionSaveEditor.Logic.Services;

public class SaveGameProvider : ISaveGameProvider
{
    public string ReadSaveFile(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Save file not found at {filePath}");

        return File.ReadAllText(filePath);
    }

    public void WriteSaveFile(string filePath, string content)
    {
        var dir = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        var tempPath = $"{filePath}.tmp";
        File.WriteAllText(tempPath, content);

        if (File.Exists(filePath))
        {
            File.Replace(tempPath, filePath, null, true);
        }
        else
        {
            File.Move(tempPath, filePath);
        }

        File.SetLastWriteTimeUtc(filePath, DateTime.UtcNow);
    }
}
