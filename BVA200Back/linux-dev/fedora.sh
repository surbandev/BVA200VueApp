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
    usr='whoami'; 
fi

#setup UDEV rules
rm -f /etc/udev/rules.d/80-brightspec.rules
touch /etc/udev/rules.d/80-brightspec.rules 
echo 'SUBSYSTEM=="usb", ATTR{idVendor}=="04d8", ATTR{idProduct}=="f85d", MODE="0666"' > /etc/udev/rules.d/80-brightspec.rules 
udevadm control --reload 

#setup certificates for HTTPS
# Setup Firefox


su -c "echo 'pref(\"general.config.filename\", \"firefox.cfg\"); pref(\"general.config.obscure_value\", 0);' > ./autoconfig.js" $usr

su -c "echo '//Enable policies.json
lockPref("browser.policies.perUserDir", false);' > firefox.cfg" $usr

su -c 'echo "{
    \"policies\": {
        \"Certificates\": {
            \"Install\": [
            	\"aspnetcore-localhost-https.crt\"
            ]
        }
    }
}" > policies.json' $usr

dnf install wkhtmltopdf nss-tools dotnet-sdk-6.0 chromium xdotool

#clean existing certs as both root and non-root
dotnet dev-certs https --clean
su -c 'dotnet dev-certs https --clean' $usr

#su -c 'dotnet dev-certs https -ep localhost.crt' $usr
su -c 'dotnet dev-certs https -ep localhost.crt --format PEM' $usr

mv autoconfig.js /usr/lib64/firefox/
mv firefox.cfg /usr/lib64/firefox/
mv policies.json /usr/lib64/firefox/distribution/

su -c 'mkdir -p ~/.mozilla/certificates' $usr
su -c 'cp localhost.crt ~/.mozilla/certificates/aspnetcore-localhost-https.crt' $usr

# Trust Edge/Chrome
su -c 'certutil -d sql:$HOME/.pki/nssdb -A -t "P,," -n localhost -i ./localhost.crt' $usr
su -c 'certutil -d sql:$HOME/.pki/nssdb -A -t "C,," -n localhost -i ./localhost.crt' $usr

# Trust dotnet-to-dotnet (.pem extension is important here)
cp localhost.crt /etc/pki/tls/certs/aspnetcore-localhost-https.pem
update-ca-trust

# Cleanup
su -c 'rm localhost.crt' $usr
