envws
=====

Web service and related tools for distributed Envision.

##To build:

* You must right-click Solution --> Enable NuGet package restore.
* You also must set Powershell's execution policy to Unrestricted so that the pre-build events (which are implemented as powershell scripts) can be executed by Visual Studio.
 1. Start powershell (32-bit) as administrator
 2. Execute: `Set-ExecutionPolicy Unrestricted`
 3. If Windows says this is a bad idea, just agree, and say [Y]es.
 4. Do the same for the 64-bit version of powershell.
  
## Directory structure:
* csharp: all of the code for envws
  * ConsoleClient: an experimental client for envws, mostly useful as an example for other clients and for testing.
  * Envws: visual studio project folder.
    * ClientLib: Project folder for a library that clients should link to. The code in here has been transfered to EnvwsLib and this project should no longer be used. This project has been removed from the Envws solution.
    * EnvwsLib: dependences shared between EnvwsOrchestrator, EnvwsTracker. Also, this library should be linked to by any client that you write.
    * EnvwsOrchestrator: The orchestration program. Exposes two services: one for envws clients, and another for the trackers.
    * EnvwsTracker: The process tracker program that tracks processes until completion. 
* doc: Some useful (hopefully) documentation for envws.
* Scripts: Some useful (hopefully) powershell scripts for setting up and building the envws project.
