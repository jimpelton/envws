param(
    [Parameter(Mandatory=$true)]
    [string]$ProcessName = "",
    [Parameter(Mandatory=$true)]
    [string]$ComputerName = ""
    )


$name = "vmuser"
$secret = "01000000d08c9ddf0115d1118c7a00c04fc297eb010000006ef6313c75626d4e86554e214cd321710000000002000000000010660000000100002000000088d8f1727b408b4bae75b46557f03ddec88ce60ac2fd27c972598a4bec28cc33000000000e8000000002000020000000764f2594004fd2fda0a1bf82fa85a0abc5a0ecb2bd297f015b5601f6d94510a0100000002ddbeb08f0876727a482a7522300df5b4000000026230871059237021cdbb8ce5a3e763e54c92fdf485834b42206c74506491e428536666bb3d16bae73f9eaf05d6e50af1a59e8be75d5250894220a3063d46558"
$pw =  $secret | convertto-securestring
$cred = new-object -typename System.Management.Automation.PSCredential `
                    -argumentlist $name, $pw
$sc = { 
        param($p)
        $proc = Get-Process $p 
        if ($proc) { Stop-Process $proc }
        else { Write-Error "$ProcessName not found"}
        }
    
Invoke-Command -ComputerName $ComputerName `
                -Authentication Default -Credential $cred `
                -ScriptBlock $sc

Write-Output "Done..."

