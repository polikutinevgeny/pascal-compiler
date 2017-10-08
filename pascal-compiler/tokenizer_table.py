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
table = []
            #r'\x00\x01\x02\x03\x04\x05\x06\x07\x08\t\n\x0b\x0c\r\x0e\x0f\x10\x11\x12\x13\x14\x15\x16\x17\x18\x19\x1a\x1b\x1c\x1d\x1e\x1f !"#$%&\'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~\x7f'
table.append(r'x   u   u   u   u   u   u   u   u   x l u   u   x u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   xuu#hbas pS*+S-./IIIIIIIIII:S<O>uOiiiiiiiiiiiiiiiiiiiiiiiiiiSu SOiuiiiiiiiiiiiiiiiiiiiiiiiiiimuuuu') # Start
table.append(r'x   u   u   u   u   u   u   u   u   x x u   u   x u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   xuuxxxxx xxxxxxxxiiiiiiiiiixxxxxuxiiiiiiiiiiiiiiiiiiiiiiiiiixu xxiuiiiiiiiiiiiiiiiiiiiiiiiiiixuuuu') # Identifier
table.append(r'x   u   u   u   u   u   u   u   u   x l u   u   x u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   xuuxxxxx xxxxxxxxxxxxxxxxxxxxxxxuxxxxxxxxxxxxxxxxxxxxxxxxxxxxu xxxuxxxxxxxxxxxxxxxxxxxxxxxxxxxuuuu') # NewLine
table.append(r'u   u   u   u   u   u   u   u   u   s u u   u   u u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   sssssssA sssssssssssssssssssssssssssssssssssssssssssssssssssss ssssssssssssssssssssssssssssssssssu') # StringLiteral
table.append(r'x   u   u   u   u   u   u   u   u   x x u   u   x u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   xxx#xxxs xxxxxxxxxxxxxxxxxxxxxxxuxxxxxxxxxxxxxxxxxxxxxxxxxxxxx xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxu') # StringLiteralClosed
table.append(r'u   u   u   u   u   u   u   u   u   u u u   u   u u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   uuuu$%&u uuuuuuuuDDDDDDDDDDuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuu uuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuu') # StringLiteralCharStart
table.append(r'x   u   u   u   u   u   u   u   u   x x u   u   x u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   xuu#xxxs xxxxxxxxDDDDDDDDDDxxxxxuxxxxxxxxxxxxxxxxxxxxxxxxxxxxx xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx') # StringLiteralCharDec
table.append(r'u   u   u   u   u   u   u   u   u   u u u   u   u u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   uuuuuuuu uuuuuuuuHHHHHHHHHHuuuuuuuHHHHHHuuuuuuuuuuuuuuuuuuuuuu uuuuHHHHHHuuuuuuuuuuuuuuuuuuuuuuuuu') # StringLiteralCharHex
table.append(r'x   u   u   u   u   u   u   u   u   x x u   u   x u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   xuu#xxxs xxxxxxxxHHHHHHHHHHxxxxxuxHHHHHHxxxxxxxxxxxxxxxxxxxxxx xxxxHHHHHHxxxxxxxxxxxxxxxxxxxxxxxxx') # StringLiteralCharHexValue
table.append(r'u   u   u   u   u   u   u   u   u   u u u   u   u u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   uuuuuuuu uuuuuuuuQQQQQQQQuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuu uuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuu') # StringLiteralCharOct
table.append(r'x   u   u   u   u   u   u   u   u   x x u   u   x u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   xuu#xxxs xxxxxxxxQQQQQQQQuuxxxxxuxxxxxxxxxxxxxxxxxxxxxxxxxxxxx xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx') # StringLiteralCharOctValue
table.append(r'u   u   u   u   u   u   u   u   u   u u u   u   u u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   uuuuuuuu uuuuuuuuBBuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuu uuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuu') # StringLiteralCharBin
table.append(r'x   u   u   u   u   u   u   u   u   x x u   u   x u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   xuu#xxxs xxxxxxxxBBxxxxxxxxxxxxxuxxxxxxxxxxxxxxxxxxxxxxxxxxxxx xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx') # StringLiteralCharBinValue
           #r'\x00\x01\x02\x03\x04\x05\x06\x07\x08\t\n\x0b\x0c\r\x0e\x0f\x10\x11\x12\x13\x14\x15\x16\x17\x18\x19\x1a\x1b\x1c\x1d\x1e\x1f !"#$%&\'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~\x7f'
