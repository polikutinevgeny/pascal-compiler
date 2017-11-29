import os
import re
import subprocess
import sys
import argparse

r = re.compile('(?P<name>.+)\.in')
total = 0
passed = 0

for d in os.listdir(r'tests/'):
    print("\nStarting {} tests".format(d))
    c = 0
    p = 0
    for i in os.listdir(r'tests/' + d):
        name = r.match(i)
        if name:
            total += 1
            c += 1
            if d == 'generate':
                # subprocess.run([r'C:/lazarus/fpc/bin/i386-win32/fpc.exe', '-MObjfpc', r'tests/{}/{}'.format(d, i)], stdout=subprocess.DEVNULL)
                subprocess.run([r'bin/Debug/PascalCompiler.exe', '-m', d, '-i', r'tests/{}/{}'.format(d, i), '-o', 'temp.asm'])
                subprocess.run([r'C:/Program Files (x86)/Microsoft Visual Studio/2017/Community/VC/Tools/MSVC/14.11.25503/bin/HostX86/x86/ml.exe', '/c', '/coff', 'temp.asm'], stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
                subprocess.run([r'C:/Program Files (x86)/Microsoft Visual Studio/2017/Community/VC/Tools/MSVC/14.11.25503/bin/HostX86/x86/link.exe', '/SUBSYSTEM:CONSOLE', 'temp.obj'], stdout=subprocess.DEVNULL)                
                s = subprocess.check_output([r'temp.exe'.format(d, name.group('name'))])
                # f2 = open(r'tests/{}/{}.out'.format(d, name.group('name')), 'w', encoding='cp1251');
                # f2.write(s.replace(b'\r\n', b'\n').decode('cp1251'));
                # f2.close()
                f1 = open(r'tests/{}/{}.out'.format(d, name.group('name')), encoding='cp1251');
                if f1.read() != s.replace(b'\r\n', b'\n').decode('cp1251'):
                    print('Test "{}" failed'.format(i))
                else:
                    print('Test "{}" passed'.format(i))
                    passed += 1
                    p += 1
                f1.close();
            else:
                # subprocess.call([r'bin/Debug/PascalCompiler.exe', '-m', '{}'.format(d), '-i', r'tests/{}/{}'.format(d, i), '-o', r'tests/{}/{}.out'.format(d, name.group('name'))])
                subprocess.call([r'bin/Debug/PascalCompiler.exe', '-m', d, '-i', r'tests/{}/{}'.format(d, i), '-o', 'temp.txt'])
                f1, f2 = open(r'tests/{}/{}.out'.format(d, name.group('name')), encoding='cp1251'), open('temp.txt', encoding='cp1251')
                if f1.read() != f2.read():
                    print('Test "{}" failed'.format(i))
                else:
                    print('Test "{}" passed'.format(i))
                    passed += 1
                    p += 1
                f1.close()
                f2.close()
    print("\nEnded {} tests, launched {}, passed, {}, failed {}.".format(d, c, p, c - p))
print('Total {} tests launched, {} passed, {} failed.'.format(total, passed, total - passed))
os.remove('temp.txt')
os.remove('temp.obj')
os.remove('temp.asm')
os.remove('temp.exe')