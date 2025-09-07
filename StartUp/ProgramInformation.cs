using System.Text.Json.Serialization;

namespace StartUp;

public class ProgramInformation
{
    [JsonPropertyName("FileName")]
    public string FileName { get; set; }
    [JsonPropertyName("Arguments")]
    public string? Arguments { get; set; }

    private VirtualDesktopNames _virtualDesktop = VirtualDesktopNames.Default;
    [JsonPropertyName("VirtualDesktop")] 
    public VirtualDesktopNames VirtualDesktop
    {
        get { return _virtualDesktop;}
        set
        {
            _virtualDesktop = value;
            if (!VirtualDesktopNamesList.Contains(value))
            {
                VirtualDesktopNamesList.Add(value);
            }
        }
    }

    public static List<VirtualDesktopNames> VirtualDesktopNamesList = new List<VirtualDesktopNames>();

}