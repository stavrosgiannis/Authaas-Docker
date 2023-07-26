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
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, @"Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Application.Exit();
        }
    }
}