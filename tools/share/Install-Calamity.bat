@echo off
rem Runs the installer next to this file without changing the system's PowerShell execution policy.
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0install.ps1"
