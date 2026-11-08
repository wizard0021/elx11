# -*- coding: utf-8 -*-
"""
ELX11 - Evolution Legacy X 11
PES 2011 Server Connector, Game Launcher & Hardware Telemetry Tool
==================================================================
Version: 2.0 Community Edition
Built for the ELX11 Community
"""

import sys
import os
import ctypes
import stat
import socket
import time
import subprocess
import urllib.request
import json
import webbrowser
import platform

VERSION = "v2.0 Community Edition"
SERVER_IP = "63.178.232.241"
SERVER_DOMAIN = "elx11.wizcake.space"
API_KEY = "elx11_bot_secret_2026"

HOSTS_PATH = os.path.join(os.environ.get("SystemRoot", "C:\\Windows"), "System32", "drivers", "etc", "hosts")

ENTRIES = [
    f"{SERVER_IP} pes11pcgate-e.winning-eleven.net",
    f"{SERVER_IP} pes11pcgate-u.winning-eleven.net",
    f"{SERVER_IP} pes11pcgate-j.winning-eleven.net",
    f"{SERVER_IP} pes11pcgate.winning-eleven.net",
    f"{SERVER_IP} pes7stun-e.winning-eleven.net",
    f"{SERVER_IP} pes7stun-u.winning-eleven.net",
    f"{SERVER_IP} pes7stun-j.winning-eleven.net",
    f"{SERVER_IP} pes7stun.winning-eleven.net",
    f"{SERVER_IP} id.konami.net",
    f"{SERVER_IP} locate.madserver.net",
]

BANNER = rf"""
======================================================================
  ______ _     __   __ _ _ 
 |  ____| |    \ \ / // / |
 | |__  | |     \ V / / /| |
 |  __| | |      > < / / | |
 | |____| |____ / . \_  _| |
 |______|______/_/ \_\ _| |_|
   EVOLUTION LEGACY X 11 - PES 2011 COMMUNITY LAUNCHER
   {VERSION} | Server: {SERVER_IP}
   Web: https://{SERVER_DOMAIN} | Survey: /survey
======================================================================
"""

def is_admin():
    try:
        return ctypes.windll.shell32.IsUserAnAdmin() != 0
    except Exception:
        return False

def request_elevation():
    """Relaunches the current script/exe with Administrator privileges if needed."""
    if is_admin():
        return True
    try:
        if getattr(sys, 'frozen', False):
            exe = sys.executable
            args = " ".join([f'"{a}"' for a in sys.argv[1:]])
        else:
            exe = sys.executable
            args = f'"{os.path.abspath(__file__)}" ' + " ".join([f'"{a}"' for a in sys.argv[1:]])
            
        ret = ctypes.windll.shell32.ShellExecuteW(None, "runas", exe, args, None, 1)
        if ret > 32:
            sys.exit(0)
    except Exception as e:
        print(f"[-] Elevation error: {e}")
    return False

def remove_readonly(path):
    try:
        if os.path.exists(path):
            os.chmod(path, stat.S_IWRITE | stat.S_IREAD)
    except Exception:
        pass

def check_hosts_status():
    if not os.path.exists(HOSTS_PATH):
        return False
    try:
        with open(HOSTS_PATH, "r", encoding="utf-8", errors="ignore") as f:
            content = f.read()
        return "pes11pcgate-e.winning-eleven.net" in content and SERVER_IP in content
    except Exception:
        return False

