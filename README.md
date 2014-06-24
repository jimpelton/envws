envws
=====

Web service and related tools for distributed Envision.

##To build:

* You must right-click Solution --> Enable NuGet package restore.
* You also must set Powershell's execution policy to Unrestricted so that the prebuild events (which are implemented as powershell scripts) can be executed by Visual Studio.
 1. Start powershell (32-bit) as administrator
 2. Execute: `Set-ExecutionPolicy Unrestricted`
 3. If Windows says this is a bad idea, just agree, and say [Y]es.
 4. Do the same for the 64-bit version of powershell.
  
