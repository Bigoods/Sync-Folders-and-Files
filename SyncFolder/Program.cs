string currDir = Directory.GetCurrentDirectory();
string? logFileDir = currDir + "/Log.txt";
const string lblFolderToSync = "Folder to Sync";
const string lblFolderSynced = "Folder Synced";
string? folderToSync = $"{currDir}/{lblFolderToSync}";
string? folderSynced = $"{currDir}/{lblFolderSynced}";
Console.WriteLine($"Current Directory {currDir}");
Console.WriteLine();
Console.WriteLine("Starting program to sync Folders");
Console.WriteLine($"From {folderToSync}");
Console.WriteLine($"To {folderSynced}");
Console.WriteLine();
int udpdateRateTime = 1000; // in ms
bool changes = false;

void InsertLog(string content, bool insertDate = true)
{
    using (StreamWriter writer = new StreamWriter(logFileDir, true))
    {
        if (insertDate)
            writer.WriteLine($"At {DateTime.UtcNow}");
        writer.WriteLine(content);
        writer.WriteLine();
        changes = insertDate;
    }
}
void CreateFileIfNotExists(bool exists, string syncedFilePath, string toSyncFilePath)
{
    if (!exists)
    {
        if (File.GetAttributes(toSyncFilePath) == FileAttributes.Directory)
        {
            Directory.CreateDirectory(syncedFilePath);
            InsertLog($"Create Directory {syncedFilePath.Replace(folderSynced, "")}");
        }
    }

    if ((File.GetAttributes(toSyncFilePath) == FileAttributes.Archive) &&
        (File.GetLastWriteTime(syncedFilePath) < File.GetLastWriteTime(toSyncFilePath)))
    {
        File.Copy(toSyncFilePath, syncedFilePath, true);
        InsertLog($"Syncing file {toSyncFilePath.Replace(folderToSync, "")}");
    }
}
void CreateFolderIfNotExists(bool exists, string filePath)
{
    if (!exists)
    {
        Directory.CreateDirectory(filePath);
    }
}

while (true)
{
    try
    {
        CreateFolderIfNotExists(Directory.Exists(folderToSync), folderToSync);

        foreach (string file in Directory.GetFileSystemEntries(folderToSync, "*", SearchOption.AllDirectories).OrderBy(x => x))
        {
            string fileWithoutFolder = file.Replace(folderToSync, "");
            CreateFileIfNotExists(Path.Exists(folderSynced + fileWithoutFolder), folderSynced + fileWithoutFolder, file);
        }

        var first = Directory.GetFileSystemEntries(folderToSync, "*", SearchOption.AllDirectories).Select(x => x.Replace(folderToSync, ""));
        var second = Directory.GetFileSystemEntries(folderSynced, "*", SearchOption.AllDirectories).Select(x => x.Replace(folderSynced, ""));
        IEnumerable<string> filesToRemove = second.Except(first);

        foreach (string file in filesToRemove.OrderByDescending(x => x.Length))
        {
            if (File.GetAttributes(folderSynced + file) == FileAttributes.Directory)
            {
                Directory.Delete(folderSynced + file);
                InsertLog($"Delete Folder {file}");
            }
            else
            {
                File.Delete(folderSynced + file);
                InsertLog($"Delete file {file}");
            }
        }
        if (changes)
            InsertLog("---------------------------------------------------------------------------------", false);
        Thread.Sleep(udpdateRateTime);
    }
    catch (Exception e)
    {
        InsertLog(e.ToString());
        Console.WriteLine($"Error: {e}");
    }
}