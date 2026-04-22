namespace DiceAMillionSaveEditor.Logic.Interfaces;

public interface IBackupService
{
    string CreateBackup(string originalFilePath);
    void RestoreBackup(string backupFilePath, string targetFilePath);
}
