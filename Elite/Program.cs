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
        /// Backfills GravityCache and SignalCache by scanning recent journal files.
        /// Uses a two-pass approach per file: first confirm the file contains the current
        /// system, then harvest all Scan/FSSBodySignals/SAASignalsFound events from it.
        /// This avoids the single-pass bug where scans logged before the Location/FSDJump
        /// line that confirms the system were silently skipped.
        /// Walks backwards through journal files, most recent first, up to 10 files.
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

                        // Pass 1 — does this file mention the current system at all?
                        bool systemFound = false;
                        foreach (var line in lines)
                        {
                            if (string.IsNullOrWhiteSpace(line)) continue;
                            JObject obj;
                            try { obj = JObject.Parse(line); } catch { continue; }

                            var evt = obj.Value<string>("event");
                            if (evt == "FSDJump" || evt == "Location" || evt == "CarrierJump")
                            {
                                if (string.Equals(obj.Value<string>("StarSystem"), currentSystem, StringComparison.OrdinalIgnoreCase))
                                {
                                    systemFound = true;
                                    break;
                                }
                            }
                        }

                        if (!systemFound) continue;

                        // Pass 2 — harvest all scan and signal events from the file.
                        foreach (var line in lines)
                        {
                            if (string.IsNullOrWhiteSpace(line)) continue;
                            JObject obj;
                            try { obj = JObject.Parse(line); } catch { continue; }

                            var evt = obj.Value<string>("event");
                            if (string.IsNullOrEmpty(evt)) continue;

                            if (evt == "Scan")
                            {
                                var bodyName = obj.Value<string>("BodyName");
                                var surfaceGravity = obj.Value<double?>("SurfaceGravity");
                                var radius = obj.Value<double?>("Radius");

                                if (!string.IsNullOrEmpty(bodyName)
                                    && surfaceGravity.HasValue
                                    && radius.HasValue
                                    && surfaceGravity.Value > 0
                                    && !EliteData.GravityCache.ContainsKey(bodyName))
                                {
                                    var atmosphere = obj.Value<string>("Atmosphere")
                                        ?? obj.Value<string>("AtmosphereType") ?? "";
                                    var surfaceTemperature = obj.Value<double?>("SurfaceTemperature") ?? 0;
                                    var planetClass = obj.Value<string>("PlanetClass") ?? "";
                                    var terraformState = obj.Value<string>("TerraformState") ?? "";
                                    var landable = obj.Value<bool?>("Landable") ?? false;

                                    EliteData.GravityCache[bodyName] = (surfaceGravity.Value / 9.81, radius.Value, atmosphere, surfaceTemperature, planetClass, terraformState, landable);
                                    Logger.Instance.LogMessage(TracingLevel.INFO, $"BackfillScanCache: cached {bodyName} ({surfaceGravity.Value / 9.81:F2}g, landable={landable})");
                                }
                            }
                            else if (evt == "FSSBodySignals" || evt == "SAASignalsFound")
                            {
                                var fssBody = obj.Value<string>("BodyName");
                                var signals = obj["Signals"];

                                if (!string.IsNullOrEmpty(fssBody) && signals != null)
                                {
                                    EliteData.SignalCache.TryGetValue(fssBody, out var existing);
                                    int bio = existing.BiologyCount;
                                    int geo = existing.GeologyCount;

                                    foreach (var sig in signals)
                                    {
                                        var sigType = sig.Value<string>("Type") ?? "";
                                        var sigCount = sig.Value<int>("Count");
                                        if (sigType.Contains("Biological")) bio = sigCount;
                                        else if (sigType.Contains("Geological")) geo = sigCount;
                                    }

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

        /// <summary>
        /// Reconstructs ExoBiology scan state from recent journal files on startup, so the button
        /// shows the correct genus, scan count, zone, and distance immediately.
        ///
        /// Strategy:
        ///   • Walks journal files newest→oldest (up to 10 files) exactly like BackfillScanCache.
        ///   • Each file is replayed in forward (chronological) order, tracking:
        ///       – Body context (ApproachBody, Touchdown, Location, Disembark → currentBodyName)
        ///       – Local radius map built from Scan events in the same file, keyed by BodyName
        ///       – ScanOrganic Log/Sample/Analyse events
        ///       – SellOrganicData resets
        ///   • After processing each file: if scan state was found, stop — don't look further back.
        ///   • Radius is resolved from the local per-file Scan map first, then GravityCache, so
        ///     distance works even when the scan happened in a previous game session (different file).
        ///   • A Fileheader event marks the start of a new game session within a file — body context
        ///     is cleared at that point since the player reconnected.
        /// </summary>
        private static void BackfillExoBiologyState(string journalPath)
        {
            try
            {
                var journalFiles = Directory.GetFiles(journalPath, "Journal.*.log")
                    .OrderByDescending(f => f)
                    .Take(10)
                    .ToArray();

                if (journalFiles.Length == 0)
                {
                    Logger.Instance.LogMessage(TracingLevel.INFO, "BackfillExoBio: no journal files found");
                    return;
                }

                Logger.Instance.LogMessage(TracingLevel.INFO,
                    $"BackfillExoBio: checking {journalFiles.Length} journal files");

                foreach (var file in journalFiles)
                {
                    try
                    {
                        string[] lines;
                        using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        using (var reader = new StreamReader(fs))
                            lines = reader.ReadToEnd().Split('\n');

                        Logger.Instance.LogMessage(TracingLevel.INFO, $"BackfillExoBio: processing file {file}");

                        // Local radius map for this file: BodyName → radius in metres
                        // Built from Scan events so we can resolve radius even across sessions.
                        var localRadiusMap = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);

                        // Body name the player was near/on as we step through this file
                        string currentBodyName = null;

                        // Last known lat/lon from Touchdown/Disembark/Location events in this file.
                        // ScanOrganic events do NOT carry Latitude/Longitude in the journal —
                        // we use the most recently recorded position as the sample point instead.
                        double lastKnownLat = double.NaN;
                        double lastKnownLon = double.NaN;

                        // Whether this file contains any ScanOrganic events at all
                        bool fileHasScanState = false;

                        foreach (var raw in lines)
                        {
                            var line = raw.Trim();
                            if (string.IsNullOrEmpty(line)) continue;

                            JObject obj;
                            try { obj = JObject.Parse(line); } catch { continue; }

                            var evt = obj.Value<string>("event");
                            if (string.IsNullOrEmpty(evt)) continue;

                            switch (evt)
                            {
                                // New game session within this file — clear body context
                                case "Fileheader":
                                    currentBodyName = null;
                                    break;

                                // Harvest planet radii from Scan events in this file
                                case "Scan":
                                {
                                    var bn  = obj.Value<string>("BodyName");
                                    var rad = obj.Value<double?>("Radius");
                                    var sg  = obj.Value<double?>("SurfaceGravity");
                                    if (!string.IsNullOrEmpty(bn) && rad.HasValue && rad.Value > 0
                                        && sg.HasValue && sg.Value > 0)
                                    {
                                        localRadiusMap[bn] = rad.Value;
                                    }
                                    break;
                                }

                                // Track which body the player is near, and capture lat/lon where available
                                case "ApproachBody":
                                    currentBodyName = obj.Value<string>("Body")
                                                   ?? obj.Value<string>("BodyName")
                                                   ?? currentBodyName;
                                    Logger.Instance.LogMessage(TracingLevel.INFO, $"BackfillExoBio ApproachBody: currentBodyName={currentBodyName}");
                                    break;

                                case "Touchdown":
                                {
                                    currentBodyName = obj.Value<string>("Body")
                                                   ?? obj.Value<string>("BodyName")
                                                   ?? currentBodyName;
                                    var tdLat = obj.Value<double?>("Latitude");
                                    var tdLon = obj.Value<double?>("Longitude");
                                    if (tdLat.HasValue && tdLon.HasValue) { lastKnownLat = tdLat.Value; lastKnownLon = tdLon.Value; }
                                    Logger.Instance.LogMessage(TracingLevel.INFO, $"BackfillExoBio Touchdown: body={currentBodyName} lat={lastKnownLat:F4} lon={lastKnownLon:F4}");
                                    break;
                                }

                                case "Location":
                                {
                                    var locBody = obj.Value<string>("BodyName");
                                    if (!string.IsNullOrEmpty(locBody)) currentBodyName = locBody;
                                    var locLat = obj.Value<double?>("Latitude");
                                    var locLon = obj.Value<double?>("Longitude");
                                    if (locLat.HasValue && locLon.HasValue) { lastKnownLat = locLat.Value; lastKnownLon = locLon.Value; }
                                    Logger.Instance.LogMessage(TracingLevel.INFO, $"BackfillExoBio Location: body={currentBodyName} lat={lastKnownLat:F4} lon={lastKnownLon:F4}");
                                    break;
                                }

                                case "Disembark":
                                {
                                    var disBody = obj.Value<string>("Body") ?? obj.Value<string>("BodyName");
                                    if (!string.IsNullOrEmpty(disBody)) currentBodyName = disBody;
                                    var disLat = obj.Value<double?>("Latitude");
                                    var disLon = obj.Value<double?>("Longitude");
                                    if (disLat.HasValue && disLon.HasValue) { lastKnownLat = disLat.Value; lastKnownLon = disLon.Value; }
                                    Logger.Instance.LogMessage(TracingLevel.INFO, $"BackfillExoBio Disembark: body={currentBodyName} lat={lastKnownLat:F4} lon={lastKnownLon:F4}");
                                    break;
                                }

                                case "Embark":
                                {
                                    // Boarding the ship — also has lat/lon on the surface
                                    var embLat = obj.Value<double?>("Latitude");
                                    var embLon = obj.Value<double?>("Longitude");
                                    if (embLat.HasValue && embLon.HasValue) { lastKnownLat = embLat.Value; lastKnownLon = embLon.Value; }
                                    break;
                                }

                                case "Liftoff":
                                {
                                    // Liftoff carries lat/lon of the launch point
                                    var lfLat = obj.Value<double?>("Latitude");
                                    var lfLon = obj.Value<double?>("Longitude");
                                    if (lfLat.HasValue && lfLon.HasValue) { lastKnownLat = lfLat.Value; lastKnownLon = lfLon.Value; }
                                    // Do NOT clear body name or reset scan state on Liftoff
                                    break;
                                }

                                case "CodexEntry":
                                {
                                    // CodexEntry fires at the exact player position for each biological scan.
                                    // It is written immediately before ScanOrganic in the journal, making it
                                    // the most accurate source of the actual scan coordinates.
                                    var ceLat = obj.Value<double?>("Latitude");
                                    var ceLon = obj.Value<double?>("Longitude");
                                    if (ceLat.HasValue && ceLon.HasValue) { lastKnownLat = ceLat.Value; lastKnownLon = ceLon.Value; }
                                    break;
                                }

                                case "WalkingBelt":
                                case "ScanBarrierObject":
                                {
                                    // Any on-foot event with lat/lon is a better position than Touchdown
                                    var wbLat = obj.Value<double?>("Latitude");
                                    var wbLon = obj.Value<double?>("Longitude");
                                    if (wbLat.HasValue && wbLon.HasValue) { lastKnownLat = wbLat.Value; lastKnownLon = wbLon.Value; }
                                    break;
                                }

                                case "LeaveBody":
                                case "FSDJump":
                                case "SupercruiseEntry":
                                    currentBodyName = null;
                                    break;

                                case "ScanOrganic":
                                {
                                    fileHasScanState = true;

                                    var scanType     = obj.Value<string>("ScanType") ?? "";
                                    var genusLocal   = obj.Value<string>("Genus_Localised")   ?? obj.Value<string>("Genus")   ?? "";
                                    var speciesLocal = obj.Value<string>("Species_Localised") ?? obj.Value<string>("Species") ?? "";

                                    if (genusLocal.StartsWith("$"))   genusLocal   = string.Empty;
                                    if (speciesLocal.StartsWith("$")) speciesLocal = string.Empty;

                                    string speciesWord = null;
                                    if (!string.IsNullOrEmpty(speciesLocal) && !string.IsNullOrEmpty(genusLocal))
                                    {
                                        speciesWord = speciesLocal
                                            .Replace(genusLocal, "", StringComparison.OrdinalIgnoreCase)
                                            .Trim();
                                        if (string.IsNullOrEmpty(speciesWord)) speciesWord = null;
                                    }

                                    Logger.Instance.LogMessage(TracingLevel.INFO,
                                        $"BackfillExoBio ScanOrganic: type={scanType} genus={genusLocal} species={speciesLocal} " +
                                        $"lat={obj.Value<double?>("Latitude")} lon={obj.Value<double?>("Longitude")} " +
                                        $"body={obj.Value<string>("Body")} bodyId={obj.Value<int?>("Body")} " +
                                        $"currentBodyName={currentBodyName} rawLine={line.Substring(0, Math.Min(200, line.Length))}");

                                    if (scanType == "Log")
                                    {
                                        EliteData.ExoBioGenus          = string.IsNullOrEmpty(genusLocal) ? EliteData.ExoBioGenus : genusLocal;
                                        EliteData.ExoBioSpecies        = speciesWord;
                                        EliteData.ExoBioScanCount      = 1;
                                        // Store scan 1 position; clear scan 2 (fresh sequence)
                                        // ScanOrganic does not carry Latitude/Longitude — use lastKnownLat/Lon
                                        // which was updated by CodexEntry immediately before this event.
                                        EliteData.ExoBioLogLat         = obj.Value<double?>("Latitude")  ?? lastKnownLat;
                                        EliteData.ExoBioLogLon         = obj.Value<double?>("Longitude") ?? lastKnownLon;
                                        EliteData.ExoBioSampleLat      = double.NaN;
                                        EliteData.ExoBioSampleLon      = double.NaN;
                                        EliteData.ExoBioSampleBodyName = currentBodyName;
                                        EliteData.ExoBioSamplePlanetRadius = 0;   // resolved below
                                        Logger.Instance.LogMessage(TracingLevel.INFO,
                                            $"BackfillExoBio Log stored: logLat={EliteData.ExoBioLogLat} logLon={EliteData.ExoBioLogLon} body={EliteData.ExoBioSampleBodyName} (lastKnown={lastKnownLat:F4},{lastKnownLon:F4})");
                                    }
                                    else if (scanType == "Sample")
                                    {
                                        if (!string.IsNullOrEmpty(genusLocal))  EliteData.ExoBioGenus   = genusLocal;
                                        if (!string.IsNullOrEmpty(speciesWord)) EliteData.ExoBioSpecies = speciesWord;
                                        // Increment: first Sample → 2, second Sample → 3
                                        EliteData.ExoBioScanCount      = Math.Min(EliteData.ExoBioScanCount + 1, 3);
                                        EliteData.ExoBioSampleLat      = obj.Value<double?>("Latitude")  ?? lastKnownLat;
                                        EliteData.ExoBioSampleLon      = obj.Value<double?>("Longitude") ?? lastKnownLon;
                                        EliteData.ExoBioSampleBodyName = currentBodyName;
                                        EliteData.ExoBioSamplePlanetRadius = 0;
                                        Logger.Instance.LogMessage(TracingLevel.INFO,
                                            $"BackfillExoBio Sample (scan {EliteData.ExoBioScanCount}) stored: sampleLat={EliteData.ExoBioSampleLat} sampleLon={EliteData.ExoBioSampleLon} logLat={EliteData.ExoBioLogLat} logLon={EliteData.ExoBioLogLon} body={EliteData.ExoBioSampleBodyName} (lastKnown={lastKnownLat:F4},{lastKnownLon:F4})");
                                    }
                                    else if (scanType == "Analyse")
                                    {
                                        // Analyse completes the sequence — reset state entirely
                                        EliteData.ResetExoBioState();
                                        fileHasScanState = true;
                                    }
                                    break;
                                }

                                case "SellOrganicData":
                                    EliteData.ResetExoBioState();
                                    fileHasScanState = true;   // treat as definitive — stop here
                                    break;
                            }
                        }

                        // ── Resolve radius for this file ─────────────────────────────────────
                        // Only attempt if we have an active (incomplete) scan with no radius yet.
                        if (fileHasScanState &&
                            EliteData.ExoBioScanCount > 0 && EliteData.ExoBioScanCount < 3 &&
                            EliteData.ExoBioSamplePlanetRadius <= 0 &&
                            !string.IsNullOrEmpty(EliteData.ExoBioSampleBodyName))
                        {
                            // 1. Direct match in this file's Scan events
                            if (localRadiusMap.TryGetValue(EliteData.ExoBioSampleBodyName, out double r1) && r1 > 0)
                            {
                                EliteData.ExoBioSamplePlanetRadius = r1;
                                Logger.Instance.LogMessage(TracingLevel.INFO,
                                    $"BackfillExoBio: radius {r1:F0}m from local Scan for '{EliteData.ExoBioSampleBodyName}'");
                            }
                            // 2. GravityCache (populated by BackfillScanCache across multiple files)
                            else if (EliteData.GravityCache.TryGetValue(EliteData.ExoBioSampleBodyName, out var gc) && gc.PlanetRadius > 0)
                            {
                                EliteData.ExoBioSamplePlanetRadius = gc.PlanetRadius;
                                Logger.Instance.LogMessage(TracingLevel.INFO,
                                    $"BackfillExoBio: radius {gc.PlanetRadius:F0}m from GravityCache for '{EliteData.ExoBioSampleBodyName}'");
                            }
                            // 3. Suffix fuzzy match in GravityCache (short vs fully-qualified name)
                            else
                            {
                                foreach (var kv in EliteData.GravityCache)
                                {
                                    if (kv.Value.PlanetRadius > 0 &&
                                        (kv.Key.EndsWith(EliteData.ExoBioSampleBodyName, StringComparison.OrdinalIgnoreCase) ||
                                         EliteData.ExoBioSampleBodyName.EndsWith(kv.Key, StringComparison.OrdinalIgnoreCase)))
                                    {
                                        EliteData.ExoBioSamplePlanetRadius = kv.Value.PlanetRadius;
                                        Logger.Instance.LogMessage(TracingLevel.INFO,
                                            $"BackfillExoBio: fuzzy radius {kv.Value.PlanetRadius:F0}m from '{kv.Key}' for '{EliteData.ExoBioSampleBodyName}'");
                                        break;
                                    }
                                }
                                // 4. Last resort: grab radius from local Scan map by suffix match
                                if (EliteData.ExoBioSamplePlanetRadius <= 0)
                                {
                                    foreach (var kv in localRadiusMap)
                                    {
                                        if (kv.Value > 0 &&
                                            (kv.Key.EndsWith(EliteData.ExoBioSampleBodyName, StringComparison.OrdinalIgnoreCase) ||
                                             EliteData.ExoBioSampleBodyName.EndsWith(kv.Key, StringComparison.OrdinalIgnoreCase)))
                                        {
                                            EliteData.ExoBioSamplePlanetRadius = kv.Value;
                                            Logger.Instance.LogMessage(TracingLevel.INFO,
                                                $"BackfillExoBio: fuzzy-local radius {kv.Value:F0}m from '{kv.Key}'");
                                            break;
                                        }
                                    }
                                }

                                if (EliteData.ExoBioSamplePlanetRadius <= 0)
                                    Logger.Instance.LogMessage(TracingLevel.WARN,
                                        $"BackfillExoBio: no radius found for '{EliteData.ExoBioSampleBodyName}' — will retry from StatusData on first tick");
                            }
                        }

                        // Stop walking older files once we've processed a file with scan state
                        if (fileHasScanState) break;
                    }
                    catch (Exception ex)
                    {
                        Logger.Instance.LogMessage(TracingLevel.WARN, $"BackfillExoBio: error reading {file}: {ex.Message}");
                    }
                }

                Logger.Instance.LogMessage(TracingLevel.INFO,
                    $"BackfillExoBio: genus={EliteData.ExoBioGenus} species={EliteData.ExoBioSpecies} " +
                    $"scan={EliteData.ExoBioScanCount} body='{EliteData.ExoBioSampleBodyName}' " +
                    $"logLat={EliteData.ExoBioLogLat:F4} logLon={EliteData.ExoBioLogLon:F4} " +
                    $"sampleLat={EliteData.ExoBioSampleLat:F4} sampleLon={EliteData.ExoBioSampleLon:F4} " +
                    $"radius={EliteData.ExoBioSamplePlanetRadius:F0}m");
            }
            catch (Exception ex)
            {
                Logger.Instance.LogMessage(TracingLevel.FATAL, $"BackfillExoBio: {ex}");
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

                // Backfill exobiology scan state from the current session journal
                BackfillExoBiologyState(journalPath);

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
