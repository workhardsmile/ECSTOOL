--����ExecCommand(string)����ʾ��:ִ��dos��shell����
ExecCommand("localhost,127.0.0.1:win_cmd{@cd .. && dir@}{@c: && dir@}")
ExecCommand("42.121.98.7:linux_shell{@cd .. && ls -al@}{@netstat -ant|grep 22@}")
--�������������MyForm
str = MyForm.scriptText
--�ű���ںͳ��ں���EnterBegin()
function EnterBegin()
  local i = 0
  local sum=0
  while (i <= 100) do
    --coroutine.yield( i )
    sum = sum + i
    i = i + 1
  end
  if str ~= "" then
    return "{\n"..str.."\n}\nresult:"..sum
  else
    return "WuGang: Hello World!",54321,"END"
  end
end
return "WuGang: Hello World!",54321,"END"