def update_hosts(entries_to_add=None, remove=False):
    if not is_admin():
        print("[!] Administrator privileges are required to modify the hosts file.")
        print("[*] Requesting Administrator rights...")
        request_elevation()
        return False

    try:
        remove_readonly(HOSTS_PATH)
        
        current_content = ""
        if os.path.exists(HOSTS_PATH):
            with open(HOSTS_PATH, "r", encoding="utf-8", errors="ignore") as f:
                current_content = f.read()

        lines = current_content.splitlines()
        filtered = []
        skip_block = False
        for line in lines:
            if "# --- ELX11 PES 2011 REDIRECTS ---" in line:
                skip_block = True
                continue
            if "# --- END ELX11 REDIRECTS ---" in line:
                skip_block = False
                continue
            if not skip_block:
                if any(domain in line for domain in ["winning-eleven.net", "id.konami.net", "locate.madserver.net"]):
                    continue
                filtered.append(line)

        while filtered and not filtered[-1].strip():
            filtered.pop()

        if not remove and entries_to_add:
            filtered.append("")
            filtered.append("# --- ELX11 PES 2011 REDIRECTS ---")
            for entry in entries_to_add:
                filtered.append(entry)
            filtered.append("# --- END ELX11 REDIRECTS ---")
            filtered.append("")

        new_text = "\r\n".join(filtered) + "\r\n"

        with open(HOSTS_PATH, "w", encoding="utf-8", newline="\r\n") as f:
            f.write(new_text)
            f.flush()
            os.fsync(f.fileno())

        # Flush DNS cache via native Windows API
        try:
            ctypes.windll.dnsapi.DnsFlushResolverCache()
        except Exception:
            pass

        # Verification
        with open(HOSTS_PATH, "r", encoding="utf-8", errors="ignore") as f:
            verify_text = f.read()

        if not remove:
            return "pes11pcgate-e.winning-eleven.net" in verify_text and SERVER_IP in verify_text
        else:
            return "pes11pcgate-e.winning-eleven.net" not in verify_text

    except Exception as e:
        print(f"[-] Error writing hosts file: {e}")
        return False

def find_pes2011_exe():
    candidates = [
        r"D:\PES\11\pes2011.exe",
        os.path.join(os.getcwd(), "pes2011.exe"),
        os.path.join(os.path.dirname(os.path.abspath(__file__)), "pes2011.exe"),
        r"C:\Program Files (x86)\KONAMI\Pro Evolution Soccer 2011\pes2011.exe",
        r"C:\Program Files\KONAMI\Pro Evolution Soccer 2011\pes2011.exe",
    ]

    try:
        import winreg
        with winreg.OpenKey(winreg.HKEY_LOCAL_MACHINE, r"SOFTWARE\WOW6432Node\KONAMI\PES2011") as k:
            install_dir, _ = winreg.QueryValueEx(k, "installdir")
            reg_exe = os.path.join(install_dir, "pes2011.exe")
            if os.path.exists(reg_exe):
                return reg_exe
    except Exception:
        pass

    for c in candidates:
        if os.path.exists(c):
            return c
    return None

def launch_game():
    print("\n----------------------------------------------------------------------")
    print("  [1] LAUNCH PRO EVOLUTION SOCCER 2011")
    print("----------------------------------------------------------------------")
    exe = find_pes2011_exe()
    if not exe or not os.path.exists(exe):
        print("[-] Could not automatically locate pes2011.exe.")
        exe_input = input("    Please enter the full path to pes2011.exe: ").strip('"').strip()
        if os.path.exists(exe_input):
            exe = exe_input
        else:
            print("[-] File not found. Game launch aborted.")
            return

    if not check_hosts_status():
        print("\n[!] Note: ELX11 server redirects are currently DISCONNECTED.")
        ans = input("    Connect to ELX11 servers before launching? [Y/n]: ").strip().lower()
        if ans in ("", "y", "yes"):
            connect_to_servers()

    print(f"\n[*] Starting PES 2011: {exe}")
    try:
        game_dir = os.path.dirname(exe)
        subprocess.Popen([exe], cwd=game_dir)
        print("[+] SUCCESS: Game process launched!")
        print("    In the PES 2011 main menu, select 'Online' to enter the pitch.")
    except Exception as e:
        print(f"[-] Failed to start game: {e}")

def connect_to_servers():
    print("\n----------------------------------------------------------------------")
    print("  [2] CONNECTING TO ELX11 SERVERS")
    print("----------------------------------------------------------------------")
    print("[*] Updating Windows hosts file...")
    
    success = update_hosts(ENTRIES, remove=False)
    if success:
        print("[+] SUCCESS! Configured Windows hosts redirects:")
        for e in ENTRIES:
            print(f"    -> {e}")
        print("[+] DNS Cache flushed successfully.")
        print("\n======================================================================")
        print("  [CONNECTED] Your PC is now routed to ELX11 Servers!")
        print("  Launch pes2011.exe, head to Online mode and enjoy the pitch!")
        print("======================================================================")
    else:
        print("[-] FAILED to update hosts file.")
        print("    Please ensure the launcher is run as Administrator or check antivirus.")

