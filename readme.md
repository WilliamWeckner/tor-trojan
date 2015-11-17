# TorTrojan

TorTrojan is a simple trojan-horse written in C# using tor to communicate. It deploys itself as service to survive reboots and accepts commands which will be forwarded to cmd.exe.
An Adobe Flash Player Update Fake is used to get administrative rights.
It uses a control panel written in PHP to list and control the clients.

## General

This piece of software is probably not working anymore and should not be used for violating any laws. The purpose is to give a proof-of-concept on how simple it is to write such evil program with just a small amount of knowledge, to test and improve own skills and give others an idea of how a trojan-horse could work.

## Further information

The working-directory of the whole script is "%system32%\com\tmp\".
Almost all variables of type string will be base64 encoded to add some obfuscation.

## Used Ports

* :56788 - TCP - Tor (net.exe)
* :56789 - TCP - Service (service.exe)
* :56790 - TCP - TorCtl

The remoteControl will receive notifications on port 1234.

### winnet

A dummy application that looks like an Adobe Flash Player Update.
It requires administrative privileges to run. When started, it will download the original Flash Player Update, the Server and run both.

### Server.exe

A package containing an extraction utility (7zip), net.exe and service.exe.
When they not already exist, it extracts and deploys all the needed utilities,
including the Tor config-file "rc" (where "__TARGET__" will be replaced with
the working-directory path).
The config is base64-encoded.
Either way it will start the net.exe and service.exe, and delete itself.

### net.exe / net.pack (Tor)

This is Tor. It will use the generated "rc" as config file.
The proxy will listen on port 56788.

### service.exe / service.pack (wuaucilt)

Named like the Windows Update Agent "wuauclt".
It installs itself as a service and starts the "net.exe" on every startup.
Files created by windows containing info about the service status will be removed on every
startup. ("InstallUtil.InstallLog", "service.InstallLog", "service.InstallState")
Tor will be launched in a separate thread and the service will keep checking
for the hostname-file which contains the Tor-hostname.
When found, it will connect through the Tor-Proxy, contact the SERVICE_DOMAIN
and call the "/service/cache"-Route to inform the remoteControl-Script about
the running client.
The script should be running on Port 1234 (as defined in SERVICE_PORT).
There is also a direct CLI-Interface on Port 56789. It will execute given
commands directly on the clients system.

### remoteControl

This is the web interface written with Fuel-PHP. The login is just "admin" and
"admin" until you change it (hardcoded) in your
fuel/app/classes/controller/panel.php:29

## Requirements

* Fast server to provide packages downloaded from winnet. *(optional)*
* remoteControl hosted within Tor as hidden service

## Installation (Build Server.exe)

* Install your remoteControl somewhere and get your hidden service hostname
* Build wuaucilt
  * Set remoteControl hostname as SERVICE_DOMAIN in wuaucilt/Service1.cs as base64
  * Rename the binary to service.exe
  * 7z-compress the binary
  * Rename compressed file to service.pack
* Copy service.pack to Server/Resources
* Build Server.exe

## Installation for winnet (Requires Server.exe)

* Update the DOWNLOAD_SERVER const in winnet/Program.cs to yours
* Get some adobe flash player update (or similar) and name it upd.exe on your server
* Upload Server.exe to your server
