#!/bin/bash

red=`tput setaf 1`
green=`tput setaf 2`
reset=`tput sgr0`

task="build"
config="Release"
options="/property:GenerateFullPaths=true /consoleloggerparameters:Summary"

while getopts ":t:c:" opt; do
  case $opt in
    t) task="$OPTARG"
    ;;
    c) config="$OPTARG"
    ;;
    \?) echo "Invalid option -$OPTARG" >&2
    ;;
  esac
done

compile() {
  echo "${green}\$ dotnet $task $@ -c $config $options ${reset}";
  dotnet $task "$@" -c $config $options;
}

compile ./ScadaCommon/ScadaCommon.sln
compile ./ScadaAgent/ScadaAgent/ScadaAgent.sln
compile ./ScadaComm/ScadaComm/ScadaComm.sln
compile ./ScadaServer/ScadaServer/ScadaServer.sln
compile ./ScadaWeb/ScadaWeb/ScadaWeb.sln
compile ./ScadaAdmin/ScadaAdmin/ScadaAdmin.sln
compile ./ScadaReport/ScadaReport.sln
compile ./ScadaComm/OpenDrivers/OpenDrivers.sln
compile ./ScadaComm/OpenDrivers2/OpenDrivers2.sln
compile ./ScadaServer/OpenModules/OpenModules.sln
compile ./ScadaWeb/OpenPlugins/OpenPlugins.sln
compile ./ScadaAdmin/OpenExtensions/OpenExtensions.sln