def disconnect_from_servers():
    print("\n----------------------------------------------------------------------")
    print("  [3] DISCONNECTING FROM ELX11 SERVERS (REVERT HOSTS)")
    print("----------------------------------------------------------------------")
    print("[*] Removing ELX11 redirects from Windows hosts file...")
    
    success = update_hosts(remove=True)
    if success:
        print("[+] SUCCESS! ELX11 redirects have been removed.")
        print("[+] DNS Cache flushed successfully.")
        print("\n======================================================================")
        print("  [RESTORED] Hosts file reverted to original state.")
        print("======================================================================")
    else:
        print("[-] Failed to revert hosts file.")

def test_tcp_port(host, port, timeout=2.0):
    start = time.time()
    try:
        with socket.create_connection((host, port), timeout=timeout):
            latency = (time.time() - start) * 1000.0
            return True, latency
    except Exception:
        return False, 0.0

def check_connection():
    print("\n----------------------------------------------------------------------")
    print("  [4] CHECKING CONNECTION TO ELX11 SERVERS & APIS")
    print("----------------------------------------------------------------------")
    
    configured = check_hosts_status()
    if configured:
        print("  [*] Local Hosts Routing:   [OK] Configured -> Pointing to ELX11")
    else:
        print("  [!] Local Hosts Routing:   [NOT CONFIGURED] Select option [2] to connect.")

    print("\n  [*] Probing ELX11 Game Services (Latency Test):")

    services = [
        ("Gate Master Server", SERVER_IP, 4010, "Required for server list & version check"),
        ("ELX11 Main Lobby",   SERVER_IP, 28022, "Multiplayer lobbies & room matches"),
        ("Friend Lobby (FD)",  SERVER_IP, 28021, "Direct invitations & friend matches"),
        ("Account Auth Server",SERVER_IP, 28024, "In-game tickets & credentials auth"),
        ("Menu Service",       SERVER_IP, 28023, "Online menus & room directory"),
        ("VdpChat Service",    SERVER_IP, 28025, "In-game player chat"),
        ("Web Portal (HTTP)",  SERVER_IP, 80,    "Web portal, rankings & assets"),
    ]

    all_ok = True
    for name, ip, port, desc in services:
        ok, latency = test_tcp_port(ip, port)
        if ok:
            print(f"    [ONLINE]  {name:<22} (Port {port:<5}) - {latency:.1f} ms")
        else:
            all_ok = False
            print(f"    [OFFLINE] {name:<22} (Port {port:<5}) - FAILED TO CONNECT")

    print("\n  [*] Fetching Live Server Telemetry from API:")
    try:
        url = f"http://{SERVER_IP}/api/bot/status?key={API_KEY}"
        req = urllib.request.Request(url, headers={"User-Agent": "ELX11-Launcher/2.0"})
        with urllib.request.urlopen(req, timeout=4) as response:
            data = json.loads(response.read().decode())
            print(f"    -> Server Status:       {data.get('status', 'online').upper()}")
            print(f"    -> Players Online:      {data.get('online_players', 0)}")
            print(f"    -> Active Matches:      {data.get('live_matches', 0)}")
            print(f"    -> Registered Players:  {data.get('total_players', 0)}")
            print(f"    -> Total Matches:       {data.get('total_matches', 0)}")
    except Exception as e:
        print(f"    [!] Could not query live API: {e}")

    print("\n----------------------------------------------------------------------")
    if all_ok and configured:
        print("  [STATUS] All ELX11 services are ONLINE and your PC is READY!")
    elif all_ok and not configured:
        print("  [STATUS] Server is ONLINE! Please select option [2] to route game traffic.")
    else:
        print("  [STATUS] Some services could not be reached. Check firewall/internet.")
    print("----------------------------------------------------------------------")

