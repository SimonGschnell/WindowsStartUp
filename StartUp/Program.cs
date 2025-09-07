using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using WindowsDesktop;

namespace StartUp;

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        var jsonOptions = new JsonSerializerOptions();
        jsonOptions.Converters.Add(new JsonStringEnumConverter());
        
        // Read Json file and deserialize
        var programsJson = File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"Programs.json"));
        List<ProgramInformation>? programs = JsonSerializer.Deserialize<List<ProgramInformation>>(programsJson, jsonOptions);

        // Keep track of existing and newly created desktops
        var desktops = VirtualDesktop.GetDesktops();
        var createdDesktops = new List<VirtualDesktop>();
        foreach (var d in desktops)
        {
            createdDesktops.Add(d);
        }
        
        // Create a new Virtual Desktop for the processes
        foreach (var desktopName in ProgramInformation.VirtualDesktopNamesList)
        {
            Console.WriteLine($"desktopNames statis: {desktopName}");
            var found = desktops.FirstOrDefault(d => d.Name == desktopName.ToString());
            if (found is null)
            {
                var newDesktop =VirtualDesktop.Create();
                newDesktop.Name = desktopName.ToString();
                createdDesktops.Add(newDesktop);
                Console.WriteLine($"Created desktop with name: {newDesktop.Name}");
            }
        }

        // Populate the virtual desktops one after another
        foreach (var VD_Name in Enum.GetValues<VirtualDesktopNames>())
        {
            var filteredPrograms = GetProgramsByVDName(programs, VD_Name);
            if (filteredPrograms.Count > 0)
            {
                var programDesktop = createdDesktops.First(d => d.Name == VD_Name.ToString());
                Console.WriteLine($"Switching to desktop: {VD_Name.ToString()}");
                programDesktop.Switch();
                foreach(var program in filteredPrograms)
                {
                
                
                    if (program.Arguments is null)
                    {
                        var p = Process.Start(program.FileName);
                    }
                    else
                    {
                        var p = Process.Start(new ProcessStartInfo()
                        {
                            FileName = program.FileName,
                            Arguments = program.Arguments
                        });
                    }
                    
                }
                Thread.Sleep(5000);
            }
        }
        
    }
    
    
    
    public static List<ProgramInformation> GetProgramsByVDName(IEnumerable<ProgramInformation> programs, VirtualDesktopNames name)
    {
        return programs.Where(d => d.VirtualDesktop == name).ToList();
    }
}

