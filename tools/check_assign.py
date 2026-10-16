import re,glob,math,os
W='C:/games/videogames/Boomerang.Fu.v1.3.6/Boomerang.Fu.v1.3.6/mod8p/'
for f in sorted(glob.glob(W+'results/*.txt')):
    for l in open(f,encoding='utf-8',errors='replace'):
        m=re.search(r'Spawn assignment round (\d+) \(Assets/Scenes/(\w+)\.unity, team=(\w+)\): (.*)',l)
        if not m: continue
        p=[(float(a),float(b)) for a,b in re.findall(r'=\(([\-\d\.]+),([\-\d\.]+)\)',m.group(4))]
        d=min(math.dist(p[i],p[j]) for i in range(len(p)) for j in range(i+1,len(p)))
        print('%s r%s %s team=%s n=%d minDist=%.1f distinct=%s'%(os.path.basename(f)[:-4],m.group(1),m.group(2),m.group(3),len(p),d,len(set(p))==len(p)))