table.append(r'x   u   u   u   u   u   u   u   u   x x u   u   x u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   xuuxxxxx xxxxxxdxIIIIIIIIIIxxxxxuxxxxxxxxxxxxxxxxxxxxxxxxxxxxu xxxuxxxxxxxxxxxxxxxxxxxxxxxxxxxuuuu') # Integer
table.append(r'u   u   u   u   u   u   u   u   u   u u u   u   u u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   uuuuuuuu uxuuuuxuffffffffffuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuu uuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuu') # FloatDot
table.append(r'x   u   u   u   u   u   u   u   u   x x u   u   x u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   xuuxxxxx xxxxxxxxffffffffffxxxxxuxxxxxexxxxxxxxxxxxxxxxxxxxxxu xxxuxxxxexxxxxxxxxxxxxxxxxxxxxxuuuu') # FloatFrac
table.append(r'u   u   u   u   u   u   u   u   u   u u u   u   u u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   uuuuuuuu uuuPuMuuEEEEEEEEEEuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuu uuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuu') # FloatExp
table.append(r'u   u   u   u   u   u   u   u   u   u u u   u   u u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   uuuuuuuu uuuuuuuuEEEEEEEEEEuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuu uuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuu') # FloatExpPlus
table.append(r'u   u   u   u   u   u   u   u   u   u u u   u   u u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   uuuuuuuu uuuuuuuuEEEEEEEEEEuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuu uuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuu') # FloatExpMinus
table.append(r'x   u   u   u   u   u   u   u   u   x x u   u   x u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   xuuxxxxx xxxxxxuxEEEEEEEEEExxxxxuxxxxxxxxxxxxxxxxxxxxxxxxxxxxu xxxuxxxxxxxxxxxxxxxxxxxxxxxxxxxuuuu') # FloatExpValue
           #r'\x00\x01\x02\x03\x04\x05\x06\x07\x08\t\n\x0b\x0c\r\x0e\x0f\x10\x11\x12\x13\x14\x15\x16\x17\x18\x19\x1a\x1b\x1c\x1d\x1e\x1f !"#$%&\'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~\x7f'
table.append(r'u   u   u   u   u   u   u   u   u   u u u   u   u u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   uuuuuuuu uuuuuuuuoooooooouuuuuuuuuiiiiiiiiiiiiiiiiiiiiiiiiiiuu uuiuiiiiiiiiiiiiiiiiiiiiiiiiiiuuuuu') # Ampersand
table.append(r'x   u   u   u   u   u   u   u   u   x x u   u   x u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   xuuxxxxx xxxxxxxxoooooooouuxxxxxuxxxxxxxxxxxxxxxxxxxxxxxxxxxxu xxxuxxxxxxxxxxxxxxxxxxxxxxxxxxxuuuu') # OctNumber
table.append(r'u   u   u   u   u   u   u   u   u   u u u   u   u u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   uuuuuuuu uuuuuuuuшшшшшшшшшшuuuuuuuшшшшшшuuuuuuuuuuuuuuuuuuuuuu uuuuшшшшшшuuuuuuuuuuuuuuuuuuuuuuuuu') # HexNumber
table.append(r'x   u   u   u   u   u   u   u   u   x x u   u   x u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   xuuxxxxx xxxxxxxxшшшшшшшшшшxxxxxuxшшшшшшxxxxxxxxxxxxxxxxxxxxxu xxxuшшшшшшxxxxxxxxxxxxxxxxxxxxxuuuu') # HexNumberValue
table.append(r'u   u   u   u   u   u   u   u   u   u u u   u   u u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   uuuuuuuu uuuuuuuuддuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuu uuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuu') # BinNumber
table.append(r'x   u   u   u   u   u   u   u   u   x x u   u   x u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   xuuxxxxx xxxxxxxxддuuuuuuuuxxxxxuxxxxxxxxxxxxxxxxxxxxxxxxxxxxu xxxuxxxxxxxxxxxxxxxxxxxxxxxxxxxuuuu') # BinNumberValue
           #r'\x00\x01\x02\x03\x04\x05\x06\x07\x08\t\n\x0b\x0c\r\x0e\x0f\x10\x11\x12\x13\x14\x15\x16\x17\x18\x19\x1a\x1b\x1c\x1d\x1e\x1f !"#$%&\'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~\x7f'
