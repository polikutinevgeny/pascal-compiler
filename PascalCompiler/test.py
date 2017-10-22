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
            subprocess.call([r'bin/Debug/PascalCompiler.exe', '-m', '{}'.format(d), '-i', r'tests/{}/{}'.format(d, i), '-o', r'tests/{}/{}.out'.format(d, name.group('name'))])
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
