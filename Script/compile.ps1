param([string]$t = 'build', [string]$c = 'Release',
      [string]$p = '/property:GenerateFullPaths=true', [string]$o = '/consoleloggerparameters:Summary')

function compile([string]$sln) {
      Write-Host "dotnet $t $sln -c $c $p $o" -ForegroundColor green
      dotnet $t $sln -c $c $p $o
}

compile .\ScadaCommon\ScadaCommon.sln
compile .\ScadaAgent\ScadaAgent\ScadaAgent.sln
compile .\ScadaComm\ScadaComm\ScadaComm.sln
compile .\ScadaServer\ScadaServer\ScadaServer.sln
compile .\ScadaWeb\ScadaWeb\ScadaWeb.sln
compile .\ScadaAdmin\ScadaAdmin\ScadaAdmin.sln
compile .\ScadaReport\ScadaReport.sln
compile .\ScadaComm\OpenDrivers\OpenDrivers.sln
compile .\ScadaComm\OpenDrivers2\OpenDrivers2.sln
compile .\ScadaServer\OpenModules\OpenModules.sln
compile .\ScadaWeb\Mimics\Mimics.sln
compile .\ScadaWeb\OpenPlugins\OpenPlugins.sln
compile .\ScadaAdmin\OpenExtensions\OpenExtensions.sln
