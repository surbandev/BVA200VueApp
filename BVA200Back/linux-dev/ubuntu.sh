#!/bin/bash

#root check
if [[ $EUID -ne 0 ]]; then
   echo "This script must be run as root" 
   exit 1
fi

#set username for application to run certain parts without root
if [ $SUDO_USER ]; then 
    usr=$SUDO_USER; 
else 
    usr=`whoami`; 
fi

apt update && apt upgrade -y && apt install -y chromium-browser gconf2 wkhtmltopdf p11-kit apt-transport-https nodejs npm xdotool libusb-1.0-0 ca-certificates openssl libnss3-tools espeak

#setup Microsoft repos and install dotnet if not already installed
isDotNetInstalled=$(dpkg-query -W -f='${Status}' dotnet-runtime-6.0 2>/dev/null | grep -c "ok installed")
if [ "$isDotNetInstalled" -eq "0" ]; then
   wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
   dpkg -i packages-microsoft-prod.deb
   rm packages-microsoft-prod.deb
   apt update
   apt install -y dotnet-sdk-6.0 dotnet-runtime-6.0
fi

#setup UDEV rules
rm -f /etc/udev/rules.d/80-brightspec.rules
touch /etc/udev/rules.d/80-brightspec.rules 
echo 'SUBSYSTEM=="usb", ATTR{idVendor}=="04d8", ATTR{idProduct}=="f85d", MODE="0666"' > /etc/udev/rules.d/80-brightspec.rules 
udevadm control --reload 

#setup certificates for HTTPS
# Setup Firefox
su -c 'echo "{
    \"policies\": {
        \"Certificates\": {
            \"Install\": [
            	\"aspnetcore-localhost-https.crt\"
            ]
        }
    }
}" > policies.json' $usr

#clean existing certs as both root and non-root
dotnet dev-certs https --clean
su -c 'dotnet dev-certs https --clean' $usr

#su -c 'dotnet dev-certs https -ep localhost.crt' $usr
su -c 'dotnet dev-certs https -ep localhost.crt --format PEM' $usr

mv policies.json /usr/lib/firefox/distribution/

su -c 'mkdir -p ~/.mozilla/certificates' $usr
su -c 'cp localhost.crt ~/.mozilla/certificates/aspnetcore-localhost-https.crt' $usr
su -c 'rm -rf $HOME/.pki' $usr
su -c 'mkdir -p $HOME/.pki/nssdb' $usr
su -c 'certutil -d $HOME/.pki/nssdb -N' $usr

# Trust Edge/Chrome
su -c 'certutil -d sql:$HOME/.pki/nssdb -A -t "P,," -n localhost -i ./localhost.crt' $usr
su -c 'certutil -d sql:$HOME/.pki/nssdb -A -t "C,," -n localhost -i ./localhost.crt' $usr

# Trust dotnet-to-dotnet (.pem extension is important here)
cp localhost.crt /usr/lib/ssl/certs/aspnetcore-https-localhost.pem

# Cleanup
su -c 'rm localhost.crt' $usr
