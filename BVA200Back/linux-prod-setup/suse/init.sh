#!/bin/bash

#kill any prior running instances
p5k=$(lsof -t -i:5000)
p5k1=$(lsof -t -i:5001)

if [ $p5k > 0 ]
then 
   kill -9 $p5k
fi

if [ $p5k1 > 0 ]
then 
   kill -9 $p5k1
fi

killall chromium-browser && killall chromium

#launch backend and chrome
cd $HOME/Desktop/publish && ./BVA200 & chromium-browser --password-store=basic --kiosk --incognito https://localhost:5001 && fg
