--����
local project="NReport"
local shost="10.34.130.62"
local spath="Groups\\NReport"
local lpath="D:\\NHN\\ECSTOOL20130108\\ECSTOOL\\ECSTOOL\\bin\\Debug\\Groups\\NReport"
local rpath="D:\\NHN\\ECSTOOL20130108\\ECSTOOL\\ECSTOOL\\bin\\Debug\\Temp\\NReport.rar"
local lhost="127.0.0.1"
local rar="%ProgramFiles%/WinRAR/WinRAR.exe"
--����ϵͳping����ȴ�ʱ��
function sleep(n)
  if n > 0 then os.execute("ping -n " .. tonumber(n + 1) .. " localhost > NUL") end
end
--����
ExecCommand(lhost..":win_cmd{@del /f /q \""..rpath.."\"@}")
ExecCommand(shost..":win_cmd{@cd \""..spath.."\" && del /f /q "..project..".rar@}")
--ѹ��
ExecCommand(lhost..":win_cmd{@cd \""..lpath.."\" && \""..rar.."\" a -r -o+ \""..rpath.."\" *@}")
sleep(10)
--�ϴ�
ExecCommand(lhost..":send_windows_file{@"..rpath..">"..shost.."?"..spath.."@}")
sleep(10)
--��ѹ
ExecCommand(shost..":win_cmd{@cd \""..spath.."\" && \""..rar.."\" x -r -o+ "..project..".rar .@}")
sleep(10)
return "Completed"