
sudo apt-get -y update
sudo apt-get -y install curl

#https://docs.microsoft.com/en-us/dotnet/core/install/linux-debian#debian-10-
wget https://packages.microsoft.com/config/debian/10/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb

sudo apt-get update; \
  sudo apt-get install -y apt-transport-https && \
  sudo apt-get update && \
  sudo apt-get install -y dotnet-sdk-5.0

sudo apt-get update; \
  sudo apt-get install -y apt-transport-https && \
  sudo apt-get update && \
  sudo apt-get install -y aspnetcore-runtime-5.0
#end

#https://www.mono-project.com/download/stable/#download-lin-debian
 sudo apt-get install -y apt-transport-https dirmngr gnupg ca-certificates
 sudo apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF
echo "deb https://download.mono-project.com/repo/debian stable-buster main" | sudo tee /etc/apt/sources.list.d/mono-official-stable.list
 sudo apt-get update

 sudo apt install -y mono-devel
#end

#https://stackoverflow.com/a/55070707/12387791
export FrameworkPathOverride=/usr/lib/mono/4.5/

dotnet build -c Release -nologo ./sthv/sthv.sln

ls -a ./sthv/dist

sudo apt-get install -y tree
tree .