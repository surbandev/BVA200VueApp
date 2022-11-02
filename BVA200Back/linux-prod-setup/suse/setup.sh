#!/bin/bash

#internet check
echo -e "GET http://google.com HTTP/1.0\n\n" | nc google.com 80 > /dev/null 2>&1

if [ $? -ne 0 ]; then
    echo "An internet connection is required to preform the installation / update."
    exit 1
fi

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

#make the init script executable
chmod +x ./init.sh

#make the actual executable executable... ha
chmod +x ../../BVA200

#make sure to preserve existing DB if exists and remove the existing installation directory
su -c '
    if [ -f $HOME/Desktop/publish/InternalSQLiteDb.db ]; then
        cp $HOME/Desktop/publish/InternalSQLiteDb.db $HOME/Desktop/InternalSQLiteDb.db
    fi

    if [ -f $HOME/Desktop/InternalSQLiteDb.db ]; then
        rm -rf $HOME/Desktop/publish/ ]
    else
        exit 1
    fi
' $usr

#copy everything to desktop
cp -r ../../../publish /home/${usr}/Desktop/publish

#change ownership
chown -R $usr /home/${usr}/Desktop/publish

#if we have a preserved database on the desktop, move it back to the publish folder
su -c '
    if [ -f $HOME/Desktop/InternalSQLiteDb.db ]; then
        mv $HOME/Desktop/InternalSQLiteDb.db $HOME/Desktop/publish/InternalSQLiteDb.db
    fi
' $usr

#install native repo dependencies
zypper addrepo https://download.opensuse.org/repositories/Publishing/openSUSE_Tumbleweed/Publishing.repo
zypper --gpg-auto-import-keys refresh && zypper install -y chromium wkhtmltopdf p11-kit xdotool libusb ca-certificates openssl mozilla-nss mozilla-nss-tools espeak

#setup Microsoft repos
rpm --import https://packages.microsoft.com/keys/microsoft.asc
zypper addrepo --refresh https://packages.microsoft.com/config/opensuse/15/prod.repo
zypper install -y dotnet-sdk-6.0 aspnetcore-runtime-6.0

#setup USBGuard if not already installed
#isUSBGuardInstalled=$(dpkg-query -W -f='${Status}' usbguard 2>/dev/null | grep -c "ok installed")
#if [ "$isUSBGuardInstalled" -eq "0" ]; then
#    apt install -y usbguard
#    if [ ! -f /etc/usbguard/rules.conf ]; then
#        touch /etc/usbguard/rules.conf
#    fi
#fi

#check to see if USBGuard has a policy in place to allow the Topaz reader
#the Topaz reader must 100% be allowed no matter what
#isInFile=$(cat /etc/usbguard/rules.conf | grep -c "allow id 04d8:f85d")
#if [ $isInFile -eq 0 ]; then
#    usbguard generate-policy > /etc/usbguard/rules.conf
#    echo 'allow id 04d8:f85d serial "13201012" name "Topaz - BrightSpec USB MCA" hash "u3S6XoC/eUBljIKJO2N8VLIC4Z1B7smmxxpl1GENx4U=" parent-hash "lUN32sIeMBBlD8Pd82mxu95iCTw8oKlT8iZDeg628/o=" with-interface ff:ff:ff with-connect-type ""' >> /etc/usbguard/rules.conf 
#    systemctl restart usbguard
#fi

#disable Gnome gestures
su -c '
    if [ ! -d $HOME/.local/share/gnome-shell/extensions/disable-gestures-2021@verycrazydog.gmail.com ]; then
        wget https://extensions.gnome.org/extension-data/disable-gestures-2021verycrazydog.gmail.com.v2.shell-extension.zip && 
        gnome-extensions install disable-gestures-2021verycrazydog.gmail.com.v2.shell-extension.zip && 
        gnome-extensions enable disable-gestures-2021@verycrazydog.gmail.com &&
        rm disable-gestures-2021verycrazydog.gmail.com.v2.shell-extension.zip
    fi
' $usr

