from tabulate import tabulate
d = {}
d['w'] = 'Whitespace,'
d['i'] = 'Identifier,'
d['n'] = 'Integer,'
d['f'] = 'Float,'
d['u'] = 'UnexpectedChar,'
d['l'] = 'NewLine,'
d['o'] = 'Operator,'
d['s'] = 'StringLiteral,'
d['p'] = 'Separator,'
d['c'] = 'Comment,'
d['e'] = 'Stop,'
d['q'] = 'FloatExp,'
d['r'] = 'FloatSign,'
d['z'] = 'FloatEnd,'
d['y'] = 'StringLiteralEOL,'
d['m'] = 'MultilineComment,'
d[':'] = 'Colon,'
d['<'] = 'OperatorLess,'
d['>'] = 'OperatorGreater,'
d['/'] = 'Slash,'
d['~'] = "Return,"
d['('] = "Parenthesis,"
d['.'] = 'OperatorDot,'
d['*'] = 'Asterisk,'
d['1'] = 'ReturnInt,'
d['j'] = 'FloatDot,'
data = []
           #'\x00\x01\x02\x03\x04\x05\x06\x07\x08\t\n\x0b\x0c\r\x0e\x0f\x10\x11\x12\x13\x14\x15\x16\x17\x18\x19\x1a\x1b\x1c\x1d\x1e\x1f !"#$%&\'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~\x7f'
