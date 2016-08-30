--require("socket")
--function sleep(n)   
-- socket.select(nil, nil, n) 
--end
--利用系统ping命令等待时间
function sleep(n) 
  if n > 0 then os.execute("ping -n " .. tonumber(n + 1) .. " localhost > NUL") end 
end
ExecCommand("127.0.0.1:connect_one_server{@10.34.130.44@}")
while true do 
  sleep(1)
  ExecCommand("127.0.0.1:get_status_server{@10.34.130.44@}")
  --寻找子字符串，可用正则
  local s, e = string.find(string.lower(MyForm.hReturn[MyForm.hReturn.Count-1]),"true",1)
  if (e ~= nil) and (s ~= nil) and (e > s) then 
     x = string.sub(MyForm.hReturn[MyForm.hReturn.Count-1], s, e) 
     break 
  end  
end
ExecCommand("10.34.130.44:linux_shell{@ls@}")
return x,MyForm.hReturn[MyForm.hReturn.Count-1]
