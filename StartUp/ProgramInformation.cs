using System.Text.Json.Serialization;

namespace StartUp;

public class ProgramInformation
{
    [JsonPropertyName("Name")]
    public string? Name { get; set; }
    
    [JsonPropertyName("FileName")]
    public string FileName { get; set; }
    
    [JsonPropertyName("Arguments")]
    public string? Arguments { get; set; }

    [JsonPropertyName("IsElectronApp")] 
    public bool IsElectronApp { get; set; } = false;
    [JsonPropertyName("SleepBeforeSearch")] 
    public int SleepBeforeSearch { get; set; } = 700;

    [JsonPropertyName("IsPackagedApp")] 
    public bool IsPackagedApp { get; set; } = false;

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