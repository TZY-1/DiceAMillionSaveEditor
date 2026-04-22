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

        File.WriteAllText(filePath, content);
    }
}
