if(Get-IsRemote){
        Invoke-FromTask @"
`$serviceManager = New-Object -ComObject Microsoft.Update.ServiceManager -Strict
`$serviceManager.ClientApplicationID = "Boxstarter"
`$serviceManager.RemoveService("7971f918-a847-4430-9279-4a52d1efe18d")
"@
}else{
    $serviceManager = New-Object -ComObject Microsoft.Update.ServiceManager -Strict
    $serviceManager.ClientApplicationID = "Boxstarter"
    $serviceManager.RemoveService("7971f918-a847-4430-9279-4a52d1efe18d")
}