using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace Authaas_Docker.Services;

public class IniFile
{
    private readonly string _path;
    private readonly string _exe = Assembly.GetExecutingAssembly().GetName().Name;

    [DllImport("kernel32", CharSet = CharSet.Unicode)]
    private static extern long WritePrivateProfileString(string section, string key, string value, string filePath);

    [DllImport("kernel32", CharSet = CharSet.Unicode)]
    private static extern int GetPrivateProfileString(string section, string key, string @default, StringBuilder retVal,
        int size, string filePath);

    /// <summary>
    /// Constructor for IniFile class.
    /// </summary>
    /// <param name="iniPath">Optional path to the ini file.</param>
    /// <returns>
    /// An instance of the IniFile class.
    /// </returns>
    public IniFile(string? iniPath = null)
    {
        _path = new FileInfo(iniPath ?? _exe + ".ini").FullName;
    }

    /// <summary>
    /// Reads a value from the specified section and key in the ini file.
    /// </summary>
    /// <param name="key">The key to read from.</param>
    /// <param name="section">The section to read from. If null, the exe name is used.</param>
    /// <returns>The value read from the ini file.</returns>
    public string Read(string key, string? section = null)
    {
        var retVal = new StringBuilder(255);
        GetPrivateProfileString(section ?? _exe, key, "", retVal, 255, _path);
        return retVal.ToString();
    }

    /// <summary>
    /// Writes a key/value pair to the INI file.
    /// </summary>
    /// <param name="key">The key to write.</param>
    /// <param name="value">The value to write.</param>
    /// <param name="section">The section to write the key/value pair to. If null, the executable name is used.</param>
    public void Write(string key, string value, string? section = null)
    {
        WritePrivateProfileString(section ?? _exe, key, value, _path);
    }

    /// <summary>
    /// Deletes the specified key from the configuration file.
    /// </summary>
    /// <param name="key">The key to delete.</param>
    /// <param name="section">The section to delete the key from. If null, the key will be deleted from the default section.</param>
    public void DeleteKey(string key, string? section = null)
    {
        Write(key, null, section ?? _exe);
    }

    /// <summary>
    /// Deletes a section from the executable file.
    /// </summary>
    /// <param name="section">The section to delete. If null, the entire executable file will be deleted.</param>
    public void DeleteSection(string? section = null)
    {
        Write(null, null, section ?? _exe);
    }

    /// <summary>
    /// Checks if a key exists in the configuration file.
    /// </summary>
    /// <param name="key">The key to check.</param>
    /// <param name="section">The section to check in (optional).</param>
    /// <returns>True if the key exists, false otherwise.</returns>
    public bool KeyExists(string key, string? section = null)
    {
        return Read(key, section).Length > 0;
    }
}