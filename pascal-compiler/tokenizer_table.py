from tabulate import tabulate
d = {}
d['x'] = 'Start'
d['u'] = 'UnexpectedChar'
d['l'] = 'NewLine'
d['s'] = 'StringLiteral'
d['h'] = 'HexNumber'
d['ш'] = 'HexNumberValue'
d['b'] = 'BinNumber'
d['д'] = 'BinNumberValue'
d['o'] = 'OctNumber'
d['p'] = 'Parenthesis'
d['S'] = 'Separator'
d['O'] = 'Operator'
d['.'] = 'DotOperator'
d['+'] = 'PlusOperator'
d['*'] = 'AsteriskOperator'
d['-'] = 'MinusOperator'
d['/'] = 'SlashOperator'
d['I'] = 'Integer'
d[':'] = 'Colon'
d['<'] = 'Less'
d['>'] = 'More'
d['a'] = 'Ampersand'
d['i'] = 'Identifier'
d['m'] = 'MultilineComment'
d['c'] = 'Comment'
d['A'] = 'StringLiteralClosed'
d['#'] = 'StringLiteralCharStart'
d['$'] = 'StringLiteralCharHex'
d['H'] = 'StringLiteralCharHexValue'
d['D'] = 'StringLiteralCharDec'
d['&'] = 'StringLiteralCharOct'
d['Q'] = 'StringLiteralCharOctValue'
d[r'%'] = 'StringLiteralCharBin'
d['B'] = 'StringLiteralCharBinValue'
d['d'] = 'FloatDot'
d['f'] = 'FloatFrac'
d['e'] = 'FloatExp'
d['P'] = 'FloatExpPlus'
d['M'] = 'FloatExpMinus'
d['E'] = 'FloatExpValue'
d['V'] = 'MultilineCommentAsterisk'
d['v'] = 'MultilineCommentEnd'
d['N'] = 'MultilineCommentNewLine'
data = []
           #r'\x00\x01\x02\x03\x04\x05\x06\x07\x08\t\n\x0b\x0c\r\x0e\x0f\x10\x11\x12\x13\x14\x15\x16\x17\x18\x19\x1a\x1b\x1c\x1d\x1e\x1f !"#$%&\'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~\x7f'
