using System.Diagnostics;
using System.Management;
using Authaas_Docker.Models;
using Authaas_Docker.Properties;

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

    /// <summary>
    ///     Retrieves a list of DownloadableItems from a given URL.
    /// </summary>
    /// <param name="urlString">The URL to retrieve the DownloadableItems from.</param>
    /// <returns>A list of DownloadableItems.</returns>
    public async Task<List<DownloadableItem>> GetDownloadableItemsFromUrl(string urlString)
    {
        List<DownloadableItem> items = new();

        try
        {
            using (var client = new HttpClient())
            {
                // Download the text file from the URL
                var fileContents = await client.GetStringAsync(urlString);

                var lines =
                    fileContents.Split('\n');

                foreach (var line in lines)
                {
                    var parts = line.Split(';');

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
        listBoxLogs.Items.Add(DateForLog() + "Starting multi threading Task");
        await Task.Run(async () =>
        {
            var resultVirt = await IsVirtualizationEnabled();

            if (resultVirt.IsFailure)
            {
                listBoxLogs.Invoke(
                    new Action(() =>
                        listBoxLogs.Items.Add(DateForLog() + $"Virtualization: {resultVirt.Success}")));
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
                    }
                    else
                    {
                        if (item.Data.Name.Contains("Rancher"))
                            if (Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                                                 @"\rancher-desktop"))
                            {
                                await File.WriteAllBytesAsync(
                                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                                    @"\rancher-desktop\settings.json", Resources.settings);
                            }
                            else
                            {
                                Directory.CreateDirectory(
                                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                                    @"\rancher-desktop");
                                await File.WriteAllBytesAsync(
                                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                                    @"\rancher-desktop\settings.json", Resources.settings);
                            }
                    }

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

                    if (item.Data.Name.Contains("Rancher"))
                        if (Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                                             "rancher-desktop"))
                            await File.WriteAllBytesAsync(
                                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                                "rancher-desktop", Resources.settings);
                }
            }
        });

        listBoxLogs.Items.Add(DateForLog() + "Done!");
        listBoxLogs.Items.Add("-----------------------");
    }

    /// <summary>
    ///     This method is used to log computer information and add a log item to the list box when the form is loaded.
    /// </summary>
    private void Form1_Load(object sender, EventArgs e)
    {
        LogComputerInfo();
        listBoxLogs.Items.Add(DateForLog() + $"{typeof(Form1)} loaded");
    }

    private async void Form1_Shown(object sender, EventArgs e)
    {
        var result = await GetDownloadableItemsFromUrl(
            "https://raw.githubusercontent.com/stavrosgiannis/Authaas-Docker/master/Authaas%20Docker/queueItems.txt");

        foreach (var entry in result) _test.Enqueue(entry);
    }

    private void button1_Click(object sender, EventArgs e)
    {
        TestMethod();
    }
}