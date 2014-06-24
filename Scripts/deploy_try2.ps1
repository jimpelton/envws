param(
    [string]$cname = ""
    )

$cred = Get-Credential

Invoke-Command -ComputerName "$cname" `
               -ScriptBlock { `
                 (New-Object Net.WebClient).DownloadFile("http://envws-deploy/Orchestrator/Orchestrator.zip", "C:\Orchestrator.zip") `
               } `
               -Authentication Default `
               -Credential $cred
               