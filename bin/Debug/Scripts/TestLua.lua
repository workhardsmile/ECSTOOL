--内置ExecCommand(string)函数示例:执行dos和shell命令
ExecCommand("localhost,127.0.0.1:win_cmd{@cd .. && dir@}{@c: && dir@}")
ExecCommand("42.121.98.7:linux_shell{@cd .. && ls -al@}{@netstat -ant|grep 22@}")
--内置主窗体对象MyForm
str = MyForm.scriptText
--脚本入口和出口函数EnterBegin()
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