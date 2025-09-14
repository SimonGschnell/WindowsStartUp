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
        var currentVirtualDesktop = VirtualDesktop.Current;
        
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
        
        foreach (var desktopName in ProgramInformation.VirtualDesktopNamesList)
        {
            // Create a new Virtual Desktop for the processes
            var found = desktops.FirstOrDefault(d => d.Name == desktopName.ToString());
            if (found is null)
            {
                var newDesktop =VirtualDesktop.Create();
                newDesktop.Name = desktopName.ToString();
                createdDesktops.Add(newDesktop);
                Console.WriteLine($"Created desktop with name: {newDesktop.Name}");
            }
        }
        // Ensure the default virtual desktop is created
        if (createdDesktops.All(d => d.Name != VirtualDesktopNames.Default.ToString()))
        {
            var vd =VirtualDesktop.Create();
            vd.Name = VirtualDesktopNames.Default.ToString();
            createdDesktops.Add(vd);
            Console.WriteLine($"Created desktop with name: {vd.Name}");
        }

        ExecuteForEachDesktopNameSeperately((program) =>
        {
            var process = program.Arguments is null
                ? Process.Start(program.FileName)
                : Process.Start(new ProcessStartInfo()
                {
                    FileName = program.FileName,
                    Arguments = program.Arguments
                });
            var pName = process.ProcessName;
            if (Process.GetProcessesByName(pName).Length == 0)
            {
                process.WaitForInputIdle();
            }

            FindsHandleAndMovesToDesktop(process,pName, program, createdDesktops);
            

        }, programs);

        Thread.Sleep(1500);
        createdDesktops.Find(d => d.Name == VirtualDesktopNames.Default.ToString())?.Switch();
        currentVirtualDesktop.Remove();

    }

    public static void FindsHandleAndMovesToDesktop(Process p, string pName, ProgramInformation pInfo, List<VirtualDesktop> createdDesktops)
    {
        IntPtr hwnd = IntPtr.Zero;

        if (hwnd == IntPtr.Zero)
        {
            Thread.Sleep(pInfo.SleepBeforeSearch);
            if (CheckByProcessName(pInfo.Name, out IntPtr handle1 , pInfo.IsElectronApp, pInfo.IsPackagedApp) != IntPtr.Zero)
            {
                hwnd = handle1;
            }
            if (hwnd == IntPtr.Zero && CheckByProcessName(pName, out IntPtr handle2, pInfo.IsElectronApp, pInfo.IsPackagedApp) != IntPtr.Zero)
            {
                hwnd = handle2;
            }
        }
                
        Console.WriteLine("Window Handle: " + hwnd);
        var virtualDesk = createdDesktops.First(d => d.Name == pInfo.VirtualDesktop.ToString());
        try
        {
            VirtualDesktop.MoveToDesktop(hwnd, virtualDesk);
        }catch (Exception e)
        {
            Console.WriteLine($"Could not move process {pInfo.Name?? pName} to desktop {virtualDesk.Name}. Error: {e.Message}");
        }
    }
    
    public static List<ProgramInformation> GetProgramsByVDName(IEnumerable<ProgramInformation> programs, VirtualDesktopNames name)
    {
        return programs.Where(d => d.VirtualDesktop == name).ToList();
    }

    public static void ExecuteForEachDesktopNameSeperately(Action<ProgramInformation> a, List<ProgramInformation> collection)
    {
        foreach (var VD_Name in Enum.GetValues<VirtualDesktopNames>())
        {
            var filteredPrograms = GetProgramsByVDName(collection, VD_Name);
            if (filteredPrograms.Count > 0)
            {
                foreach (var program in filteredPrograms)
                {
                    a?.Invoke(program);
                }
            }
        }
    }

    public static IntPtr CheckByProcessName(string processName, out IntPtr h , bool electronApp = false, bool packagedApp = false)
    {
        var procs = Process.GetProcessesByName(processName);
        var windowHandle = FindWindowHandle.GetWindowHandle(processName, electronApp, packagedApp);
        if(windowHandle != IntPtr.Zero){
                h = windowHandle;
                return h;
        }
        foreach (var proc in procs)
        {
            proc?.Refresh();
            if(proc.MainWindowHandle != IntPtr.Zero)
            {
                h = proc.MainWindowHandle;
                return h;
            }
        }
        h = IntPtr.Zero;
        return h;
    }
}