table.append(r'x   u   u   u   u   u   u   u   u   x x u   u   x u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   xuuxxxxx xxxxxxxcxxxxxxxxxxxxxOxuxxxxxxxxxxxxxxxxxxxxxxxxxxxxu xxxuxxxxxxxxxxxxxxxxxxxxxxxxxxxuuuu') # SlashOperator
table.append(r'x   u   u   u   u   u   u   u   u   c x u   u   x u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   cccccccc ccccccccccccccccccccccccccccccccccccccccccccccccccccc ccccccccccccccccccccccccccccccccccu') # Comment
table.append(r'x   u   u   u   u   u   u   u   u   x x u   u   x u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   xuuxxxxx xxxxxxxxxxxxxxxxxxxxxOxuxxxxxxxxxxxxxxxxxxxxxxxxxxxxu xxxuxxxxxxxxxxxxxxxxxxxxxxxxxxxuuuu') # PlusOperator
table.append(r'x   u   u   u   u   u   u   u   u   x x u   u   x u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   xuuxxxxx xxOxxxxxxxxxxxxxxxxxxOxuxxxxxxxxxxxxxxxxxxxxxxxxxxxxu xxxuxxxxxxxxxxxxxxxxxxxxxxxxxxxuuuu') # AsteriskOperator
table.append(r'x   u   u   u   u   u   u   u   u   x x u   u   x u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   xuuxxxxx xxxxxxxxxxxxxxxxxxxxxOxuxxxxxxxxxxxxxxxxxxxxxxxxxxxxu xxxuxxxxxxxxxxxxxxxxxxxxxxxxxxxuuuu') # MinusOperator
table.append(r'x   u   u   u   u   u   u   u   u   x x u   u   x u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   xuuxxxxx xxxxxxxxxxxxxxxxxxxxOOOuxxxxxxxxxxxxxxxxxxxxxxxxxxxxu xxxuxxxxxxxxxxxxxxxxxxxxxxxxxxxuuuu') # Less
table.append(r'x   u   u   u   u   u   u   u   u   x x u   u   x u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   xuuxxxxx xxxxxxxxxxxxxxxxxxxxOOOuxxxxxxxxxxxxxxxxxxxxxxxxxxxxu xxxuxxxxxxxxxxxxxxxxxxxxxxxxxxxuuuu') # More
table.append(r'x   u   u   u   u   u   u   u   u   x x u   u   x u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   xuuxxxxx xxxxxxxxxxxxxxxxxxxxxxxuxxxxxxxxxxxxxxxxxxxxxxxxxxxxu xxxuxxxxxxxxxxxxxxxxxxxxxxxxxxxuuuu') # Operator
table.append(r'x   u   u   u   u   u   u   u   u   x x u   u   x u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   xuuxxxxx xxxxxxxxxxxxxxxxxxxxxOxuxxxxxxxxxxxxxxxxxxxxxxxxxxxxu xxxuxxxxxxxxxxxxxxxxxxxxxxxxxxxuuuu') # Colon
           #r'\x00\x01\x02\x03\x04\x05\x06\x07\x08\t\n\x0b\x0c\r\x0e\x0f\x10\x11\x12\x13\x14\x15\x16\x17\x18\x19\x1a\x1b\x1c\x1d\x1e\x1f !"#$%&\'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~\x7f'
table.append(r'x   u   u   u   u   u   u   u   u   x x u   u   x u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   xuuxxxxx xxmxxxSxxxxxxxxxxxxxxxxuxxxxxxxxxxxxxxxxxxxxxxxxxxxxu xxxuxxxxxxxxxxxxxxxxxxxxxxxxxxxuuuu') # Parenthesis
table.append(r'x   u   u   u   u   u   u   u   u   x x u   u   x u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   xuuxxxxx xSxxxxOxxxxxxxxxxxxxxxxuxxxxxxxxxxxxxxxxxxxxxxxxxxxxu xxxuxxxxxxxxxxxxxxxxxxxxxxxxxxxuuuu') # DotOperator
table.append(r'x   u   u   u   u   u   u   u   u   x x u   u   x u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   xuuxxxxx xxxxxxxxxxxxxxxxxxxxxxxuxxxxxxxxxxxxxxxxxxxxxxxxxxxxu xxxuxxxxxxxxxxxxxxxxxxxxxxxxxxxuuuu') # Separator
table.append(r'u   u   u   u   u   u   u   u   u   m N u   u   m u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   mmmmmmmm mmVmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmm mmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmvmu') # MultilineComment
table.append(r'u   u   u   u   u   u   u   u   u   m N u   u   m u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   mmmmmmmm mvVmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmm mmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmvmu') # MultilineCommentAsterisk
table.append(r'x   u   u   u   u   u   u   u   u   x x u   u   x u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   xuuxxxxx xxxxxxxxxxxxxxxxxxxxxxxuxxxxxxxxxxxxxxxxxxxxxxxxxxxxu xxxuxxxxxxxxxxxxxxxxxxxxxxxxxxxuuuu') # MultilineCommentEnd
table.append(r'u   u   u   u   u   u   u   u   u   m N u   u   m u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   u   mmmmmmmm mmVmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmm mmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmvmu') # MultilineCommentNewLine

new_table = []
for i in table:
    new_table.append(['{'])
    for j in i:
        if j == ' ':
            continue
        new_table[-1].append('State.' + d[j] + ',')
    new_table[-1].append('},')

print(tabulate(new_table, tablefmt='plain'), file=open('table.txt', 'w'))
