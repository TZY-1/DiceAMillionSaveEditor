namespace DiceAMillionSaveEditor.Logic.Interfaces;

public interface ISaveGameProvider
{
    string ReadSaveFile(string filePath);
    void WriteSaveFile(string filePath, string content);
}
