using System.Diagnostics;
using System.Reflection;
using Authaas_Docker.Models;

namespace Authaas_Docker;

public partial class UpdateManager : Form
{
    public UpdateManager()
    {
        InitializeComponent();
    }

    /// <summary>
    ///     Returns a string with the current UTC date and time.
    /// </summary>
    /// <returns>
    ///     A string with the current UTC date and time in the format "[date] ".
    /// </returns>
    private static string DateForLog()
    {
        return $"[{DateTime.UtcNow}] ";
    }

    public static void RunPowerShellScriptFile(string scriptPath)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = $"-NoProfile -ExecutionPolicy Bypass -File \"{scriptPath}\"",
            UseShellExecute = false,
            RedirectStandardOutput = true
        };

        using (var process = new Process { StartInfo = startInfo })
        {
            process.Start();
            while (!process.StandardOutput.EndOfStream)
            {
                var line = process.StandardOutput.ReadLine();
                Debug.WriteLine(line);
            }
        }
    }

    private async void UpdateManager_Shown(object sender, EventArgs e)
    {
        try
        {
            listBox1.Items.Add(DateForLog() +
                               $"AuthaasDocker v{Assembly.GetExecutingAssembly().GetName().Version}: {GitHubRelease.CalculateCurrentAppHash()}");

            var result = await GitHubRelease.GetLatestReleaseTagAsync("stavrosgiannis", "Authaas-Docker");
            listBox1.Items.Add(DateForLog() +
                               $"Newest AuthaasDocker: {result}");

            listBox1.Items.Add(DateForLog() +
                               "Checking if update is needed..");
            var result2 = await GitHubRelease.DownloadLatestReleaseIfUpdateAvailable(
                Assembly.GetExecutingAssembly().GetName().Version,
                "stavrosgiannis",
                "Authaas-Docker", progressBar1);
            listBox1.Items.Add(DateForLog() + $"{result2.Message}");

            RunPowerShellScriptFile("test.ps1");
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, @"Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Application.Exit();
        }
    }
}