using System.Management;
using Authaas_Docker.Models;

namespace Authaas_Docker;

public partial class Form1 : Form
{
    private readonly QueueProcessor<DownloadableItem> _test = new();

    public Form1()
    {
        InitializeComponent();
    }

    /// <summary>
    ///     Returns a string with the current UTC date and time.
    /// </summary>
    /// <returns>
    ///     A string with the current UTC date and time in the format "[date] ".
    /// </returns>
    public string DateForLog()
    {
        return $"[{DateTime.UtcNow}] ";
    }


    /// <summary>
    ///     Logs the computer information such as Operating system version, Operating system architecture, Computer model,
    ///     Processor information, Disk space, and Amount of RAM.
    /// </summary>
    public void LogComputerInfo()
    {
        // Operating system version
        var osVersion = Environment.OSVersion.VersionString;
        listBoxLogs.Items.Add($"Operating system version: {osVersion}");

        // Operating system architecture
        var osArchitecture = Environment.Is64BitOperatingSystem ? "64-bit" : "32-bit";
        listBoxLogs.Items.Add($"Operating system architecture: {osArchitecture}");

        // Computer model
        var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem");

        // Processor information
        //searcher.Query.QueryString = "SELECT * FROM Win32_Processor";
        //foreach (var item in searcher.Get())
        //{
        //    var name = item["Name"];
        //    var cores = item["NumberOfCores"];
        //    listBoxLogs.Items.Add($"Processor: {name}, Cores: {cores}");
        //}

        // Disk space
        searcher.Query.QueryString = "SELECT * FROM Win32_LogicalDisk WHERE DriveType=3";
        foreach (var item in searcher.Get())
        {
            var deviceId = item["DeviceID"];
            var size = Math.Round((ulong)item["Size"] / (1024.0 * 1024 * 1024), 2);
            var freeSpace = Math.Round((ulong)item["FreeSpace"] / (1024.0 * 1024 * 1024), 2);
            listBoxLogs.Items.Add($"Drive {deviceId}: {freeSpace} GB free of {size} GB");
        }

        // Amount of RAM
        searcher.Query.QueryString = "SELECT TotalVisibleMemorySize,FreePhysicalMemory FROM Win32_OperatingSystem";
        foreach (var item in searcher.Get())
        {
            var totalVisibleMemorySize = (ulong)item["TotalVisibleMemorySize"];
            var freePhysicalMemory = (ulong)item["FreePhysicalMemory"];
            var usedMemory = totalVisibleMemorySize - freePhysicalMemory;

            listBoxLogs.Items.Add($"Total RAM: {totalVisibleMemorySize / 1024} MB");
            listBoxLogs.Items.Add($"Used RAM: {usedMemory / 1024} MB");
        }

        listBoxLogs.Items.Add("-------------------------");
    }

    public async void TestMethod()
    {
        var item = new DownloadableItem
        {
            Name = "Notepad++",
            Description = "The best Editor",
            Url =
                "https://github.com/notepad-plus-plus/notepad-plus-plus/releases/download/v8.5.4/npp.8.5.4.Installer.x64.exe",
            DestinationPath = "notepad++.exe"
        };

        var item2 = new DownloadableItem
        {
            Name = "VS Code",
            Description = "The best IDE",
            Url =
                "https://code.visualstudio.com/sha/download?build=stable&os=win32-x64-user",
            DestinationPath = "VS Code.exe"
        };

        _test.Enqueue(item);
        _test.Enqueue(item2);

        await Task.Run(async () =>
        {
            foreach (var item in _test.GetQueueItems())
                if (!item.Data.IsInstalled().Result.Success)
                {
                    // Process the item using processFunction
                    listBoxLogs.Invoke(new Action(() =>
                        listBoxLogs.Items.Add(DateForLog() + $"Downloading {item.Data.Name}")));
                    var result = await item.Data.DownloadFile(progressBar1);
                    listBoxLogs.Invoke(new Action(() =>
                        listBoxLogs.Items.Add(DateForLog() + $"Installing {item.Data.Name}")));
                    var result2 = await item.Data.Install();
                    listBoxLogs.Invoke(new Action(() =>
                        listBoxLogs.Items.Add(DateForLog() + $"Deleting installer {item.Data.Name}")));
                    var result3 = await item.Data.CleanTemp();
                }
            // Add your specific logic here
        });
    }

    private void Form1_Load(object sender, EventArgs e)
    {
        LogComputerInfo();
        listBoxLogs.Items.Add(DateForLog() + $"{typeof(Form1)} loaded");
    }

    private void Form1_Shown(object sender, EventArgs e)
    {
        TestMethod();
    }
}