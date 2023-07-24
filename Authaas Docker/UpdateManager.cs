using System.Diagnostics;

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
            listBox1.Items.Add(DateForLog() + $"UpdateManager: {CalculateCurrentAppHash()}");
            if (File.Exists("AuthaasDocker.exe"))
                listBox1.Items.Add(DateForLog() + $"AuthaasDocker: {CalculateFileHash("AuthaasDocker.exe")}");
            else listBox1.Items.Add(DateForLog() + "AuthaasDocker.exe not found!");


            var result = await GetLatestReleaseTagAsync("stavrosgiannis", "Authaas-Docker");
            listBox1.Items.Add(DateForLog() +
                               $"Newest AuthaasDocker: {result}");

            await DownloadLatestReleaseIfUpdateAvailable(CalculateFileHash("AuthaasDocker.exe"), "stavrosgiannis",
                "Authaas-Docker");
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            MessageBox.Show(ex.Message, @"Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Application.Exit();
        }
    }
}