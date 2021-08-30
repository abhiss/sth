# STHV - Survive the Hunt FiveM
A FiveM gamemode based on Surive the Hunt by FailRace.\
To play on an official server join the [Survive the Hunt Discord server](discord.gg/HygTs4s).

This is the repository for the STHV resource. STHV was created for the STH Discord server to allow us to easily play Survive the Hunt without cheaters and the typical GTA Online disruptions. Currently, we are in the process of modularizing the resource to allow for different "gamemodes" which will be variations of the classic STH mode. The name STHV might be a misnomer soon as we add new gamemodes that deviate from Survive the Hunt.

Some directories have readme.md files that describe that they are used for. If you have questions about the repo, ask in the Discord server linked above.

## Installing
### txAdmin
[txAdmin](https://github.com/tabarra/txAdmin) is an easy to use web panel for deploying and managing FiveM servers. STHV has first class support for deploying on txAdmin. Use the txAdmin docs to set up your server. Choose the `Remote URL Template` deployment type and copy and paste the following URL: `https://raw.githubusercontent.com/abhiss/sth/master/sthv/etc/fxadmin_recipe.yaml`

### manual
Refer to [fxadmin_recipe.yaml](sthv/etc/fxadmin_recipe.yaml). Download STHV, the main resource, from [releases](https://github.com/abhiss/sth/releases/tag/latest). There are also some dependencies on a few default resources and external resources located in [ext_resources](ext_resources). Note that [vSync](ext_resources) and [ipl](ext_resources) are entirely optional and the game is only somewhat broken without them. A reference [server.cfg](sthv/etc/server.cfg) is also included. 

### Docker 
todo - A custom docker image for STHV would be nice. Open to PRs. :)

## Building
The resource projects target .NET Standard 2.0 and .NET Framework for the server and client resource respectively. You need to have .NET SDKs to build source. These can be installed [manually](https://dotnet.microsoft.com/download/visual-studio-sdks) or along with [Visual Stuio](https://visualstudio.microsoft.com/). The resulting files will in the [dist](/sthv/dist) directory, which can be started as a resource. The `sthvServer.csproj` can optionally run a script after building if the environment variable PostBuildScript is set the path of a powershell script.

### Windows - Visual Studio: 

Open [sthv.sln](/sthv/) and build.

### Windows - Dotnet CLI:
```
$env:PostBuildScript="C:/path/to/script.ps1"
git clone https://github.com/abhiss/sth.git
dotnet build -c Release ./sth/sthv/sthv.sln
```

### Linux:
Building on Linux requires [Mono](mono-project.com/) because FiveM client resources target .NET Framework, which isn't available in the .NET SDK that ships for Linux. Refer to the [build script](/sthv/etc/prebuild.sh), which runs on Debian 10 and Ubuntu-Latest in automated builds.
