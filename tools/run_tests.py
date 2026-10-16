import subprocess, time, os, sys, re, shutil
W=r'C:\games\videogames\Boomerang.Fu.v1.3.6\Boomerang.Fu.v1.3.6\mod8p'
G=r'C:\games\videogames\Boomerang.Fu.v1.3.6\Boomerang.Fu.v1.3.6\game'
CFG=G+r'\BepInEx\config\local.boomerangfu.eightplayers.cfg'
LOG=G+r'\BepInEx\LogOutput.log'
SC={
 'dm8':dict(AutoPlayers=8),
 'team8':dict(AutoPlayers=8,AutoTeam='true'),
 'dm7':dict(AutoPlayers=7),
 'team7':dict(AutoPlayers=7,AutoTeam='true'),
 'dm6':dict(AutoPlayers=6),
 'dm5':dict(AutoPlayers=5),
 'dm4':dict(AutoPlayers=4),
 'dm3':dict(AutoPlayers=3),
 'dm2':dict(AutoPlayers=2),
 'golden8':dict(AutoPlayers=8,AutoMatchType='GoldenDisc'),
 'koth8':dict(AutoPlayers=8,AutoMatchType='KingOfTheHill'),
 'hide8':dict(AutoPlayers=8,AutoMatchType='HideAndSeek'),
 'long8':dict(AutoPlayers=8,AutoScoreGoal=14),
}
names=sys.argv[2:] or list(SC)
secs=int(sys.argv[1])
os.makedirs(W+r'\results',exist_ok=True)
subprocess.run(['taskkill','/IM','Boomerang Fu.exe','/F'],capture_output=True); time.sleep(3)
shutil.copy(os.path.join(W,'build','BoomerangFu8P.dll'),os.path.join(G,'BepInEx','plugins','BoomerangFu8P.dll'))
for n in names:
    subprocess.run(['taskkill','/IM','Boomerang Fu.exe','/F'],capture_output=True)
    shots=W+r'\shots_'+n
    shutil.rmtree(shots,ignore_errors=True); os.makedirs(shots)
    d=SC[n]
    lines=['[Test]','AutoMatch = true','ScreenshotInterval = 8','ScreenshotDir = '+shots]
    if 'AutoScoreGoal' not in d: lines.append('AutoScoreGoal = 2')
    lines+=['%s = %s'%(k,v) for k,v in d.items()]
    open(CFG,'w').write('\n'.join(lines)+'\n')
    shutil.copy(W+r'\build\BoomerangFu8P.dll',G+r'\BepInEx\plugins\BoomerangFu8P.dll')
    out=open(W+r'\tools\stdout_'+n+'.txt','w')
    p=subprocess.Popen([G+r'\Boomerang Fu.exe'],cwd=G,stdout=out,stderr=subprocess.STDOUT)
    time.sleep(secs)
    subprocess.run(['taskkill','/IM','Boomerang Fu.exe','/F'],capture_output=True)
    out.close(); time.sleep(4)
    log=open(LOG,encoding='utf-8',errors='replace').read().splitlines()
    keep=[l for l in log if re.search(r'AUTOMATCH (bots|menu|continue)|Spawn (plan|assignment)|Playlist|GoldenDisc|\[Error|Exception',l)]
    so=open(W+r'\tools\stdout_'+n+'.txt',encoding='utf-8',errors='replace').read().splitlines()
    exc=[ (so[i]+' | '+(so[i+1] if i+1<len(so) else '')) for i,l in enumerate(so) if re.search(r'Exception:',l)]
    open(os.path.join(W,'results',n+'.txt'),'w',encoding='utf-8').write('\n'.join(keep)+'\n--- unity exceptions ---\n'+'\n'.join(exc)+'\n')
    print(n,'lines',len(keep),'exceptions',len(exc),'screens',len(os.listdir(shots)))
