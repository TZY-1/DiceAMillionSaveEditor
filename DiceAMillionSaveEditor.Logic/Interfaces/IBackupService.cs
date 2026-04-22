namespace DiceAMillionSaveEditor.Logic.Interfaces;

public interface IBackupService
{
    void CreateBackup(string originalFilePath);
    void RestoreBackup(string backupFilePath, string targetFilePath);
}
