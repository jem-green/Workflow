sc config "workflow" start= delayed-auto
sc config "workflow" obj= %COMPUTERNAME%\administrator password= C0mplex2B0ld
ntrights +r SeServiceLogonRight -u %COMPUTERNAME%\administrator -m \\%COMPUTERNAME%
Net Start workflow service
pause