data.append(r'x   u   u   u   u   u   u   u   u   x l u   u   l u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   xuu#hbas pS*+S-./IIIIIIIIII:S<O>uOiiiiiiiiiiiiiiiiiiiiiiiiiiSu SOiuiiiiiiiiiiiiiiiiiiiiiiiiiimuuuu') # Start
data.append(r'x   u   u   u   u   u   u   u   u   x x u   u   x u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   xuuxxxxx xxxxxxxxiiiiiiiiiixxxxxuxiiiiiiiiiiiiiiiiiiiiiiiiiixu xxiuiiiiiiiiiiiiiiiiiiiiiiiiiixuuuu') # Identifier
data.append(r'x   u   u   u   u   u   u   u   u   x l u   u   l u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   xuuxxxxx xxxxxxxxxxxxxxxxxxxxxxxuxxxxxxxxxxxxxxxxxxxxxxxxxxxxu xxxuxxxxxxxxxxxxxxxxxxxxxxxxxxxuuuu') # NewLine
data.append(r'x   u   u   u   u   u   u   u   u   s u u   u   u u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   sssssssA sssssssssssssssssssssssssssssssssssssssssssssssssssss ssssssssssssssssssssssssssssssssssu') # StringLiteral
data.append(r'x   u   u   u   u   u   u   u   u   x x u   u   x u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   xxx#xxxs xxxxxxxxxxxxxxxxxxxxxxxuxxxxxxxxxxxxxxxxxxxxxxxxxxxxx xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxu') # StringLiteralClosed
data.append(r'x   u   u   u   u   u   u   u   u   u u u   u   u u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   uuuu$%&u uuuuuuuuDDDDDDDDDDuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuu uuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuu') # StringLiteralCharStart
data.append(r'x   u   u   u   u   u   u   u   u   x x u   u   x u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   xuu#xxxs xxxxxxxxDDDDDDDDDDxxxxxuxxxxxxxxxxxxxxxxxxxxxxxxxxxxx xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx') # StringLiteralCharDec
data.append(r'x   u   u   u   u   u   u   u   u   u u u   u   u u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   uuuuuuuu uuuuuuuuHHHHHHHHHHuuuuuuuHHHHHHuuuuuuuuuuuuuuuuuuuuuu uuuuHHHHHHuuuuuuuuuuuuuuuuuuuuuuuuu') # StringLiteralCharHex
data.append(r'x   u   u   u   u   u   u   u   u   x x u   u   x u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   xuu#xxxs xxxxxxxxHHHHHHHHHHxxxxxuxHHHHHHxxxxxxxxxxxxxxxxxxxxxx xxxxHHHHHHxxxxxxxxxxxxxxxxxxxxxxxxx') # StringLiteralCharHexValue
data.append(r'x   u   u   u   u   u   u   u   u   u u u   u   u u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   uuuuuuuu uuuuuuuuQQQQQQQQuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuu uuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuu') # StringLiteralCharOct
data.append(r'x   u   u   u   u   u   u   u   u   x x u   u   x u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   xuu#xxxs xxxxxxxxQQQQQQQQuuxxxxxuxxxxxxxxxxxxxxxxxxxxxxxxxxxxx xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx') # StringLiteralCharOctValue
data.append(r'x   u   u   u   u   u   u   u   u   u u u   u   u u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   uuuuuuuu uuuuuuuuBBuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuu uuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuu') # StringLiteralCharBin
data.append(r'x   u   u   u   u   u   u   u   u   x x u   u   x u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   xuu#xxxs xxxxxxxxBBxxxxxxxxxxxxxuxxxxxxxxxxxxxxxxxxxxxxxxxxxxx xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx') # StringLiteralCharBinValue
           #r'\x00\x01\x02\x03\x04\x05\x06\x07\x08\t\n\x0b\x0c\r\x0e\x0f\x10\x11\x12\x13\x14\x15\x16\x17\x18\x19\x1a\x1b\x1c\x1d\x1e\x1f !"#$%&\'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~\x7f'
data.append(r'x   u   u   u   u   u   u   u   u   x x u   u   x u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   xuuxxxxx xxxxxxdxIIIIIIIIIIxxxxxuxxxxxxxxxxxxxxxxxxxxxxxxxxxxu xxxuxxxxxxxxxxxxxxxxxxxxxxxxxxxuuuu') # Integer
data.append(r'x   u   u   u   u   u   u   u   u   u u u   u   u u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   uuuuuuuu uuuuuuxuffffffffffuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuu uuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuu') # FloatDot
data.append(r'x   u   u   u   u   u   u   u   u   x x u   u   x u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   xuuxxxxx xxxxxxuxffffffffffxxxxxuxxxxxexxxxxxxxxxxxxxxxxxxxxxu xxxuxxxxexxxxxxxxxxxxxxxxxxxxxxuuuu') # FloatFrac
data.append(r'x   u   u   u   u   u   u   u   u   u u u   u   u u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   uuuuuuuu uuuPuMuuEEEEEEEEEEuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuu uuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuu') # FloatExp
data.append(r'x   u   u   u   u   u   u   u   u   u u u   u   u u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   uuuuuuuu uuuuuuuuEEEEEEEEEEuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuu uuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuu') # FloatExpPlus
data.append(r'x   u   u   u   u   u   u   u   u   u u u   u   u u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   uuuuuuuu uuuuuuuuEEEEEEEEEEuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuu uuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuu') # FloatExpMinus
data.append(r'x   u   u   u   u   u   u   u   u   x x u   u   x u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   xuuxxxxx xxxxxxuxEEEEEEEEEExxxxxuxxxxxxxxxxxxxxxxxxxxxxxxxxxxu xxxuxxxxxxxxxxxxxxxxxxxxxxxxxxxuuuu') # FloatExpValue
           #r'\x00\x01\x02\x03\x04\x05\x06\x07\x08\t\n\x0b\x0c\r\x0e\x0f\x10\x11\x12\x13\x14\x15\x16\x17\x18\x19\x1a\x1b\x1c\x1d\x1e\x1f !"#$%&\'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~\x7f'
