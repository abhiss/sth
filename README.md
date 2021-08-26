# STHV - Survive the Hunt FiveM
A FiveM gamemode based on Surive the Hunt by FailRace.\
To play on an official server join the [Survive the Hunt Discord server](discord.gg/HygTs4s).

This is the repository for the STHV resource. STHV was created for the STH Discord server to allow us to easily play Survive the Hunt without cheaters and the typical GTA Online disruptions. Currently, we are in the process of modularizing the resource to allow for different "gamemodes" which will be variations of the classic STH mode. The name STHV might be a misnomer soon as we add new gamemodes that deviate from Survive the Hunt.

Some directories have readme.md files that describe that they are used for. If you have questions about the repo, ask in the Discord server linked above.


## Building
The resource projects target .NET Standard 2.0 and .NET Framework for the server and client resource respectively. You need to have .NET SDKs to build source. These can be installed [manually](https://dotnet.microsoft.com/download/visual-studio-sdks) or along with [Visual Stuio](https://visualstudio.microsoft.com/). The resulting files will in the [dist](/sthv/dist) directory, which can be started as a resource.

### Windows - Visual Studio: 

Open [sthv.sln](/sthv/) and build.

### Windows - Dotnet CLI:
```
git clone https://github.com/abhiss/sth.git
dotnet build -c Release ./sth/sthv/sthv.sln
```

### Linux:
Building on Linux requires [Mono](mono-project.com/) because FiveM client resources target .NET Framework, which isn't available in the .NET SDK that ships for Linux. Refer to the [build script](/sthv/etc/prebuild.sh), which runs on Debian 10 and Ubuntu-Latest in automated builds.
