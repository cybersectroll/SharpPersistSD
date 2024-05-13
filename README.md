# TrollPersist
A Post-Compromise .NET library to embed persistency to persistency by abusing Security Descriptors of remote machines.\
The techniques incorporated are not novel but I've yet to come across a fully reflective convenient .NET library that does the following:
1. Abuse SD of Remote SCM, Remote Registry, Remote WMI
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

# Usage

## SCM 

## REG

## WMI

### library
Allow asdas;dkasd

```
[TrollPersist.SecurityDescriptor]::WMI_ModifyWMISD("Hostname","SDDL")
```

>\[TrollPersist.SecurityDescriptor]::WMI_ModifyWMISD("Hostname","SDDL")\
Allow zzzzz\
>\[TrollPersist.SecurityDescriptor]::REG_ModifyRegistryContainingSD("DCOM","Hostname","SDDL","software\microsoft\ole", "MachineLaunchRestriction")

### example

> [TrollPersist.SecurityDescriptor]::WMI_ModifyWMISD("Hostname","(A;;CCWP;;;WD)")




This is a

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