#disable Gnome OSK
su -c '
    if [ ! -d $HOME/.local/share/gnome-shell/extensions/block-caribou-36@lxylxy123456.ercli.dev ]; then
        wget https://extensions.gnome.org/extension-data/block-caribou-36lxylxy123456.ercli.dev.v5.shell-extension.zip && 
        gnome-extensions install block-caribou-36lxylxy123456.ercli.dev.v5.shell-extension.zip  && 
        gnome-extensions enable block-caribou-36@lxylxy123456.ercli.dev &&
        rm block-caribou-36lxylxy123456.ercli.dev.v5.shell-extension.zip 
    fi
' $usr



#setup UDEV rules
rm -f /etc/udev/rules.d/80-brightspec.rules
touch /etc/udev/rules.d/80-brightspec.rules 
echo 'SUBSYSTEM=="usb", ATTR{idVendor}=="04d8", ATTR{idProduct}=="f85d", MODE="0666"' > /etc/udev/rules.d/80-brightspec.rules 
udevadm control --reload 

#setup certificates for HTTPS

echo 'pref("general.config.filename", "firefox.cfg"); pref("general.config.obscure_value", 0);' > ./autoconfig.js


echo '//Enable policies.json lockPref("browser.policies.perUserDir", false);' > firefox.cfg

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

su -c 'dotnet dev-certs https -ep localhost.crt --format PEM' $usr

su -c 'mkdir -p ~/.mozilla/certificates' $usr
su -c 'cp localhost.crt ~/.mozilla/certificates/aspnetcore-localhost-https.crt' $usr
su -c 'rm -rf $HOME/.pki' $usr
su -c 'mkdir -p $HOME/.pki/nssdb' $usr
su -c 'certutil --empty-password -d $HOME/.pki/nssdb -N' $usr

# Trust Edge/Chrome
su -c 'certutil -d sql:$HOME/.pki/nssdb -A -t "P,," -n localhost -i ./localhost.crt' $usr
su -c 'certutil -d sql:$HOME/.pki/nssdb -A -t "C,," -n localhost -i ./localhost.crt' $usr

# Trust dotnet-to-dotnet (.pem extension is important here) #SUSE SPECIFIC
sudo cp localhost.crt /var/lib/ca-certificates/openssl/aspnetcore-localhost-https.pem

# Cleanup
su -c 'rm localhost.crt' $usr

#setup autostart
su -c '
    if [ ! -d $HOME/.config/autostart ]; then
        mkdir $HOME/.config/autostart
    fi

    rm $HOME/.config/autostart/bva200.desktop

    rm $HOME/.config/autostart/bva200.desktop
    echo "[Desktop Entry]
    Name=BVA200
    Comment=
    Exec=$HOME/Desktop/publish/linux-prod-setup/suse/init.sh
    Terminal=false
    Type=Application" > $HOME/.config/autostart/bva200.desktop
' $usr

#Linux Power Config
su -c '
gsettings set org.gnome.settings-daemon.plugins.power ambient-enabled false
gsettings set org.gnome.settings-daemon.plugins.power idle-dim true
gsettings set org.gnome.settings-daemon.plugins.power sleep-inactive-ac-type nothing
gsettings set org.gnome.settings-daemon.plugins.power sleep-inactive-battery-type nothing
gsettings set org.gnome.desktop.lockdown disable-lock-screen true
gsettings set org.gnome.desktop.screensaver ubuntu-lock-on-suspend false
gsettings set org.gnome.desktop.screensaver lock-enabled false
gsettings set org.gnome.desktop.screensaver idle-activation-enabled true
gsettings set org.gnome.desktop.screensaver logout-enabled false
gsettings set org.gnome.desktop.screensaver user-switch-enabled false
gsettings set org.gnome.desktop.session idle-delay "uint32 450"
gsettings set org.gnome.desktop.notifications show-banners false
' $usr

#printers
systemctl -f enable cups && systemctl start cups

#deal with the bootloader giving the user to jump right in.
#set the timeout to 0
sed -i 's/GRUB_TIMEOUT=8/GRUB_TIMEOUT=0/g' /etc/default/grub 
grub2-mkconfig -o /boot/grub2/grub.cfg


zenity --info --width 300 --text "Installation / Update complete. Remove the USB Flash Drive and click OK to reboot."

reboot