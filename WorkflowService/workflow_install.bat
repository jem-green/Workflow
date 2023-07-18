C:
cd \
cd "C:\program files\workflow"
C:\Windows\Microsoft.NET\Framework\v4.0.30319\installutil workflowservice.exe
C:\WINDOWS\Microsoft.NET\Framework\v2.0.50727\installutil workflowservice.exe
sc config "workflow" start= delayed-auto
sc config "workflow" obj= %COMPUTERNAME%\administrator password= C0mplex2B0ld
ntrights +r SeServiceLogonRight -u %COMPUTERNAME%\administrator -m \\%COMPUTERNAME%
Net Start workflow service
pause
