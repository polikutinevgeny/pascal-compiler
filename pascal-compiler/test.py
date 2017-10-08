import os
import re
import subprocess

r = re.compile('(?P<name>.+)\.in')
total = 0
passed = 0

for i in os.listdir(r'tests/tokenizer/'):
    name = r.match(i)
    if name:
        total += 1
        print(i)
        subprocess.call([r'bin/Debug/pascal-compiler.exe', '-m', 'tokenize', '-i ', r'tests/tokenizer/' + i, '-o', 'temp.out'])
        f1, f2 = open(r'tests/tokenizer/{}.out'.format(name.group('name'))), open('temp.out')
        if f1.read() != f2.read():
            print('Test "{}" failed'.format(i))
        else:
            passed += 1
print('Total {} tests launched, {} passed, {} failed.'.format(total, passed, total - passed))
