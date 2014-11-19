#!/bin/sh
cd $1
make
/usr/bin/osascript -e 'do shell script "make install" with administrator privileges'
