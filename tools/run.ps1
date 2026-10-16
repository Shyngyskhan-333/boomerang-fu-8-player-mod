param([int]$Seconds=45,[string]$Pattern='8 Players|Error|Exception|DIAG|Grew|Rewired|GameManager|Round|Level|Spawn|8P')
$g='C:\games\videogames\Boomerang.Fu.v1.3.6\Boomerang.Fu.v1.3.6\game'; $w='C:\games\videogames\Boomerang.Fu.v1.3.6\Boomerang.Fu.v1.3.6\mod8p'
Stop-Process -Name 'Boomerang Fu' -Force -ErrorAction SilentlyContinue
Copy-Item "$w\build\BoomerangFu8P.dll" "$g\BepInEx\plugins\" -Force
$p=Start-Process "$g\Boomerang Fu.exe" -WorkingDirectory $g -PassThru -RedirectStandardOutput "$w\tools\stdout.txt"
Start-Sleep $Seconds
Stop-Process -Name 'Boomerang Fu' -Force -ErrorAction SilentlyContinue
Get-Content "$g\BepInEx\LogOutput.log" | Select-String -Pattern $Pattern | select -First 120 | %{$_.Line}