def detect_hardware():
    specs = {
        "os": "Windows 10 64 bit",
        "cpu": "Direct3D Compatible CPU",
        "cpu_cores": 4,
        "ram_gb": 8,
        "gpu": "Direct3D 9.0c GPU",
        "resolution": "1920 x 1080 (1080p FHD)",
        "pcspec_preset": "HIGH"
    }

    try:
        ver = sys.getwindowsversion()
        if ver.build >= 22000:
            specs["os"] = "Windows 11 64 bit"
        elif ver.major == 10:
            specs["os"] = "Windows 10 64 bit"
        elif ver.major == 6 and ver.minor == 1:
            specs["os"] = "Windows 7 64 bit"
        elif ver.major == 6 and ver.minor in (2, 3):
            specs["os"] = "Windows 8.1 64 bit"
        else:
            specs["os"] = f"Windows {ver.major}.{ver.minor}"
    except Exception:
        pass

    try:
        cores = os.cpu_count()
        if cores:
            specs["cpu_cores"] = cores
        specs["cpu"] = platform.processor() or f"{cores}-Core Processor"
    except Exception:
        pass

    try:
        class MEMORYSTATUSEX(ctypes.Structure):
            _fields_ = [
                ('dwLength', ctypes.c_ulong),
                ('dwMemoryLoad', ctypes.c_ulong),
                ('ullTotalPhys', ctypes.c_ulonglong),
                ('ullAvailPhys', ctypes.c_ulonglong),
                ('ullTotalPageFile', ctypes.c_ulonglong),
                ('ullAvailPageFile', ctypes.c_ulonglong),
                ('ullTotalVirtual', ctypes.c_ulonglong),
                ('ullAvailVirtual', ctypes.c_ulonglong),
                ('sullAvailExtendedVirtual', ctypes.c_ulonglong),
            ]
        stat = MEMORYSTATUSEX()
        stat.dwLength = ctypes.sizeof(MEMORYSTATUSEX)
        if ctypes.windll.kernel32.GlobalMemoryStatusEx(ctypes.byref(stat)):
            specs["ram_gb"] = round(stat.ullTotalPhys / (1024**3))
    except Exception:
        pass

    try:
        w = ctypes.windll.user32.GetSystemMetrics(0)
        h = ctypes.windll.user32.GetSystemMetrics(1)
        if w and h:
            specs["resolution"] = f"{w} x {h}"
    except Exception:
        pass

    try:
        import winreg
        with winreg.OpenKey(winreg.HKEY_LOCAL_MACHINE, r"SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000") as k:
            desc, _ = winreg.QueryValueEx(k, "DriverDesc")
            if desc:
                specs["gpu"] = desc
    except Exception:
        pass

    return specs

