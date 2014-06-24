Invoke-Command `
	-ComputerName envws-orch `
	-ScriptBlock { `
	(New-Object Net.WebClient).DownloadFile("http://envws-deploy/Orchestrator/Orchestrator.zip", "C:\Orchestrator.zip") `
	} `
	-Credential $cred

Invoke-Command -ComputerName envws-orch -ScriptBlock { start-service orchestratorservices } -Credential $cred

Invoke-Command -ComputerName envws-orch -ScriptBlock { get-service orchestratorservices } -Credential $cred