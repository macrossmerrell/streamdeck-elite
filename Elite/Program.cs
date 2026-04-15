using BarRaider.SdTools;
using EliteJournalReader;
using EliteJournalReader.Events;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;


namespace Elite
{

    public class KeyBindingFileEvent : EventArgs
    {

    }

    public class KeyBindingWatcher : FileSystemWatcher
    {
        public event EventHandler KeyBindingUpdated;

        protected KeyBindingWatcher()
        {

        }

        public KeyBindingWatcher(string path, string fileName)
        {
            Filter = fileName;
            NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.LastWrite;
            Path = path;
        }

        public virtual void StartWatching()
        {
            if (EnableRaisingEvents)
            {
                return;
            }

            Changed -= UpdateStatus;
            Changed += UpdateStatus;

            EnableRaisingEvents = true;
        }

        public virtual void StopWatching()
        {
            try
            {
                if (EnableRaisingEvents)
                {
                    Changed -= UpdateStatus;

                    EnableRaisingEvents = false;
                }
            }
            catch (Exception e)
            {
                Trace.TraceError($"Error while stopping Status watcher: {e.Message}");
                Trace.TraceInformation(e.StackTrace);
            }
        }

        protected void UpdateStatus(object sender, FileSystemEventArgs e)
        {
            Thread.Sleep(50);

            KeyBindingUpdated?.Invoke(this, EventArgs.Empty);
        }
       

    }
    

    class Program
    {
        public static FifoExecution keywatcherjob = new FifoExecution(); 

        public static KeyBindingWatcher KeyBindingWatcherStartPreset;
        public static StatusWatcher StatusWatcher;
        public static CargoWatcher CargoWatcher;
        public static NavRouteWatcher NavRouteWatcher;
        public static JournalWatcher JournalWatcher;

        public static Dictionary<BindingType, UserBindings> Binding = new Dictionary<BindingType, UserBindings>();

        public static KeyBindingWatcher[] KeyBindingWatcher = new KeyBindingWatcher[4];

        private class UnsafeNativeMethods
        {
            [DllImport("Shell32.dll")]
            public static extern int SHGetKnownFolderPath([MarshalAs(UnmanagedType.LPStruct)]Guid rfid, uint dwFlags, IntPtr hToken, out IntPtr ppszPath);
        }

        /// <summary>
        /// The standard Directory of the Player Journal files (C:\Users\%username%\Saved Games\Frontier Developments\Elite Dangerous).
        /// </summary>
        public static DirectoryInfo StandardDirectory
        {
            get
            {
                int result = UnsafeNativeMethods.SHGetKnownFolderPath(new Guid("4C5C32FF-BB9D-43B0-B5B4-2D72E54EAAA4"), 0, new IntPtr(0), out IntPtr path);
                if (result >= 0)
                {
                    try { return new DirectoryInfo(Marshal.PtrToStringUni(path) + @"\Frontier Developments\Elite Dangerous"); }
                    catch { return new DirectoryInfo(Directory.GetCurrentDirectory()); }
                }
                else
                {
                    return new DirectoryInfo(Directory.GetCurrentDirectory());
                }
            }
        }

        public static void HandleKeyBindingEvents(object sender, object evt)
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, $"Reloading Key Bindings");