data.append('u   u   u   u   u   u   u   u   u   w l  u   u  l  u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u  wuuuuuus (poopo./nnnnnnnnnn:p<o>uouuuuuuuuuuuuuuuuuuuuuuuuuupu poiuiiiiiiiiiiiiiiiiiiiiiiiiiimuuuu') #whitespace
data.append('u   u   u   u   u   u   u   u   u   w l  u   u  l  u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u  wuuuuuus (poopo./nnnnnnnnnn:p<o>uouuuuuuuuuuuuuuuuuuuuuuuuuupu poiuiiiiiiiiiiiiiiiiiiiiiiiiiimuuuu') #new line
data.append('u   u   u   u   u   u   u   u   u   e e  u   u  e  u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u  euuuuuuu eeeeeeeeiiiiiiiiiieeeeeuuuuuuuuuuuuuuuuuuuuuuuuuuuueu euiuiiiiiiiiiiiiiiiiiiiiiiiiiieuuuu') #identifier
data.append('u   u   u   u   u   u   u   u   u   e e  u   u  e  u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u  euuuuuuu ueeeeejennnnnnnnnneeeeeuuuuuuuuuuuuuuuuuuuuuuuuuuuuuu euuuuuuuquuuuuuuuuuuuuuuuuuuuueuuuu') #integer
data.append('u   u   u   u   u   u   u   u   u   e e  u   u  e  u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u  euuuuuuu ueeeee1effffffffffeeeeeuuuuuuuuuuuuuuuuuuuuuuuuuuuuuu euuuuuuuuuuuuuuuuuuuuuuuuuuuuueuuuu') #floatDot
data.append('u   u   u   u   u   u   u   u   u   e e  u   u  e  u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u  euuuuuuu ueeeeeeeffffffffffeeeeeuuuuuuuuuuuuuuuuuuuuuuuuuuuuuu euuuuuuuquuuuuuuuuuuuuuuuuuuuueuuuu') #float
data.append('u   u   u   u   u   u   u   u   u   u u  u   u  u  u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u  uuuuuuuu uuururuuzzzzzzzzzzuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuu uuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuu') #float exp
data.append('u   u   u   u   u   u   u   u   u   u u  u   u  u  u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u  uuuuuuuu uuuuuuuuzzzzzzzzzzuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuu uuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuu') #float sign
data.append('u   u   u   u   u   u   u   u   u   e e  u   u  e  u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u  euuuuuuu eeeeeeeezzzzzzzzzzeeeeeuuuuuuuuuuuuuuuuuuuuuuuuuuuuuu uuuuuuuuuuuuuuuuuuuuuuuuuuuuuueuuuu') #float end
data.append('u   u   u   u   u   u   u   u   u   s y  u   u  y  u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u  ssssssse sssssssssssssssssssssssssssssssssssssssssssssssssssss ssssssssssssssssssssssssssssssssssu') #string
data.append('u   u   u   u   u   u   u   u   u   e e  u   u  e  u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u  euuuuuuu euuuuuuueeeeeeeeeeuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuu uuuueeeeeeeeeeeeeeeeeeeeeeeeeeeuuuu') #operator
data.append('u   u   u   u   u   u   u   u   u   e e  u   u  e  u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u  euuuuuuu euuuuuuueeeeeeeeeeuuuoouuuuuuuuuuuuuuuuuuuuuuuuuuuuuu uuuueeeeeeeeeeeeeeeeeeeeeeeeeeeuuuu') #operator less
data.append('u   u   u   u   u   u   u   u   u   e e  u   u  e  u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u  euuuuuuu euuuuuuueeeeeeeeeeuuuouuuuuuuuuuuuuuuuuuuuuuuuuuuuuuu uuuueeeeeeeeeeeeeeeeeeeeeeeeeeeuuuu') #operator greater
data.append('u   u   u   u   u   u   u   u   u   e e  u   u  e  u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u  euuuuuuu euuuuuuuuuuuuuuuuuuuuouuuuuuuuuuuuuuuuuuuuuuuuuuuuueu uueueeeeeeeeeeeeeeeeeeeeeeeeeeeuuuu') #colon
data.append('u   u   u   u   u   u   u   u   u   e e  u   u  e  u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u  euuuuuue eeueueuueeeeeeeeeeueuuuueuuuuuuuuuuuuuuuuuuuuuuuuuuuu uueueeeeeeeeeeeeeeeeeeeeeeeeeeeuuuu') #separator
data.append('u   u   u   u   u   u   u   u   u   c l  u   u  l  u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u  cccccccc ccccccccccccccccccccccccccccccccccccccccccccccccccccc ccccccccccccccccccccccccccccccccccu') #comment
data.append('u   u   u   u   u   u   u   u   u   m m  u   u  m  u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u  mmmmmmmm mm*mmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmm mmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmwmu') #multiline comment
data.append('u   u   u   u   u   u   u   u   u   e e  u   u  e  u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u  euuuuuuu euuuuuu~eeeeeeeeeeuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuu uuuueeeeeeeeeeeeeeeeeeeeeeeeeeeuuuu') #slash
data.append('u   u   u   u   u   u   u   u   u   w l  u   u  l  u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u  wuuuuuus (poopo./nnnnnnnnnn:p<o>uouuuuuuuuuuuuuuuuuuuuuuuuuupu poiuiiiiiiiiiiiiiiiiiiiiiiiiiimuuuu') #stop
data.append('u   u   u   u   u   u   u   u   u   u u  u   u  u  u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u  uuuuuuuu muuuuu.cuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuu uuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuu') #return
data.append('u   u   u   u   u   u   u   u   u   u u  u   u  u  u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u  uuuuuuuu muuuuu.cuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuu uuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuu') #returnInt
data.append('u   u   u   u   u   u   u   u   u   e e  u   u  e  u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u  euuuuuue ee~euepueeeeeeeeeeueuuuueuuuuuuuuuuuuuuuuuuuuuuuuuuuu uueueeeeeeeeeeeeeeeeeeeeeeeeeeeuuuu') #parenthesis
data.append('u   u   u   u   u   u   u   u   u   u u  u   u  u  u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u  uuuuuuuu upuuuuouuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuu uuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuu') #OperatorDot
data.append('u   u   u   u   u   u   u   u   u   m m  u   u  m  u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u  mmmmmmmm mwmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmm mmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmwmu') #asterisk

rdata = []
for i in data:
    rdata.append(['{'])
    for j in i:
        if j == ' ':
            continue
        rdata[-1].append('State.' + d[j])
    rdata[-1].append('},')

ouf=open('1.txt', 'w')
print(tabulate(rdata, tablefmt='plain'), file=ouf)
for i in d.values():
    print(i)
