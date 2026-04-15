C:
cd \
cd "C:\program files\workflow"
sc.exe create "workflow service" binpath= "c:\program files\workflow\WorkflowService.exe"
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\installutil workflowservice.exe
C:\Windows\Microsoft.NET\Framework\v4.0.30319\installutil workflowservice.exe
C:\WINDOWS\Microsoft.NET\Framework\v2.0.50727\installutil workflowservice.exe
sc config "workflow Service" start= delayed-auto
sc config "workflow Serivce" obj= %COMPUTERNAME%\administrator password= C0mplex2B0ld
ntrights +r SeServiceLogonRight -u %COMPUTERNAME%\administrator -m \\%COMPUTERNAME%
Net Start "workflow service"
pause
