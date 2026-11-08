using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Collections.Generic;
using Microsoft.Win32;

namespace ELX11
{
    class Program
    {
        const string VERSION = "v2.0 Community Edition";
        const string SERVER_IP = "63.178.232.241";
        const string SERVER_DOMAIN = "elx11.wizcake.space";
        const string API_KEY = "elx11_bot_secret_2026";

        static readonly string[] REDIRECT_DOMAINS = new string[]
        {
            "pes11pcgate-e.winning-eleven.net",
            "pes11pcgate-u.winning-eleven.net",
            "pes11pcgate-j.winning-eleven.net",
            "pes11pcgate.winning-eleven.net",
            "pes7stun-e.winning-eleven.net",
            "pes7stun-u.winning-eleven.net",
            "pes7stun-j.winning-eleven.net",
            "pes7stun.winning-eleven.net",
            "id.konami.net",
            "locate.madserver.net"
        };

        static readonly string HOSTS_PATH = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.System),
            @"drivers\etc\hosts"
        );

        [DllImport("dnsapi.dll", EntryPoint = "DnsFlushResolverCache")]
        static extern UInt32 DnsFlushResolverCache();

        [DllImport("user32.dll")]
        static extern int GetSystemMetrics(int nIndex);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private class MEMORYSTATUSEX
        {
            public uint dwLength;
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;
            public MEMORYSTATUSEX()
            {
                this.dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
            }
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);

        static bool IsAdministrator()
        {
            try
            {
                WindowsIdentity id = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(id);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch
            {
                return false;
            }
        }

        static void RelaunchElevated()
        {
            if (IsAdministrator()) return;
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = Process.GetCurrentProcess().MainModule.FileName;
                psi.UseShellExecute = true;
                psi.Verb = "runas";
                Process.Start(psi);
                Environment.Exit(0);
            }
            catch
            {
                // User declined UAC or non-interactive pipe: continue in standard mode
            }
        }

        static void FlushDns()
        {
            try
            {
                DnsFlushResolverCache();
            }
            catch { }
        }

        static bool IsConnected()
        {
            if (!File.Exists(HOSTS_PATH)) return false;
            try
            {
                string text = File.ReadAllText(HOSTS_PATH);
                return text.Contains("pes11pcgate-e.winning-eleven.net") && text.Contains(SERVER_IP);
            }
            catch
            {
                return false;
            }
        }

        static bool UpdateHosts(bool remove)
        {
            if (!IsAdministrator())
            {
                Console.WriteLine("[!] Administrator privileges required to edit hosts.");
                RelaunchElevated();
                return false;
            }

            try
            {
                if (File.Exists(HOSTS_PATH))
                {
                    FileAttributes attr = File.GetAttributes(HOSTS_PATH);
                    if ((attr & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                    {
                        File.SetAttributes(HOSTS_PATH, attr & ~FileAttributes.ReadOnly);
                    }
                }

                string content = File.Exists(HOSTS_PATH) ? File.ReadAllText(HOSTS_PATH) : "";
                string[] lines = content.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
                List<string> filtered = new List<string>();
                bool skipBlock = false;

                foreach (string line in lines)
                {
                    if (line.Contains("# --- ELX11 PES 2011 REDIRECTS ---"))
                    {
                        skipBlock = true;
                        continue;
                    }
                    if (line.Contains("# --- END ELX11 REDIRECTS ---"))
                    {
                        skipBlock = false;
                        continue;
                    }
                    if (!skipBlock)
                    {
                        bool matchesDomain = false;
                        foreach (string d in REDIRECT_DOMAINS)
                        {
                            if (line.Contains(d))
                            {
                                matchesDomain = true;
                                break;
                            }
                        }
                        if (!matchesDomain)
                        {
                            filtered.Add(line);
                        }
                    }
                }

                while (filtered.Count > 0 && string.IsNullOrWhiteSpace(filtered[filtered.Count - 1]))
                {
                    filtered.RemoveAt(filtered.Count - 1);
                }

                if (!remove)
                {
                    filtered.Add("");
                    filtered.Add("# --- ELX11 PES 2011 REDIRECTS ---");
                    foreach (string d in REDIRECT_DOMAINS)
                    {
                        filtered.Add(SERVER_IP + " " + d);
                    }
                    filtered.Add("# --- END ELX11 REDIRECTS ---");
                    filtered.Add("");
                }

                File.WriteAllText(HOSTS_PATH, string.Join("\r\n", filtered.ToArray()) + "\r\n", Encoding.UTF8);
                FlushDns();

                string verify = File.ReadAllText(HOSTS_PATH);
                if (!remove)
                    return verify.Contains("pes11pcgate-e.winning-eleven.net") && verify.Contains(SERVER_IP);
                else
                    return !verify.Contains("pes11pcgate-e.winning-eleven.net");
            }
            catch (Exception ex)
            {
                Console.WriteLine("[-] Failed updating hosts file: " + ex.Message);
                return false;
            }
        }

        static string FindPesExe()
        {
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\KONAMI\PES2011"))
                {
                    if (key != null)
                    {
                        object val = key.GetValue("installdir");
                        if (val != null)
                        {
                            string regExe = Path.Combine(val.ToString(), "pes2011.exe");
                            if (File.Exists(regExe)) return regExe;
                        }
                    }
                }
            }
            catch { }

            string[] candidates = new string[]
            {
                @"D:\PES\11\pes2011.exe",
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "pes2011.exe"),
                @"C:\Program Files (x86)\KONAMI\Pro Evolution Soccer 2011\pes2011.exe",
                @"C:\Program Files\KONAMI\Pro Evolution Soccer 2011\pes2011.exe"
            };

            foreach (string c in candidates)
            {
                if (File.Exists(c)) return c;
            }
            return null;
        }

        static void LaunchGame()
        {
            Console.WriteLine("\n----------------------------------------------------------------------");
            Console.WriteLine("  [1] LAUNCH PRO EVOLUTION SOCCER 2011");
            Console.WriteLine("----------------------------------------------------------------------");

            string exe = FindPesExe();
            if (string.IsNullOrEmpty(exe) || !File.Exists(exe))
            {
                Console.WriteLine("[-] Could not automatically locate pes2011.exe.");
                Console.Write("    Please enter the full path to pes2011.exe: ");
                string custom = Console.ReadLine();
                if (!string.IsNullOrEmpty(custom))
                {
                    custom = custom.Trim('"', ' ');
                    if (File.Exists(custom)) exe = custom;
                }
                if (string.IsNullOrEmpty(exe) || !File.Exists(exe))
                {
                    Console.WriteLine("[-] File not found. Game launch aborted.");
                    return;
                }
            }

            if (!IsConnected())
            {
                Console.WriteLine("\n[!] Note: ELX11 server redirects are currently DISCONNECTED.");
                Console.Write("    Connect to ELX11 servers before launching? [Y/n]: ");
                string ans = Console.ReadLine();
                if (string.IsNullOrEmpty(ans) || ans.Trim().ToLower().StartsWith("y"))
                {
                    ConnectToServers();
                }
            }

            Console.WriteLine("\n[*] Starting PES 2011: " + exe);
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = exe;
                psi.WorkingDirectory = Path.GetDirectoryName(exe);
                Process.Start(psi);
                Console.WriteLine("[+] SUCCESS: Game process launched!");
                Console.WriteLine("    In the PES 2011 main menu, select 'Online' to enter the pitch.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("[-] Failed to start game: " + ex.Message);
            }
        }

        static void ConnectToServers()
        {
            Console.WriteLine("\n----------------------------------------------------------------------");
            Console.WriteLine("  [2] CONNECTING TO ELX11 SERVERS");
            Console.WriteLine("----------------------------------------------------------------------");
            Console.WriteLine("[*] Updating Windows hosts file...");

            if (UpdateHosts(false))
            {
                Console.WriteLine("[+] SUCCESS! Configured Windows hosts redirects:");
                foreach (string d in REDIRECT_DOMAINS)
                {
                    Console.WriteLine("    -> " + SERVER_IP + " " + d);
                }
                Console.WriteLine("[+] DNS Cache flushed successfully.");
                Console.WriteLine("\n======================================================================");
                Console.WriteLine("  [CONNECTED] Your PC is now routed to ELX11 Servers!");
                Console.WriteLine("  Launch pes2011.exe, head to Online mode and enjoy the pitch!");
                Console.WriteLine("======================================================================");
            }
            else
            {
                Console.WriteLine("[-] FAILED to update hosts file.");
                Console.WriteLine("    Please ensure the launcher is run as Administrator or check antivirus.");
            }
        }

        static void DisconnectFromServers()
        {
            Console.WriteLine("\n----------------------------------------------------------------------");
            Console.WriteLine("  [3] DISCONNECTING FROM ELX11 SERVERS (REVERT HOSTS)");
            Console.WriteLine("----------------------------------------------------------------------");
            Console.WriteLine("[*] Removing ELX11 redirects from Windows hosts file...");

            if (UpdateHosts(true))
            {
                Console.WriteLine("[+] SUCCESS! ELX11 redirects have been removed.");
                Console.WriteLine("[+] DNS Cache flushed successfully.");
                Console.WriteLine("\n======================================================================");
                Console.WriteLine("  [RESTORED] Hosts file reverted to original state.");
                Console.WriteLine("======================================================================");
            }
            else
            {
                Console.WriteLine("[-] Failed to revert hosts file.");
            }
        }

        static bool TestTcpPort(string host, int port, out double latencyMs)
        {
            latencyMs = 0;
            Stopwatch sw = Stopwatch.StartNew();
            try
            {
                using (TcpClient client = new TcpClient())
                {
                    IAsyncResult ar = client.BeginConnect(host, port, null, null);
                    bool success = ar.AsyncWaitHandle.WaitOne(2000);
                    if (!success) return false;
                    client.EndConnect(ar);
                    sw.Stop();
                    latencyMs = sw.Elapsed.TotalMilliseconds;
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        static void CheckConnection()
        {
            Console.WriteLine("\n----------------------------------------------------------------------");
            Console.WriteLine("  [4] CHECKING CONNECTION TO ELX11 SERVERS & APIS");
            Console.WriteLine("----------------------------------------------------------------------");

            bool configured = IsConnected();
            if (configured)
                Console.WriteLine("  [*] Local Hosts Routing:   [OK] Configured -> Pointing to ELX11");
            else
                Console.WriteLine("  [!] Local Hosts Routing:   [NOT CONFIGURED] Select option [2] to connect.");

            Console.WriteLine("\n  [*] Probing ELX11 Game Services (Latency Test):");

            var services = new Tuple<string, int>[]
            {
                Tuple.Create("Gate Master Server", 4010),
                Tuple.Create("ELX11 Main Lobby", 28022),
                Tuple.Create("Friend Lobby (FD)", 28021),
                Tuple.Create("Account Auth Server", 28024),
                Tuple.Create("Menu Service", 28023),
                Tuple.Create("VdpChat Service", 28025),
                Tuple.Create("Web Portal (HTTP)", 80)
            };

            bool allOk = true;
            foreach (var s in services)
            {
                double lat;
                if (TestTcpPort(SERVER_IP, s.Item2, out lat))
                {
                    Console.WriteLine(string.Format("    [ONLINE]  {0,-22} (Port {1,-5}) - {2:0.0} ms", s.Item1, s.Item2, lat));
                }
                else
                {
                    allOk = false;
                    Console.WriteLine(string.Format("    [OFFLINE] {0,-22} (Port {1,-5}) - FAILED TO CONNECT", s.Item1, s.Item2));
                }
            }

            Console.WriteLine("\n  [*] Fetching Live Server Telemetry from API:");
            try
            {
                using (WebClient wc = new WebClient())
                {
                    wc.Headers["User-Agent"] = "ELX11-Launcher/2.0-CS";
                    string json = wc.DownloadString("http://" + SERVER_IP + "/api/bot/status?key=" + API_KEY);
                    Console.WriteLine("    -> Live API Response: OK");
                    // Simple parse
                    ExtractField(json, "status", "Server Status");
                    ExtractField(json, "online_players", "Players Online");
                    ExtractField(json, "live_matches", "Active Matches");
                    ExtractField(json, "total_players", "Registered Players");
                    ExtractField(json, "total_matches", "Total Matches");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("    [!] Could not query live API: " + ex.Message);
            }

            Console.WriteLine("\n----------------------------------------------------------------------");
            if (allOk && configured)
                Console.WriteLine("  [STATUS] All ELX11 services are ONLINE and your PC is READY!");
            else if (allOk && !configured)
                Console.WriteLine("  [STATUS] Server is ONLINE! Please select option [2] to route game traffic.");
            else
                Console.WriteLine("  [STATUS] Some services could not be reached. Check firewall/internet.");
            Console.WriteLine("----------------------------------------------------------------------");
        }

        static void ExtractField(string json, string key, string label)
        {
            try
            {
                int idx = json.IndexOf("\"" + key + "\"");
                if (idx >= 0)
                {
                    int colon = json.IndexOf(':', idx);
                    int comma = json.IndexOfAny(new char[] { ',', '}' }, colon);
                    if (colon > 0 && comma > colon)
                    {
                        string val = json.Substring(colon + 1, comma - colon - 1).Trim().Trim('"');
                        Console.WriteLine(string.Format("    -> {0,-21}: {1}", label, val));
                    }
                }
            }
            catch { }
        }

        static void DetectHardware(out string os, out string cpu, out int cores, out string gpu, out int ramGb, out string res)
        {
            os = "Windows 10 64 bit";
            try
            {
                using (RegistryKey k = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion"))
                {
                    if (k != null)
                    {
                        object pName = k.GetValue("ProductName");
                        if (pName != null)
                        {
                            string osName = pName.ToString();
                            // If build >= 22000, it's Windows 11 even if ProductName says Windows 10
                            object buildObj = k.GetValue("CurrentBuildNumber") ?? k.GetValue("CurrentBuild");
                            if (buildObj != null)
                            {
                                int bNum;
                                if (int.TryParse(buildObj.ToString(), out bNum) && bNum >= 22000)
                                {
                                    osName = osName.Replace("Windows 10", "Windows 11");
                                    if (!osName.Contains("Windows 11")) osName = "Windows 11 " + osName;
                                }
                            }
                            os = osName + (Environment.Is64BitOperatingSystem ? " 64-bit" : " 32-bit");
                        }
                    }
                }
            }
            catch { }

            cores = Environment.ProcessorCount;
            cpu = Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER") ?? (cores + "-Core CPU");
            try
            {
                using (RegistryKey k = Registry.LocalMachine.OpenSubKey(@"HARDWARE\DESCRIPTION\System\CentralProcessor\0"))
                {
                    if (k != null)
                    {
                        object pName = k.GetValue("ProcessorNameString");
                        if (pName != null) cpu = pName.ToString().Trim();
                    }
                }
            }
            catch { }

            gpu = "Direct3D 9.0c GPU";
            try
            {
                using (RegistryKey k = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"))
                {
                    if (k != null)
                    {
                        object desc = k.GetValue("DriverDesc");
                        if (desc != null) gpu = desc.ToString().Trim();
                    }
                }
            }
            catch { }

            ramGb = 8;
            try
            {
                MEMORYSTATUSEX memStatus = new MEMORYSTATUSEX();
                if (GlobalMemoryStatusEx(memStatus))
                {
                    ramGb = (int)Math.Round((double)memStatus.ullTotalPhys / (1024 * 1024 * 1024));
                }
            }
            catch { }

            res = "1920 x 1080 (1080p FHD)";
            try
            {
                int w = GetSystemMetrics(0);
                int h = GetSystemMetrics(1);
                if (w > 0 && h > 0)
                {
                    res = w + " x " + h;
                }
            }
            catch { }
        }

        static void HardwareSurveyMenu()
        {
            Console.WriteLine("\n----------------------------------------------------------------------");
            Console.WriteLine("  [5] ELX11 COMMUNITY HARDWARE & SYSTEMS SURVEY (VALVE/STEAM STYLE)");
            Console.WriteLine("----------------------------------------------------------------------");
            Console.WriteLine("[*] Probing local Direct3D hardware and display specifications...");

            string os, cpu, gpu, res;
            int cores, ramGb;
            DetectHardware(out os, out cpu, out cores, out gpu, out ramGb, out res);

            Console.WriteLine("\n  [+] Detected System Profile:");
            Console.WriteLine("      -> Operating System:    " + os);
            Console.WriteLine(string.Format("      -> Processor (CPU):     {0} ({1} Cores)", cpu, cores));
            Console.WriteLine("      -> Graphics Card (GPU): " + gpu);
            Console.WriteLine("      -> System Memory (RAM): " + ramGb + " GB");
            Console.WriteLine("      -> Primary Resolution:  " + res);
            Console.WriteLine("      -> Recommended Preset:  HIGH (60 FPS Target)");

            Console.WriteLine("\n  [*] Privacy Notice: Data is submitted completely anonymously.");
            Console.WriteLine("      No personal data, user accounts, or IP addresses are stored.");
            Console.WriteLine("      Official Survey URL: https://" + SERVER_DOMAIN + "/survey");

            Console.Write("\n  Participate in monthly community hardware survey? [Y/n]: ");
            string ans = Console.ReadLine();
            if (string.IsNullOrEmpty(ans) || ans.Trim().ToLower().StartsWith("y"))
            {
                Console.WriteLine("[*] Sending anonymous telemetry to ELX11 Survey Hub...");
                try
                {
                    string jsonPayload = string.Format(
                        "{{\"os\":\"{0}\",\"cpu\":\"{1}\",\"cpu_cores\":{2},\"gpu\":\"{3}\",\"ram_gb\":{4},\"resolution\":\"{5}\",\"pcspec_preset\":\"HIGH\",\"source\":\"LAUNCHER\"}}",
                        os.Replace("\"", "\\\""),
                        cpu.Replace("\"", "\\\""),
                        cores,
                        gpu.Replace("\"", "\\\""),
                        ramGb,
                        res.Replace("\"", "\\\"")
                    );

                    HttpWebRequest req = (HttpWebRequest)WebRequest.Create("http://" + SERVER_IP + "/api/survey/report");
                    req.Method = "POST";
                    req.ContentType = "application/json";
                    req.UserAgent = "ELX11-Launcher/2.0-CS";
                    req.Timeout = 6000;

                    byte[] postBytes = Encoding.UTF8.GetBytes(jsonPayload);
                    req.ContentLength = postBytes.Length;
                    using (Stream s = req.GetRequestStream())
                    {
                        s.Write(postBytes, 0, postBytes.Length);
                    }

                    using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
                    using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
                    {
                        string respText = sr.ReadToEnd();
                        if (respText.Contains("\"success\":true") || respText.Contains("true"))
                        {
                            Console.WriteLine("[+] SUCCESS! Your system specs were recorded in the community survey.");
                            Console.WriteLine("    View live community stats at: https://" + SERVER_DOMAIN + "/survey");
                        }
                        else
                        {
                            Console.WriteLine("[-] Server replied: " + respText);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[-] Could not submit survey: " + ex.Message);
                }
            }
            else
            {
                Console.WriteLine("[*] Survey submission skipped.");
            }
        }

        static void OpenWebPortal()
        {
            Console.WriteLine("\n----------------------------------------------------------------------");
            Console.WriteLine("  [6] OPEN ELX11 WEB HUB & SURVEY IN BROWSER");
            Console.WriteLine("----------------------------------------------------------------------");
            Console.WriteLine("[*] Opening https://" + SERVER_DOMAIN + " in your default browser...");
            try
            {
                Process.Start("https://" + SERVER_DOMAIN);
                Console.WriteLine("[+] Web page opened successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("[-] Could not open browser: " + ex.Message);
            }
        }

        static void Main(string[] args)
        {
            if (!IsAdministrator())
            {
                RelaunchElevated();
            }

            Console.Title = "ELX11 - Evolution Legacy X 11 Community Launcher";

            string detectedGame = FindPesExe();
            string gameLabel = !string.IsNullOrEmpty(detectedGame) ? " (Found: " + detectedGame + ")" : "";

            while (true)
            {
                try { Console.Clear(); } catch { }

                Console.WriteLine(@"
======================================================================
  ______ _     __   __ _ _ 
 |  ____| |    \ \ / // / |
 | |__  | |     \ V / / /| |
 |  __| | |      > < / / | |
 | |____| |____ / . \_  _| |
 |______|______/_/ \_\ _| |_|
   EVOLUTION LEGACY X 11 - PES 2011 COMMUNITY LAUNCHER
   " + VERSION + @" | Server: " + SERVER_IP + @"
   Web: https://" + SERVER_DOMAIN + @" | Survey: /survey
======================================================================");

                bool isConn = IsConnected();
                string statusText = isConn ? "[CONNECTED] -> Pointing to ELX11 (" + SERVER_IP + ")" : "[DISCONNECTED] -> Official / Default Hosts";
                string adminFlag = IsAdministrator() ? "Admin Mode" : "Standard User";

                Console.WriteLine("  Current Status: " + statusText);
                Console.WriteLine("  Privilege:      [" + adminFlag + "]");
                Console.WriteLine("======================================================================");
                Console.WriteLine("  Main Menu:");
                Console.WriteLine("  [1] Launch PES 2011" + gameLabel);
                Console.WriteLine("  [2] Connect to ELX11 Servers (Add Hosts Redirects)");
                Console.WriteLine("  [3] Disconnect from ELX11 Servers (Restore Original Hosts)");
                Console.WriteLine("  [4] Check Connection & Server Diagnostics (Ping & Port Tests)");
                Console.WriteLine("  [5] Hardware Survey & Specs Diagnostic (Steam-Style Telemetry)");
                Console.WriteLine("  [6] Open ELX11 Web Portal (Rankings, Live Matches & Survey)");
                Console.WriteLine("  [7] Quit");
                Console.WriteLine("======================================================================");
                Console.Write("\n  Select an option [1-7]: ");

                string choice = Console.ReadLine();
                if (choice == null) break;
                choice = choice.Trim();

                if (choice == "1")
                {
                    LaunchGame();
                    Console.WriteLine("\nPress Enter to return to menu...");
                    Console.ReadLine();
                }
                else if (choice == "2")
                {
                    ConnectToServers();
                    Console.WriteLine("\nPress Enter to return to menu...");
                    Console.ReadLine();
                }
                else if (choice == "3")
                {
                    DisconnectFromServers();
                    Console.WriteLine("\nPress Enter to return to menu...");
                    Console.ReadLine();
                }
                else if (choice == "4")
                {
                    CheckConnection();
                    Console.WriteLine("\nPress Enter to return to menu...");
                    Console.ReadLine();
                }
                else if (choice == "5")
                {
                    HardwareSurveyMenu();
                    Console.WriteLine("\nPress Enter to return to menu...");
                    Console.ReadLine();
                }
                else if (choice == "6")
                {
                    OpenWebPortal();
                    Console.WriteLine("\nPress Enter to return to menu...");
                    Console.ReadLine();
                }
                else if (choice == "7")
                {
                    Console.WriteLine("\nThank you for using ELX11 Launcher. See you on the pitch!");
                    System.Threading.Thread.Sleep(800);
                    break;
                }
                else
                {
                    Console.WriteLine("[!] Invalid option. Please select 1 through 7.");
                    System.Threading.Thread.Sleep(800);
                }
            }
        }
    }
}
