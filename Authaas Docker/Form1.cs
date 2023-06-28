using System.Diagnostics;
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

    /// <summary>
    ///     Checks if virtualization is enabled on the current computer.
    /// </summary>
    /// <returns>
    ///     Returns a GenericResult indicating if virtualization is enabled or not.
    /// </returns>
    public async Task<GenericResult> IsVirtualizationEnabled()
    {
        using (var searcher = new ManagementObjectSearcher("SELECT HypervisorPresent FROM Win32_ComputerSystem"))
        {
            foreach (ManagementObject obj in searcher.Get())
            {
                var hypervisorPresent = (bool)obj["HypervisorPresent"];

                if (hypervisorPresent) return GenericResult.Ok(); // Virtualization is enabled
            }
        }

        return GenericResult.Fail("Virtualization not enabled"); // Virtualization is disabled or an error occurred
    }

    public async Task<List<DownloadableItem>> GetDownloadableItemsFromUrl(string urlString)
    {
        List<DownloadableItem> items = new();

        try
        {
            using (var client = new HttpClient())
            {
                // Download the text file from the URL
                var fileContents = await client.GetStringAsync(urlString);

                string[] lines =
                    fileContents.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var line in lines)
                {
                    string[] parts = line.Split(';');

                    if (parts.Length >= 5)
                    {
                        var name = parts[0];
                        var description = parts[1];
                        var runArguments = parts[2];
                        var url = parts[3];
                        var destinationPath = parts[4];
                        var installationDir = parts[5];

                        var item = new DownloadableItem
                        {
                            Name = name,
                            Description = description,
                            RunArguments = runArguments,
                            Url = url,
                            DestinationPath = destinationPath,
                            InstallationDir = installationDir
                        };

                        items.Add(item);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Handle any exception that occurred while downloading or processing the file
            Debug.WriteLine($"Error retrieving file: {ex.Message}");
        }

        return items;
    }

    public async void TestMethod()
    {
        var item = new DownloadableItem
        {
            Name = "Notepad++",
            Description = "The best Editor",
            RunArguments = "/S",
            Url =
                "https://github.com/notepad-plus-plus/notepad-plus-plus/releases/download/v8.5.4/npp.8.5.4.Installer.x64.exe",
            DestinationPath = "notepad++.exe",
            InstallationDir = $@"{Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)}\Notepad++"
        };

        var item2 = new DownloadableItem
        {
            Name = "VS Code",
            Description = "The best IDE",
            Url =
                "https://code.visualstudio.com/sha/download?build=stable&os=win32-x64-user",
            DestinationPath = "VS Code.exe",
            InstallationDir =
                $@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\Programs\Microsoft VS Code"
        };

        var item3 = new DownloadableItem
        {
            Name = "Git",
            Description = "Git repository",
            Url =
                "https://github.com/git-for-windows/git/releases/download/v2.41.0.windows.1/Git-2.41.0-64-bit.exe",
            DestinationPath = "git.exe",
            InstallationDir = $@"{Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)}\Git"
        };

        _test.Enqueue(item);
        _test.Enqueue(item2);
        _test.Enqueue(item3);


        listBoxLogs.Items.Add(DateForLog() + "Starting multi threading Task");
        await Task.Run(async () =>
        {
            var resultVirt = await IsVirtualizationEnabled();

            if (resultVirt.IsFailure)
            {
                listBoxLogs.Invoke(
                    new Action(() =>
                        listBoxLogs.Items.Add(DateForLog() + $"Virtualization: {resultVirt.IsFailure}")));
                return;
            }


            foreach (var item in _test.GetQueueItems())
            {
                listBoxLogs.Invoke(
                    new Action(() =>
                        listBoxLogs.Items.Add(DateForLog() + $"Checking if {item.Data.Name} is installed")));
                if (!item.Data.IsInstalled().Result.Success)
                {
                    // Process the item using processFunction
                    listBoxLogs.Invoke(new Action(() =>
                        listBoxLogs.Items.Add(DateForLog() + $"Downloading {item.Data.Name}")));
                    var result = await item.Data.DownloadFile(progressBar1);
                    if (result.IsFailure)
                        listBoxLogs.Invoke(new Action(() =>
                            listBoxLogs.Items.Add(DateForLog() + $"Download exited with code: {result.Message}")));
                    listBoxLogs.Invoke(new Action(() =>
                        listBoxLogs.Items.Add(DateForLog() + $"Installing {item.Data.Name}")));
                    var result2 = await item.Data.Install();
                    if (result2.IsFailure)
                        listBoxLogs.Invoke(new Action(() =>
                            listBoxLogs.Items.Add(DateForLog() + $"{result2.Message}")));
                    listBoxLogs.Invoke(new Action(() =>
                        listBoxLogs.Items.Add(DateForLog() + $"Deleting installer {item.Data.Name}")));
                    var result3 = await item.Data.CleanTemp();
                    if (result3.IsFailure)
                        listBoxLogs.Invoke(new Action(() =>
                            listBoxLogs.Items.Add(DateForLog() +
                                                  $"Deleting installer exited with code: {result3.Message}")));
                }
                else
                {
                    listBoxLogs.Invoke(
                        new Action(() =>
                            listBoxLogs.Items.Add(DateForLog() + $"Found {item.Data.Name} installation")));
                }
            }


            // Add your specific logic here
        });
    }

    /// <summary>
    ///     This method is used to log computer information and add a log item to the list box when the form is loaded.
    /// </summary>
    private void Form1_Load(object sender, EventArgs e)
    {
        LogComputerInfo();
        listBoxLogs.Items.Add(DateForLog() + $"{typeof(Form1)} loaded");
    }

    private void Form1_Shown(object sender, EventArgs e)
    {
    }

    private void button1_Click(object sender, EventArgs e)
    {
        TestMethod();
    }

    private void timer1_Tick(object sender, EventArgs e)
    {
        listBoxLogs.TopIndex = listBoxLogs.Items.Count - 1;
    }
}