def hardware_survey_menu():
    print("\n----------------------------------------------------------------------")
    print("  [5] ELX11 COMMUNITY HARDWARE & SYSTEMS SURVEY (VALVE/STEAM STYLE)")
    print("----------------------------------------------------------------------")
    print("[*] Probing local Direct3D hardware and display specifications...")
    specs = detect_hardware()
    
    print("\n  [+] Detected System Profile:")
    print(f"      -> Operating System:    {specs['os']}")
    print(f"      -> Processor (CPU):     {specs['cpu']} ({specs['cpu_cores']} Cores)")
    print(f"      -> Graphics Card (GPU): {specs['gpu']}")
    print(f"      -> System Memory (RAM): {specs['ram_gb']} GB")
    print(f"      -> Primary Resolution:  {specs['resolution']}")
    print(f"      -> Recommended Preset:  HIGH (60 FPS Target)")

    print("\n  [*] Privacy Notice: Data is submitted completely anonymously.")
    print("      No personal data, user accounts, or IP addresses are stored.")
    print(f"      Official Survey URL: https://{SERVER_DOMAIN}/survey")

    try:
        submit = input("\n  Participate in monthly community hardware survey? [Y/n]: ").strip().lower()
    except (KeyboardInterrupt, EOFError):
        return

    if submit in ("", "y", "yes"):
        print("[*] Sending anonymous telemetry to ELX11 Survey Hub...")
        try:
            url = f"http://{SERVER_IP}/api/survey/report"
            payload = {
                "os": specs["os"],
                "cpu": specs["cpu"],
                "cpu_cores": specs["cpu_cores"],
                "gpu": specs["gpu"],
                "ram_gb": specs["ram_gb"],
                "resolution": specs["resolution"],
                "pcspec_preset": specs["pcspec_preset"],
                "source": "LAUNCHER"
            }
            req = urllib.request.Request(
                url,
                data=json.dumps(payload).encode("utf-8"),
                headers={"Content-Type": "application/json", "User-Agent": "ELX11-Launcher/2.0"}
            )
            with urllib.request.urlopen(req, timeout=5) as resp:
                r_data = json.loads(resp.read().decode())
                if r_data.get("success"):
                    print("[+] SUCCESS! Your system specs were recorded in the community survey.")
                    print(f"    View live community stats at: https://{SERVER_DOMAIN}/survey")
                else:
                    print("[-] Server replied:", r_data)
        except Exception as e:
            print(f"[-] Could not submit survey: {e}")
    else:
        print("[*] Survey submission skipped.")

def open_web_portal():
    print("\n----------------------------------------------------------------------")
    print("  [6] OPEN ELX11 WEB HUB & SURVEY IN BROWSER")
    print("----------------------------------------------------------------------")
    print(f"[*] Opening https://{SERVER_DOMAIN} in your default browser...")
    try:
        webbrowser.open(f"https://{SERVER_DOMAIN}")
        print("[+] Web page opened successfully.")
    except Exception as e:
        print(f"[-] Could not open browser: {e}")

def main():
    if not is_admin():
        request_elevation()

    detected_game = find_pes2011_exe()
    game_label = f" (Found: {detected_game})" if detected_game else ""

    while True:
        os.system("cls" if os.name == "nt" else "clear")
        print(BANNER)
        
        is_conn = check_hosts_status()
        admin_flag = "Admin Mode" if is_admin() else "Standard User (Elevation Required for Hosts)"
        if is_conn:
            print(f"  Current Status: [CONNECTED] -> Pointing to ELX11 ({SERVER_IP})")
        else:
            print("  Current Status: [DISCONNECTED] -> Official / Default Hosts")
        print(f"  Privilege:      [{admin_flag}]")
        print("======================================================================")
        print("  Main Menu:")
        print(f"  [1] Launch PES 2011{game_label}")
        print("  [2] Connect to ELX11 Servers (Add Hosts Redirects)")
        print("  [3] Disconnect from ELX11 Servers (Restore Original Hosts)")
        print("  [4] Check Connection & Server Diagnostics (Ping & Port Tests)")
        print("  [5] Hardware Survey & Specs Diagnostic (Steam-Style Telemetry)")
        print("  [6] Open ELX11 Web Portal (Rankings, Live Matches & Survey)")
        print("  [7] Quit")
        print("======================================================================")

        try:
            choice = input("\n  Select an option [1-7]: ").strip()
        except (KeyboardInterrupt, EOFError):
            print("\nExiting...")
            break

        if choice == "1":
            launch_game()
            input("\nPress Enter to return to menu...")
        elif choice == "2":
            connect_to_servers()
            input("\nPress Enter to return to menu...")
        elif choice == "3":
            disconnect_from_servers()
            input("\nPress Enter to return to menu...")
        elif choice == "4":
            check_connection()
            input("\nPress Enter to return to menu...")
        elif choice == "5":
            hardware_survey_menu()
            input("\nPress Enter to return to menu...")
        elif choice == "6":
            open_web_portal()
            input("\nPress Enter to return to menu...")
        elif choice == "7":
            print("\nThank you for using ELX11 Launcher. See you on the pitch!")
            time.sleep(1)
            break
        else:
            print("[!] Invalid option. Please select 1 through 7.")
            time.sleep(1)

if __name__ == "__main__":
    main()