data.append(r'u   u   u   u   u   u   u   u   u   u u u   u   u u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   uuuuuuuu uuuuuuuuoooooooouuuuuuuuuiiiiiiiiiiiiiiiiiiiiiiiiiiuu uuiuiiiiiiiiiiiiiiiiiiiiiiiiiiuuuuu') # Ampersand
data.append(r'u   u   u   u   u   u   u   u   u   x x u   u   x u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   xuuxxxxx xxxxxxxxoooooooouuxxxxxuxxxxxxxxxxxxxxxxxxxxxxxxxxxxu xxxuxxxxxxxxxxxxxxxxxxxxxxxxxxxuuuu') # OctNumber
data.append(r'u   u   u   u   u   u   u   u   u   u u u   u   u u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   uuuuuuuu uuuuuuuuшшшшшшшшшшuuuuuuuшшшшшшuuuuuuuuuuuuuuuuuuuuuu uuuuшшшшшшuuuuuuuuuuuuuuuuuuuuuuuuu') # HexNumber
data.append(r'u   u   u   u   u   u   u   u   u   x x u   u   x u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   xuuxxxxx xxxxxxxxшшшшшшшшшшxxxxxuxшшшшшшxxxxxxxxxxxxxxxxxxxxxu xxxuшшшшшшxxxxxxxxxxxxxxxxxxxxxuuuu') # HexNumberValue
data.append(r'u   u   u   u   u   u   u   u   u   u u u   u   u u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   uuuuuuuu uuuuuuuuддuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuu uuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuu') # BinNumber
data.append(r'u   u   u   u   u   u   u   u   u   x x u   u   x u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   xuuxxxxx xxxxxxxxддuuuuuuuuxxxxxuxxxxxxxxxxxxxxxxxxxxxxxxxxxxu xxxuxxxxxxxxxxxxxxxxxxxxxxxxxxxuuuu') # BinNumberValue
           #r'\x00\x01\x02\x03\x04\x05\x06\x07\x08\t\n\x0b\x0c\r\x0e\x0f\x10\x11\x12\x13\x14\x15\x16\x17\x18\x19\x1a\x1b\x1c\x1d\x1e\x1f !"#$%&\'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~\x7f'
