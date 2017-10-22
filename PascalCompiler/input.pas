program imya;
var
   i: integer;
   a: float;
   arr: array[0..9] of Integer;
const
    c = 10.1;
begin
   for i := 0 to 10 do
      for i := 0 to 3 do
      begin
         a := 34.5;
         if a >= 35 then
            a := 33
         else
            a := a - i * - i;
      end;
end.