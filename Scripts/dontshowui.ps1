# create reg. key for turning off the "Searching For a Solution" dialog box.

New-itemproperty -path hklm:\software\microsoft\windows\'windows error reporting' -name DontShowUI -propertytype DWORD -Value 1