data.append(r'u   u   u   u   u   u   u   u   u   x x u   u   x u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   xuuxxxxx xxxxxxxcxxxxxxxxxxxxxOxuxxxxxxxxxxxxxxxxxxxxxxxxxxxxu xxxuxxxxxxxxxxxxxxxxxxxxxxxxxxxuuuu') # SlashOperator
data.append(r'u   u   u   u   u   u   u   u   u   c x u   u   x u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   cccccccc ccccccccccccccccccccccccccccccccccccccccccccccccccccc ccccccccccccccccccccccccccccccccccu') # Comment
data.append(r'u   u   u   u   u   u   u   u   u   x x u   u   x u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   xuuxxxxx xxxxxxxxxxxxxxxxxxxxxOxuxxxxxxxxxxxxxxxxxxxxxxxxxxxxu xxxuxxxxxxxxxxxxxxxxxxxxxxxxxxxuuuu') # PlusOperator
data.append(r'u   u   u   u   u   u   u   u   u   x x u   u   x u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   xuuxxxxx xxOxxxxxxxxxxxxxxxxxxOxuxxxxxxxxxxxxxxxxxxxxxxxxxxxxu xxxuxxxxxxxxxxxxxxxxxxxxxxxxxxxuuuu') # AsteriskOperator
data.append(r'u   u   u   u   u   u   u   u   u   x x u   u   x u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   xuuxxxxx xxxxxxxxxxxxxxxxxxxxxOxuxxxxxxxxxxxxxxxxxxxxxxxxxxxxu xxxuxxxxxxxxxxxxxxxxxxxxxxxxxxxuuuu') # MinusOperator
data.append(r'u   u   u   u   u   u   u   u   u   x x u   u   x u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   xuuxxxxx xxxxxxxxxxxxxxxxxxxxOOOuxxxxxxxxxxxxxxxxxxxxxxxxxxxxu xxxuxxxxxxxxxxxxxxxxxxxxxxxxxxxuuuu') # Less
data.append(r'u   u   u   u   u   u   u   u   u   x x u   u   x u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   xuuxxxxx xxxxxxxxxxxxxxxxxxxxOOOuxxxxxxxxxxxxxxxxxxxxxxxxxxxxu xxxuxxxxxxxxxxxxxxxxxxxxxxxxxxxuuuu') # More
data.append(r'u   u   u   u   u   u   u   u   u   x x u   u   x u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   xuuxxxxx xxxxxxxxxxxxxxxxxxxxxxxuxxxxxxxxxxxxxxxxxxxxxxxxxxxxu xxxuxxxxxxxxxxxxxxxxxxxxxxxxxxxuuuu') # Operator
data.append(r'u   u   u   u   u   u   u   u   u   x x u   u   x u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   xuuxxxxx xxxxxxxxxxxxxxxxxxxxxOxuxxxxxxxxxxxxxxxxxxxxxxxxxxxxu xxxuxxxxxxxxxxxxxxxxxxxxxxxxxxxuuuu') # Colon
           #r'\x00\x01\x02\x03\x04\x05\x06\x07\x08\t\n\x0b\x0c\r\x0e\x0f\x10\x11\x12\x13\x14\x15\x16\x17\x18\x19\x1a\x1b\x1c\x1d\x1e\x1f !"#$%&\'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~\x7f'
data.append(r'u   u   u   u   u   u   u   u   u   x x u   u   x u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   xuuxxxxx xxmxxxSxxxxxxxxxxxxxxxxuxxxxxxxxxxxxxxxxxxxxxxxxxxxxu xxxuxxxxxxxxxxxxxxxxxxxxxxxxxxxuuuu') # Parenthesis
data.append(r'u   u   u   u   u   u   u   u   u   x x u   u   x u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   xuuxxxxx xSxxxxOxxxxxxxxxxxxxxxxuxxxxxxxxxxxxxxxxxxxxxxxxxxxxu xxxuxxxxxxxxxxxxxxxxxxxxxxxxxxxuuuu') # DotOperator
data.append(r'u   u   u   u   u   u   u   u   u   x x u   u   x u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   xuuxxxxx xxxxxxxxxxxxxxxxxxxxxxxuxxxxxxxxxxxxxxxxxxxxxxxxxxxxu xxxuxxxxxxxxxxxxxxxxxxxxxxxxxxxuuuu') # Separator
data.append(r'u   u   u   u   u   u   u   u   u   m N u   u   N u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   mmmmmmmm mmVmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmm mmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmvmu') # MultilineComment
data.append(r'u   u   u   u   u   u   u   u   u   m N u   u   N u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   mmmmmmmm mvVmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmm mmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmvmu') # MultilineCommentAsterisk
data.append(r'u   u   u   u   u   u   u   u   u   x x u   u   x u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   xuuxxxxx xxxxxxxxxxxxxxxxxxxxxxxuxxxxxxxxxxxxxxxxxxxxxxxxxxxxu xxxuxxxxxxxxxxxxxxxxxxxxxxxxxxxuuuu') # MultilineCommentEnd
data.append(r'x   u   u   u   u   u   u   u   u   m N u   u   N u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   mmmmmmmm mmVmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmm mmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmvmu') # MultilineCommentNewLine
rdata = []
for i in data:
    rdata.append(['{'])
    for j in i:
        if j == ' ':
            continue
        rdata[-1].append('State.' + d[j] + ',')
    rdata[-1].append('},')

print(tabulate(rdata, tablefmt='plain'), file=open('table.txt', 'w'))
