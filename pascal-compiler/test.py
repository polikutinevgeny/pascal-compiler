import os
import re
import subprocess

r = re.compile('(?P<name>.+)\.in')
total = 0
passed = 0

for d in os.listdir(r'tests/'):
    print("Starting {}".format(d))
    for i in os.listdir(r'tests/' + d):
        name = r.match(i)
        if name:
            total += 1
            print(i)
            # subprocess.call([r'bin/Debug/pascal-compiler.exe', '-m', 'tokenize', '-i', r'tests/tokenizer/{}'.format(i), '-o', r'tests/tokenizer/{}.out'.format(name.group('name'))])
            subprocess.call([r'bin/Debug/pascal-compiler.exe', '-m', 'tokenize', '-i', r'tests/tokenizer/{}'.format(i), '-o', 'temp.out'])
            f1, f2 = open(r'tests/tokenizer/{}.out'.format(name.group('name')), encoding='cp1251'), open('temp.out', encoding='cp1251')
            if f1.read() != f2.read():
                print('Test "{}" failed'.format(i))
            else:
                passed += 1
            f1.close()
            f2.close()
print('Total {} tests launched, {} passed, {} failed.'.format(total, passed, total - passed))
os.remove('temp.out')
