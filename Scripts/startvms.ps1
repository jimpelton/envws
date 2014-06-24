
[string[]] $vmPaths = @('G:\Virtual Machines\Arch64-envws-deploy-server\Arch64-envws-deploy-server.vmx',
                        'G:\Virtual Machines\envws-orch\envws-orch.vmx',
                        'G:\Virtual Machines\envws-track-1\envws-track-1.vmx'
                        )

$vmrun = 'C:\Program Files (x86)\VMware\VMware Workstation\vmrun.exe'

foreach ($p in $vmPaths) {
    $command = "$vmrun -T ws start $p nogui"
    Write-Output "Starting: $command"
    & "$vmrun" -T ws start "$p" nogui
}

Write-Output "Exiting..."

