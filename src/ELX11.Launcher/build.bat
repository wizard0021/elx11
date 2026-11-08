@echo off
echo Compiling ELX11 Launcher with Microsoft Visual C# Compiler...
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe /target:exe /optimize+ /platform:anycpu /win32manifest:app.manifest /out:ELX11_Launcher_v2.exe ELX11_Launcher.cs
if %errorlevel% equ 0 (
    echo [SUCCESS] ELX11_Launcher_v2.exe built successfully.
) else (
    echo [ERROR] Build failed.
)
pause
