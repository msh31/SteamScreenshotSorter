using SteamImageSorter;
using SteamWebAPI2.Interfaces;
using SteamWebAPI2.Utilities;
using System.Diagnostics;

string configFile = "config.json";
var configManager = new JsonConfigManager(configFile);
AppSettings appSettings = configManager.ReadConfig();
var imageExtensions = new[] { ".jpg", ".jpeg", ".png" };

#region checks
//make sure user has entered a value into the api key and directory in the json file
while (string.IsNullOrWhiteSpace(appSettings.ApiKey) || string.IsNullOrEmpty(appSettings.Directory))
{
    if (string.IsNullOrEmpty(appSettings.ApiKey))
    {
        Console.Write("API key is missing or invalid. Please enter your API key: ");
        appSettings.ApiKey = Console.ReadLine() ?? string.Empty;
    }

    if (string.IsNullOrEmpty(appSettings.Directory))
    {
        Console.Write("No directory found! Enter your Steam screenshots folder: ");
        appSettings.Directory = Console.ReadLine() ?? string.Empty;
    }

    configManager.WriteConfig(appSettings);
}
#endregion

var steamInterfaceFactory = new SteamWebInterfaceFactory(appSettings.ApiKey);
var steamApps = steamInterfaceFactory.CreateSteamWebInterface<SteamApps>(new HttpClient()); 
var appListResponse = await steamApps.GetAppListAsync();
var uniqueAppList = appListResponse.Data.DistinctBy(app => app.AppId).ToList();
var appCache = uniqueAppList.ToDictionary(app => app.AppId, app => app.Name);

#region funcs
async Task<string?> GetGameNameFromSteamApiTask(int appId)
{
    try
    {
        Console.WriteLine($"Requesting game details for App ID {appId}...");
        
        // Check if the app ID exists in the cache
        if (appCache.TryGetValue((uint)appId, out string? appName)) 
        {
            Console.WriteLine($"Successfully retrieved game name for App ID {appId}: {appName}");
            return appName;
        }
        else
        {
            Console.WriteLine($"No valid response received for App ID {appId}");
            return null;
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error fetching game details for App ID {appId}: {ex.Message}");
        return null;
    }
}

void MoveFileToGameFolder(string filePath, string gameName)
{
    string validGameName = gameName.Replace(":", " - ");

    //Remove "on Steam", registered trademark, and clean up trailing punctuation/whitespace
    validGameName = validGameName.Replace("on Steam", "")
        .Replace("®", "")
        .TrimEnd('-', ' ', '™');

    string directory = Path.Combine(appSettings.Directory, validGameName);
    string newFilePath = Path.Combine(directory, Path.GetFileName(filePath));

    if (!Directory.Exists(directory)) { Directory.CreateDirectory(directory); }

    File.Move(filePath, newFilePath, true);
}
#endregion

#region The process
Stopwatch stopwatch = new Stopwatch();
stopwatch.Start();

int totalScreenshots = 0;
int unknownAppIdScreenshots = 0;

var imageFiles = Directory.EnumerateFiles(appSettings.Directory, "*.*", SearchOption.AllDirectories)
    .Where(file => imageExtensions.Contains(Path.GetExtension(file).ToLowerInvariant()));

foreach (var file in imageFiles)
{
    totalScreenshots++; 
    string fileName = Path.GetFileNameWithoutExtension(file);
    string[] parts = fileName.Split('_');
    if (parts.Length > 0 && int.TryParse(parts[0], out int appId))
    {
        Console.WriteLine($"Processing file: {file} with App ID: {appId}");
        string? gameName = await GetGameNameFromSteamApiTask(appId);

        if (gameName != null) { MoveFileToGameFolder(file, gameName); }
        else
        {
            unknownAppIdScreenshots++; 
            Console.WriteLine($"Skipping file: {file} - No matching game found for App ID {appId}");
        }
    }
    else
    {
        unknownAppIdScreenshots++; 
        Console.WriteLine($"Skipping file: {file} - Could not parse App ID.");
    }
}
#endregion

stopwatch.Stop();
TimeSpan elapsed = stopwatch.Elapsed;

Console.WriteLine("--------------------------------------------------");
Console.WriteLine($"Sorting completed in {elapsed.TotalSeconds:F2} seconds.");
Console.WriteLine($"Total screenshots found: {totalScreenshots}");
Console.WriteLine($"Screenshots moved to unknown game folder: {unknownAppIdScreenshots}");
Console.WriteLine("--------------------------------------------------");
await Task.Delay(100);
// Environment.Exit(0);