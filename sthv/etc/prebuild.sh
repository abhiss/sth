su
apt-get -y update
apt-get -y install curl

#https://docs.microsoft.com/en-us/dotnet/core/install/linux-debian#debian-10-
curl https://packages.microsoft.com/config/debian/10/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
 dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb

   apt-get update; \
   apt-get install -y apt-transport-https && \
   apt-get update && \
   apt-get install -y dotnet-sdk-5.0

   apt-get update; \
   apt-get install -y apt-transport-https && \
   apt-get update && \
   apt-get install -y aspnetcore-runtime-5.0
#end

#https://www.mono-project.com/download/stable/#download-lin-debian
 apt install -y apt-transport-https dirmngr gnupg ca-certificates
 apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF
echo "deb https://download.mono-project.com/repo/debian stable-buster main" | tee /etc/apt/sources.list.d/mono-official-stable.list
 apt update

 apt install -y mono-devel
#end

#https://stackoverflow.com/a/55070707/12387791
export FrameworkPathOverride=/usr/lib/mono/4.5/

