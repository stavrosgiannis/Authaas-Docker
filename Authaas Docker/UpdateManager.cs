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

    private async void UpdateManager_Shown(object sender, EventArgs e)
    {
        try
        {
            listBox1.Items.Add(DateForLog() +
                               $"AuthaasDocker v{Assembly.GetExecutingAssembly().GetName().Version}: {GitHubRelease.CalculateCurrentAppHash()}");

            var result = await GitHubRelease.GetLatestReleaseTagAsync("stavrosgiannis", "Authaas-Docker");
            listBox1.Items.Add(DateForLog() +
                               $"Newest AuthaasDocker: {result}");

            //    await GitHubRelease.DownloadLatestReleaseIfUpdateAvailable(GitHubRelease.CalculateCurrentAppHash(),
            //        "stavrosgiannis",
            //        "Authaas-Docker");
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            MessageBox.Show(ex.Message, @"Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Application.Exit();
        }
    }
}