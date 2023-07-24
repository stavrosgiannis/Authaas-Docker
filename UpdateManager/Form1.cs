using System.Security.Cryptography;
using System.Text.Json;

namespace UpdateManager;

public partial class Form1 : Form
{
    private static readonly HttpClient Client = new();

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
    private static string DateForLog()
    {
        return $"[{DateTime.UtcNow}] ";
    }


    private void Form1_Load(object sender, EventArgs e)
    {
    }


    private async void Form1_Shown(object sender, EventArgs e)
    {
        //listBox1.Items.Add($"{DateForLog()}Checking for updates");
        //var updateManager = new UpdateManagerClass("https://example.com/latest-version.txt",
        //    "https://example.com/latest-version.exe");
        //await updateManager.CheckForUpdatesAsync();


        listBox1.Items.Add(DateForLog() + $"{CalculateCurrentAppHash()}");


        var result = await GetLatestReleaseTagAsync("stavrosgiannis", "Authaas-Docker");
        listBox1.Items.Add(DateForLog() +
                           $"{result}");
    }

    private string CalculateCurrentAppHash()
    {
        using (var stream = File.OpenRead(Application.ExecutablePath))
        {
            using var sha = SHA1.Create();
            var hash = sha.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }


    private async Task<string?> GetLatestReleaseTagAsync(string repoOwner, string repoName)
    {
        var url = $"https://api.github.com/repos/{repoOwner}/{repoName}/releases/latest";
        Client.DefaultRequestHeaders.UserAgent
            .ParseAdd("Authaas-Docker-Updater"); // GitHub API requires a User-Agent header
        var json = await Client.GetStringAsync(url);
        var release = JsonSerializer.Deserialize<GitHubReleases>(json);

        // Remove the "commit-" prefix from the tag
        var tag = release?.tag_name;
        if (tag != null && tag.StartsWith("sha1-")) tag = tag.Substring("sha1-".Length);

        return tag;
    }
}