            keywatcherjob.QueueUserWorkItem(GetKeyBindings, null);
        }
        private static void WatchJournalForSignals(string journalPath)
        {
            Task.Run(() =>
            {
                try
                {
                    // Find the most recent journal file
                    string currentFile = null;
                    long filePosition = 0;

                    while (true)
                    {
                        try
                        {
                            // Always watch the most recent journal file
                            var latestFile = Directory.GetFiles(journalPath, "Journal.*.log")
                                .OrderByDescending(f => f)
                                .FirstOrDefault();

                            if (latestFile != currentFile)
                            {
                                currentFile = latestFile;
                                filePosition = 0;
                                Logger.Instance.LogMessage(TracingLevel.INFO, $"SignalWatcher: watching {currentFile}");
                            }

                            if (currentFile == null) { System.Threading.Thread.Sleep(1000); continue; }

                            using (var fs = new FileStream(currentFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                            {
                                fs.Seek(filePosition, SeekOrigin.Begin);
                                using (var reader = new StreamReader(fs))
                                {
                                    string line;
                                    while ((line = reader.ReadLine()) != null)
                                    {
                                        if (string.IsNullOrWhiteSpace(line)) continue;
                                        if (!line.Contains("FSSBodySignals") && !line.Contains("SAASignalsFound")) continue;

                                        try
                                        {
                                            var obj = JObject.Parse(line);
                                            var evt = obj.Value<string>("event");
                                            var bodyName = obj.Value<string>("BodyName");
                                            var signals = obj["Signals"];

                                            if (string.IsNullOrEmpty(bodyName) || signals == null) continue;

                                            EliteData.SignalCache.TryGetValue(bodyName, out var existing);
                                            int bio = existing.BiologyCount, geo = existing.GeologyCount;

                                            foreach (var sig in signals)
                                            {
                                                var sigType = sig.Value<string>("Type") ?? "";
                                                var sigCount = sig.Value<int>("Count");
                                                if (sigType.Contains("Biological")) bio = sigCount;
                                                else if (sigType.Contains("Geological")) geo = sigCount;
                                            }

                                            EliteData.SignalCache[bodyName] = (bio, geo);
                                            Logger.Instance.LogMessage(TracingLevel.INFO, $"SignalWatcher: {bodyName} bio={bio} geo={geo}");
                                        }
                                        catch { }
                                    }
                                    filePosition = fs.Position;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Instance.LogMessage(TracingLevel.WARN, $"SignalWatcher error: {ex.Message}");
                        }

                        System.Threading.Thread.Sleep(1000);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Instance.LogMessage(TracingLevel.FATAL, $"SignalWatcher fatal: {ex}");
                }
            });
        }

        private static bool IsFileLocked(FileInfo file)
        {
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None);
            }
            catch (IOException e)
            {
                if (file.FullName.ToLower().Contains(".start"))
                {
                    Logger.Instance.LogMessage(TracingLevel.ERROR, $"error opening file {file.FullName} {e}");
                }

                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }
            finally
            {
                stream?.Close();
            }

            //file is not locked
            return false;
        }


        // copied from https://github.com/MagicMau/EliteJournalReader

        private static FileInfo FileInfo(string cargoPath)
        {
            try
            {
                var info = new FileInfo(cargoPath);
                if (info.Exists)
                {
                    // This info can be cached so force a refresh
                    info.Refresh();
                }
                return info;
            }
            catch { return null; }
        }


        // copied from https://github.com/MagicMau/EliteJournalReader
        public static string[] ReadStartPreset(string startPresetPath)
        {
            try
            {
                Thread.Sleep(100);

                FileInfo fileInfo = null;
                try
                {
                    fileInfo = FileInfo(startPresetPath);
                }
                catch (Exception e)
                {
                    Logger.Instance.LogMessage(TracingLevel.ERROR, $"error opening file {startPresetPath} {e}");
                }

                if (fileInfo != null)
                {
                    var maxTries = 6;
                    while (IsFileLocked(fileInfo))
                    {
                        Thread.Sleep(100);
                        maxTries--;
                        if (maxTries == 0)
                        {
                            Logger.Instance.LogMessage(TracingLevel.ERROR, $"file still locked. exiting {startPresetPath}");

                            return null;
                        }
                    }

                    using (var fs = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    using (var reader = new StreamReader(fs, Encoding.UTF8))
                    {
                        fs.Seek(0, SeekOrigin.Begin);
                        var bindsNames = reader.ReadToEnd();

                        Logger.Instance.LogMessage(TracingLevel.INFO, $"startpreset contents : {bindsNames}");

                        if (string.IsNullOrEmpty(bindsNames))
                        {
                            return null;
                        }
                        return bindsNames.Split('\n');

                    }
                }
            }
            catch (Exception e)
            {
                Logger.Instance.LogMessage(TracingLevel.FATAL, e.ToString());
            }

            return null;
        }
        
        public static bool HandleKeyBinding(BindingType bindingType, string  bindingsPath, string bindsName)
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, "handle key binding " + bindsName);

            if (KeyBindingWatcher[(int)bindingType] != null)
            {
                KeyBindingWatcher[(int)bindingType].StopWatching();
                KeyBindingWatcher[(int)bindingType].Dispose();
                KeyBindingWatcher[(int)bindingType] = null;
            }


            var fileName = Path.Combine(bindingsPath, bindsName + ".4.2.binds");

            if (!File.Exists(fileName))
            {
                Logger.Instance.LogMessage(TracingLevel.ERROR, "file not found " + fileName);

                fileName = fileName.Replace(".4.2.binds", ".4.1.binds");

                if (!File.Exists(fileName))
                {
                    Logger.Instance.LogMessage(TracingLevel.ERROR, "file not found " + fileName);

                    fileName = fileName.Replace(".4.1.binds", ".4.0.binds");

                    if (!File.Exists(fileName))
                    {
                        Logger.Instance.LogMessage(TracingLevel.ERROR, "file also not found " + fileName);

                        fileName = fileName.Replace(".4.0.binds", ".3.0.binds");

                        if (!File.Exists(fileName))
                        {
                            Logger.Instance.LogMessage(TracingLevel.ERROR, "file also not found " + fileName);

                            fileName = fileName.Replace(".3.0.binds", ".binds");

                            if (!File.Exists(fileName))
                            {
                                Logger.Instance.LogMessage(TracingLevel.ERROR, "file also not found " + fileName);
                            }
                        }
                    }
                }
            }

            // steam
            if (!File.Exists(fileName))
            {
                bindingsPath = SteamPath.FindSteamEliteDirectory();

                if (!string.IsNullOrEmpty(bindingsPath))
                {
                    fileName = Path.Combine(bindingsPath, bindsName + ".4.2.binds");

                    if (!File.Exists(fileName))
                    {
                        Logger.Instance.LogMessage(TracingLevel.ERROR, "steam file not found " + fileName);

                        fileName = fileName.Replace(".4.2.binds", ".4.1.binds");

                        if (!File.Exists(fileName))
                        {
                            Logger.Instance.LogMessage(TracingLevel.ERROR, "steam file not found " + fileName);

                            fileName = fileName.Replace(".4.1.binds", ".4.0.binds");

                            if (!File.Exists(fileName))
                            {
                                Logger.Instance.LogMessage(TracingLevel.ERROR, "steam file also not found " + fileName);

                                fileName = fileName.Replace(".4.0.binds", ".3.0.binds");

                                if (!File.Exists(fileName))
                                {
                                    Logger.Instance.LogMessage(TracingLevel.ERROR, "steam file also not found " + fileName);

                                    fileName = fileName.Replace(".3.0.binds", ".binds");

                                    if (!File.Exists(fileName))
                                    {
                                        Logger.Instance.LogMessage(TracingLevel.ERROR, "steam file also not found " + fileName);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // epic
            if (!File.Exists(fileName))
            {
                bindingsPath = EpicPath.FindEpicEliteDirectory();

                if (!string.IsNullOrEmpty(bindingsPath))
                {
                    fileName = Path.Combine(bindingsPath, bindsName + ".4.2.binds");

                    if (!File.Exists(fileName))
                    {
                        Logger.Instance.LogMessage(TracingLevel.ERROR, "epic file not found " + fileName);

                        fileName = fileName.Replace(".4.2.binds", ".4.1.binds");

                        if (!File.Exists(fileName))
                        {
                            Logger.Instance.LogMessage(TracingLevel.ERROR, "epic file not found " + fileName);

                            fileName = fileName.Replace(".4.1.binds", ".4.0.binds");

                            if (!File.Exists(fileName))
                            {
                                Logger.Instance.LogMessage(TracingLevel.ERROR, "epic file also not found " + fileName);

                                fileName = fileName.Replace(".4.0.binds", ".3.0.binds");

                                if (!File.Exists(fileName))
                                {
                                    Logger.Instance.LogMessage(TracingLevel.ERROR, "epic file also not found " + fileName);

                                    fileName = fileName.Replace(".3.0.binds", ".binds");

                                    if (!File.Exists(fileName))
                                    {
                                        Logger.Instance.LogMessage(TracingLevel.ERROR, "epic file not found " + fileName);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (File.Exists(fileName))
            {
                var serializer = new XmlSerializer(typeof(UserBindings));

                //Logger.Instance.LogMessage(TracingLevel.INFO, "using " + fileName);

                var reader = new StreamReader(fileName);
                Binding[bindingType] = (UserBindings)serializer.Deserialize(reader);
                reader.Close();


                var keyBindingPath = Path.GetDirectoryName(fileName);
                Logger.Instance.LogMessage(TracingLevel.INFO, "monitoring key binding path #2 " + keyBindingPath);
                var keyBindingFileName = Path.GetFileName(fileName);


                Logger.Instance.LogMessage(TracingLevel.INFO, "monitoring key binding file name #2 " + keyBindingFileName);

                KeyBindingWatcher[(int)bindingType] = new KeyBindingWatcher(keyBindingPath, keyBindingFileName);
                KeyBindingWatcher[(int)bindingType].KeyBindingUpdated += HandleKeyBindingEvents;
                KeyBindingWatcher[(int)bindingType].StartWatching();
            }
            else
            {
                Logger.Instance.LogMessage(TracingLevel.ERROR, "file not found " + fileName);

                return false;
            }

            return true;
        }


        private static void GetKeyBindings(Object threadContext)
        {
            if (KeyBindingWatcherStartPreset != null)
            {
                KeyBindingWatcherStartPreset.StopWatching();
                KeyBindingWatcherStartPreset.Dispose();
                KeyBindingWatcherStartPreset = null;
            }

            Logger.Instance.LogMessage(TracingLevel.INFO, $"LocalApplicationData path {Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}");

            var bindingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Frontier Developments\Elite Dangerous\Options\Bindings");

            if (!Directory.Exists(bindingsPath))
            {
                Logger.Instance.LogMessage(TracingLevel.FATAL, $"Directory doesn't exist {bindingsPath}");
            }
            else
            {
                Logger.Instance.LogMessage(TracingLevel.INFO, $"Directory found {bindingsPath}");
            }

            var startPresetPath = Path.Combine(bindingsPath, "StartPreset.4.start");

            if (!File.Exists(startPresetPath))
            {
                Logger.Instance.LogMessage(TracingLevel.INFO, $"StartPreset.4.start not found {startPresetPath}");

                startPresetPath = Path.Combine(bindingsPath, "StartPreset.start");

                if (!File.Exists(startPresetPath))
                {
                    Logger.Instance.LogMessage(TracingLevel.INFO, $"StartPreset.start also not found {startPresetPath}");
                }
            }
            else
            {
                Logger.Instance.LogMessage(TracingLevel.INFO, $"StartPreset.4.start found {startPresetPath}");
            }

            //Logger.Instance.LogMessage(TracingLevel.INFO, "bindings path " + bindingsPath);

            var bindsNames = ReadStartPreset(startPresetPath);

            Logger.Instance.LogMessage(TracingLevel.INFO, "key bindings " + string.Join(",",bindsNames));

            var keyBindingPath = Path.GetDirectoryName(startPresetPath);
            Logger.Instance.LogMessage(TracingLevel.INFO, "monitoring key binding path #1 " + keyBindingPath);
            var keyBindingFileName = Path.GetFileName(startPresetPath);

            Logger.Instance.LogMessage(TracingLevel.INFO, "monitoring key binding file name #1 " + keyBindingFileName);
            KeyBindingWatcherStartPreset = new KeyBindingWatcher(keyBindingPath, keyBindingFileName);
            KeyBindingWatcherStartPreset.KeyBindingUpdated += HandleKeyBindingEvents;
            KeyBindingWatcherStartPreset.StartWatching();

            if (bindsNames.Length == 4) // odyssey
            {
                Logger.Instance.LogMessage(TracingLevel.INFO, "odyssey key bindings");

                HandleKeyBinding(BindingType.General, bindingsPath, bindsNames[0]);
                HandleKeyBinding(BindingType.Ship, bindingsPath, bindsNames[1]);
                HandleKeyBinding(BindingType.Srv, bindingsPath, bindsNames[2]);
                var found = HandleKeyBinding(BindingType.OnFoot, bindingsPath, bindsNames[3]);

                if (!found)
                {
                    Logger.Instance.LogMessage(TracingLevel.INFO, "not found, retry on foot binding with key binding " + bindsNames[2]);
                    HandleKeyBinding(BindingType.OnFoot, bindingsPath, bindsNames[2]);
                }
            }
            else // horizon
            {
                Logger.Instance.LogMessage(TracingLevel.INFO, "horizon key bindings");

                HandleKeyBinding(BindingType.General, bindingsPath, bindsNames.First());
                HandleKeyBinding(BindingType.Ship, bindingsPath, bindsNames.First());
                HandleKeyBinding(BindingType.Srv, bindingsPath, bindsNames.First());
                HandleKeyBinding(BindingType.OnFoot, bindingsPath, bindsNames.First());
            }

        }

        /// <summary>
        /// Backfills the GravityCache by scanning recent journal files for Scan events
        /// matching the current star system. Walks backwards through journal files,
        /// most recent first, up to 10 files back.
        /// </summary>
        private static void BackfillScanCache(string journalPath)
        {
            try
            {
                var currentSystem = EliteData.StarSystem;
                if (string.IsNullOrEmpty(currentSystem))
                {
                    Logger.Instance.LogMessage(TracingLevel.INFO, "BackfillScanCache: no current system, skipping");
                    return;
                }

                Logger.Instance.LogMessage(TracingLevel.INFO, $"BackfillScanCache: scanning for system '{currentSystem}'");

                // Get all journal files sorted most-recent first
                var journalFiles = Directory.GetFiles(journalPath, "Journal.*.log")
                    .OrderByDescending(f => f)
                    .Take(10)
                    .ToArray();

                Logger.Instance.LogMessage(TracingLevel.INFO, $"BackfillScanCache: checking {journalFiles.Length} journal files");

                foreach (var file in journalFiles)
                {
                    try
                    {
                        var lines = File.ReadAllLines(file);
                        bool systemFound = false;

                        foreach (var line in lines)
                        {
                            if (string.IsNullOrWhiteSpace(line)) continue;

                            JObject obj;
                            try { obj = JObject.Parse(line); }
                            catch { continue; }

                            var evt = obj.Value<string>("event");
                            if (string.IsNullOrEmpty(evt)) continue;

                            // Check if this file contains our current system
                            if (evt == "FSDJump" || evt == "Location" || evt == "CarrierJump")
                            {
                                if (obj.Value<string>("StarSystem") == currentSystem)
                                    systemFound = true;
                            }

                            // Cache Scan events if we've confirmed system presence
                            if (evt == "Scan" && systemFound)
                            {
                                var bodyName = obj.Value<string>("BodyName");
                                var surfaceGravity = obj.Value<double?>("SurfaceGravity");
                                var radius = obj.Value<double?>("Radius");
                                var atmosphere = obj.Value<string>("Atmosphere") 
                                    ?? obj.Value<string>("AtmosphereType") ?? "";
                                var surfaceTemperature = obj.Value<double?>("SurfaceTemperature") ?? 0;

                                if (!string.IsNullOrEmpty(bodyName)
                                    && surfaceGravity.HasValue
                                    && radius.HasValue
                                    && surfaceGravity.Value > 0
                                    && !EliteData.GravityCache.ContainsKey(bodyName))
                                {
                                    var planetClass = obj.Value<string>("PlanetClass") ?? "";
                                    var terraformState = obj.Value<string>("TerraformState") ?? "";
                                    EliteData.GravityCache[bodyName] = (surfaceGravity.Value / 9.81, radius.Value, atmosphere, surfaceTemperature, planetClass, terraformState);
                                }
                            }
                            // --- New Signal Cache Backfill Block Starts Here ---
                            else if (evt == "FSSBodySignals" || evt == "SAASignalsFound") 
                            {
                                var fssBody = obj.Value<string>("BodyName");
                                var signals = obj["Signals"];

                                if (!string.IsNullOrEmpty(fssBody) && signals != null)
                                {
                                    // Check if we already have partial data for this body to avoid overwriting existing counts
                                    EliteData.SignalCache.TryGetValue(fssBody, out var existing);
                                    int bio = existing.BiologyCount;
                                    int geo = existing.GeologyCount;

                                    foreach (var sig in signals)
                                    {
                                        var sigType = sig.Value<string>("Type") ?? "";
                                        var sigCount = sig.Value<int>("Count");

                                        if (sigType.Contains("Biological")) 
                                            bio = sigCount;
                                        else if (sigType.Contains("Geological")) 
                                            geo = sigCount;
                                    }

                                    // Update the static cache with the new bio/geo tallies
                                    EliteData.SignalCache[fssBody] = (bio, geo);
                                    Logger.Instance.LogMessage(TracingLevel.INFO, $"BackfillSignalCache: {fssBody} bio={bio} geo={geo}");
                                }
                            }                        
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Instance.LogMessage(TracingLevel.WARN, $"BackfillScanCache: error reading {file}: {ex.Message}");
                    }
                }

                Logger.Instance.LogMessage(TracingLevel.INFO, 
                    $"BackfillScanCache: complete, {EliteData.GravityCache.Count} bodies cached");
            }
            catch (Exception ex)
            {
                Logger.Instance.LogMessage(TracingLevel.FATAL, $"BackfillScanCache: {ex}");
            }
        }

        static void Main(string[] args)
        {
            // Uncomment this line of code to allow for debugging
            //while (!System.Diagnostics.Debugger.IsAttached) { System.Threading.Thread.Sleep(100); }

            Logger.Instance.LogMessage(TracingLevel.INFO, "Init Elite Api");

            try
            {
                GetKeyBindings(null);


                var journalPath = StandardDirectory.FullName;

                Logger.Instance.LogMessage(TracingLevel.INFO, "journal path " + journalPath);

                if (!Directory.Exists(journalPath))
                {
                    Logger.Instance.LogMessage(TracingLevel.FATAL, $"Directory doesn't exist {journalPath}");
                }

                var defaultFilter = @"Journal.*.log";
//#if DEBUG
            //defaultFilter = @"JournalAlpha.*.log";
//#endif

                StatusWatcher = new StatusWatcher(journalPath);

                StatusWatcher.StatusUpdated += EliteData.HandleStatusEvents;

                StatusWatcher.StartWatching();

                JournalWatcher = new JournalWatcher(journalPath, defaultFilter);

                JournalWatcher.AllEventHandler += EliteData.HandleEliteEvents;

                JournalWatcher.StartWatching().Wait();

                // Backfill scan cache from recent journal files for current star system
                BackfillScanCache(journalPath);

                // Watch journal for FSSBodySignals and SAASignalsFound independently
                WatchJournalForSignals(journalPath);

                CargoWatcher = new CargoWatcher(journalPath);

                CargoWatcher.CargoUpdated += EliteData.HandleCargoEvents;

                CargoWatcher.StartWatching();

                NavRouteWatcher = new NavRouteWatcher(journalPath);

                NavRouteWatcher.NavRouteUpdated += EliteData.HandleNavRouteEvents;

                NavRouteWatcher.StartWatching();
            }
            catch (Exception ex)
            {
                Logger.Instance.LogMessage(TracingLevel.FATAL, $"Elite Api: {ex}");
            }


            //EliteAPI.Events.AllEvent += (sender, e) => Console.Beep();

            Profile.ReadProfiles();


            SDWrapper.Run(args);
        }


    }
}
