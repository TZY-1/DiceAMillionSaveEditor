using System;
using System.IO;
using DiceAMillionSaveEditor.Logic.Interfaces;

namespace DiceAMillionSaveEditor.Logic.Services;

public class BackupService : IBackupService
{
    public string CreateBackup(string originalFilePath)
    {
        if (!File.Exists(originalFilePath))
            throw new FileNotFoundException($"Original save file not found: {originalFilePath}");

        var directory = Path.GetDirectoryName(originalFilePath) ?? string.Empty;
        var extension = Path.GetExtension(originalFilePath);
        var filenameWithoutExt = Path.GetFileNameWithoutExtension(originalFilePath);
        var backupDirectory = Path.Combine(directory, "backup");
        Directory.CreateDirectory(backupDirectory);

        var backupFilename = $"{filenameWithoutExt}_backup_{DateTime.Now:yyyyMMdd_HHmmss}{extension}";
        var backupFilePath = Path.Combine(backupDirectory, backupFilename);
        
        File.Copy(originalFilePath, backupFilePath, true);
        return backupFilePath;
    }

    public void RestoreBackup(string backupFilePath, string targetFilePath)
    {
        if (!File.Exists(backupFilePath))
            throw new FileNotFoundException("Backup file not found.");
            
        File.Copy(backupFilePath, targetFilePath, true);
    }
}
