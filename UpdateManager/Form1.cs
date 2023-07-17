namespace UpdateManager;

public partial class Form1 : Form
{
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

    private void Form1_Load(object sender, EventArgs e)
    {
    }

    private async void Form1_Shown(object sender, EventArgs e)
    {
        listBox1.Items.Add($"{DateForLog()}Checking for updates");
        var updateManager = new UpdateManagerClass("https://example.com/latest-version.txt",
            "https://example.com/latest-version.exe");
        await updateManager.CheckForUpdatesAsync();
    }

}