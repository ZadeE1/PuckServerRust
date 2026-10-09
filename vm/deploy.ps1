# Deploy the Linux Puck bundle into the Lima VM.
# Usage: .\vm\deploy.ps1 [-Instance puck] [-SkipBuild]
# Requires: Docker Desktop (for docker/build.sh), limactl + QEMU on PATH.
# ponytail: build on host, limactl copy in, systemd to run. No extra tooling.
param(
  [string]$Instance = "puck",
  [switch]$SkipBuild
)
$ErrorActionPreference = "Stop"
$RepoRoot = Split-Path -Parent $PSScriptRoot
Set-Location $RepoRoot
$env:PATH = "C:\Program Files\qemu;$env:LOCALAPPDATA\Microsoft\WinGet\Packages\Lima.Lima_Microsoft.Winget.Source_8wekyb3d8bbwe\bin;C:\Program Files\Git\usr\bin;" + $env:PATH

if (-not $SkipBuild) {
  # Builds ./puck-linux-server/server via Docker (linux/amd64, Steam tool 3481440).
  bash docker/build.sh ./puck-linux-server
}

$GuestUser = (limactl shell $Instance -- whoami).Trim()
$GuestHome = (limactl shell $Instance -- printenv HOME).Trim()
Write-Host "Guest user: $GuestUser home: $GuestHome"

# limactl copy (scp) is broken on Windows hosts; stream via tar instead.
# Must run under Git Bash: raw byte pipe + MSYS2_ARG_CONV_EXCL so guest
# absolute paths survive MSYS2's arg mangling.
$bash = "C:\Program Files\Git\bin\bash.exe"
limactl shell $Instance -- mkdir -p "${GuestHome}/puck-server"
& $bash -c "export MSYS2_ARG_CONV_EXCL='*'; tar -czf - -C puck-linux-server server | limactl shell ${Instance} -- tar -xzf - -C ${GuestHome}/puck-server"
if ($LASTEXITCODE -ne 0) { throw "bundle transfer failed" }
& $bash -c "export MSYS2_ARG_CONV_EXCL='*'; limactl shell ${Instance} -- tee ${GuestHome}/puck-server/server/server_config.json < RealServerForBasis/server_config.json" 2>&1 | Out-Null

limactl shell $Instance -- sh -c "chmod +x ~/puck-server/server/Puck && sudo systemctl enable --now puck@${GuestUser} && sleep 2 && systemctl is-active puck@${GuestUser}"
Write-Host "Waiting ~45s for boot, then checking log..."
Start-Sleep -Seconds 45
limactl shell $Instance -- sh -c "grep 'ready to accept clients' ~/puck-server/server/Player.log || journalctl -u puck@${GuestUser} --no-pager | tail -30"
Write-Host "Connect game client to 127.0.0.1:25565 (forwarded to VM:25565)."
