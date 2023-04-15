#!/bin/bash
dotnet clean -c:Release || exit;
dotnet publish -c:Release -r linux-x64 -p:SelfContained=true -p:PublishSingleFile=true -p:PublishReadyToRun=true
cp VirtualMachine/EVIL.CVIL/bin/Release/net7.0/linux-x64/publish/cvil /opt/bin/cvil || exit
cp VirtualMachine/EVIL.EVIL/bin/Release/net7.0/linux-x64/publish/evil /opt/bin/evil || exit
