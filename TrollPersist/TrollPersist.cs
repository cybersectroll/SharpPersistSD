using System;
using System.Management;
using System.Security.AccessControl;
using Microsoft.Win32;
using static TrollPersist.SvcHelper;
using static TrollPersist.RegHelper;


namespace TrollPersist
{
    public static class SecurityDescriptor
    {
        public static void WMI_ModifyWMISD(string hostname, string SDDL)
        {

            try
            {

                Console.WriteLine("[+] Using WMI to set WMI SD");

                ManagementClass mc = new ManagementClass(@"\\" + hostname + @"\ROOT\cimv2:__SystemSecurity");

                ManagementBaseObject outParams = mc.InvokeMethod("GetSD", null, null);
                byte[] BinarySD = (byte[])outParams["SD"];
                string oSDDL = BinarySDToSDDL(BinarySD);
                string NewSDDL = oSDDL + SDDL;

                byte[] NewBinarySD = SDDLToBinarySD(NewSDDL);

                ManagementBaseObject inParams = mc.GetMethodParameters("SetSD");
                inParams["SD"] = NewBinarySD;
                outParams = mc.InvokeMethod("SetSD", inParams, null);

                Console.WriteLine("[+] WMI Return Value: " + outParams["ReturnValue"]);

            }
            catch (Exception e)
            {
                Console.WriteLine("[!] Catch Entered: Something went wrong.. printing error");
                Console.WriteLine("[!]" + e.ToString());
            }

        }

        public static void REG_ModifyRegistryPermissions(string hostname, string subkey, string principal, bool propogate)
        {

            try
            {

                Console.WriteLine("[+] Using RemoteRegistry to set Registry Permissions");

                RegistryKey key = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, hostname).OpenSubKey(subkey, true);
                if (key == null)
                {
                    Console.WriteLine("[!] Specified Key does not exist.. exiting early");
                    return;
                }

                RegistrySecurity rs = new RegistrySecurity();
                rs = key.GetAccessControl(AccessControlSections.All);
                rs.SetAccessRuleProtection(true, true);

                Console.WriteLine("[+] ---------------Current Access Rules---------------");
                ShowSecurity(rs);

                if (propogate)
                {
                    rs.AddAccessRule(
                        new RegistryAccessRule(
                             principal,
                             RegistryRights.FullControl,  //Change to relevant permissions
                             InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit,
                              PropagationFlags.InheritOnly,
                             AccessControlType.Allow
                        )
                    );

                }
                else
                {
                    rs.AddAccessRule(
                           new RegistryAccessRule(
                                principal,
                                RegistryRights.FullControl,  //Change to relevant permissions
                                AccessControlType.Allow
                           )
                     );

                }

                Console.WriteLine("[+] Adding new rules.. might be a bit slow..");
                key.SetAccessControl(rs);

                Console.WriteLine("[+] ---------------New Access Rules---------------");
                ShowSecurity(rs);

                Console.WriteLine("[+] Registry Permissions action successfully completed.");
            }
            catch (Exception e)
            {
                Console.WriteLine("Catch Entered: Something went wrong");
                Console.WriteLine(e.ToString());
            }

        }


        public static void REG_ModifyRegistryContainingSD(string type, string hostname, string SDDL, string sBaseKey, string value)
        {
            try
            {
                Console.WriteLine("[+] Using RemoteRegistry to set Registry Permissions");

                RegistryKey HKLMHive = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, hostname);

                bool exists = false;

                if (HKLMHive.OpenSubKey(sBaseKey, true) != null) { exists = true; }


                if (exists)
                {
                    //key already exists
                    Console.WriteLine("[+] Key exists.. proceeding.");


                    RegistryKey FinalKey = HKLMHive.OpenSubKey(sBaseKey, true);

                    byte[] obdata = (byte[])FinalKey.GetValue(value);

                    string oSDDL = BinarySDToSDDL(obdata);

                    string cSDDL = ConvertSDDLAppend(oSDDL, SDDL, type);

                    byte[] nbdata = SDDLToBinarySD(cSDDL);


                    FinalKey.SetValue(value, nbdata, RegistryValueKind.Binary);
                }
                else
                {
                    //key doesnt exists 

                    RegistryKey FinalKey = HKLMHive.CreateSubKey(sBaseKey, true);

                    string cSDDL = ConvertSDDLCreate(SDDL, type);

                    byte[] nbdata = SDDLToBinarySD(cSDDL);

                    FinalKey.SetValue(value, nbdata, RegistryValueKind.Binary);


                }

                Console.WriteLine("[+] Registry Permissions action successfully completed.");


            }
            catch (Exception e)
            {
                Console.WriteLine("[!] Catch Entered: Something went wrong, printing error message.");
                Console.WriteLine("[!] " + e.ToString());
            }

            return;
        }


        public static void REG_CreateRegKey(string hostname, string key, string value, object data, int REG_TYPE)
        {
            try
            {
                Console.WriteLine("[+] Using RemoteRegistry to Create Key, Value, Data");
                RegistryKey lastkey = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, hostname).OpenSubKey(key, true);
                lastkey.SetValue(value, data, (RegistryValueKind)REG_TYPE);

                Console.WriteLine("[+] Successfully Completed REG_CreateRegKey");
            }
            catch (Exception e)
            {
                Console.WriteLine("[!] Catch Entered: Something went wrong, printing error message.");
                Console.WriteLine("[!] " + e.ToString());
            }
            return;
        }


        public static void SCM_CreateAndStart(string hostname, string servicename, string binarypath)
        {
            try
            {
                Console.WriteLine("[+] Using SVC protocol to Create and Start Service");

                IntPtr SCMHandle = OpenSCManager(hostname, null, (uint)SCM_ACCESS.SC_MANAGER_ALL_ACCESS);

                Console.WriteLine("[+] SCMHandle: 0x{0}", SCMHandle);

                IntPtr svchandle = CreateServiceA(
                    SCMHandle,
                    servicename,
                    servicename,        //optional DisplayName
                    (uint)SERVICE_ACCESS.SERVICE_ALL_ACCESS,
                    0x10,        //SERVICE_WIN32_OWN_PROCESS
                    2,           //start_auto
                    1,           //errorcontrol
                    binarypath,  //binarypath
                    null,        //optional
                    null,        //optional
                    null,        //optional
                    null,        //RunAs  (null = localsystem)
                    null         //optional
                 );
                Console.WriteLine("[+] SVChandle: 0x{0}", svchandle);

                bool bResult = StartService(svchandle, 0, null);
                uint dwResult = GetLastError();
                if (!bResult && dwResult != 1053)
                {
                    Console.WriteLine("[!] StartServiceA failed to start the service. Error:{0}", GetLastError());
                }
                else
                {
                    Console.WriteLine("[+] Service was started");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("[!] Something went wrong..printing error message.");
                Console.WriteLine(e.ToString());
            }

        }

        public static void SVC_ModifyAndStart(string hostname, string servicename, string BinaryPathName)
        {

            try
            {

                Console.WriteLine("[+] Using SVC protocol to Modify and Start Service");

                IntPtr SCMHandle = OpenSCManager(hostname, null, (uint)SCM_ACCESS.SC_MANAGER_CONNECT);
                Console.WriteLine("[+] SCMHANDLE: 0x{0}", SCMHandle);

                IntPtr svchandle = OpenService(SCMHandle, servicename, ((uint)SERVICE_ACCESS.SERVICE_START | (uint)SERVICE_ACCESS.SERVICE_CHANGE_CONFIG | (uint)SERVICE_ACCESS.SERVICE_STOP));
                Console.WriteLine("[*] SVCHANDLE: 0x{0}", svchandle);

                if (svchandle == IntPtr.Zero)
                {
                    Console.WriteLine("[!] OpenService failed! Error:{0}", GetLastError());
                    Console.WriteLine("[!] Exiting early..");
                    return;

                }

                int configRet = ChangeServiceConfigA(
                   svchandle,        //IntPtr hService,
                   0x10,             //SERVICE_WIN32_OWN_PROCESS
                   2,                //start_auto
                   1,                //errorcontrol
                   BinaryPathName,   // string lpBinaryPathName,
                   null,    // string lpLoadOrderGroup,
                   null,    // string lpdwTagId,
                   null,    // string lpDependencies,
                   null,    // string lpServiceStartName,
                   null,    // string lpPassword,
                   null     // string lpDisplayName)
                 );

                if (configRet == 0)
                {
                    Console.WriteLine("[!] Change Service Config failed! Error:{0}", GetLastError());
                    Console.WriteLine("[!] Exiting early..");
                    return;
                }

                bool bResult = StartService(svchandle, 0, null);
                uint dwResult = GetLastError();

                if (dwResult == 1056)
                {
                    Console.WriteLine("[!] Stopping Service first");
                    Console.WriteLine("[!] Waiting 30 seconds for service to stop fully..");
                    System.Threading.Thread.Sleep(30000);
                    SERVICE_STATUS stat = new SERVICE_STATUS();
                    ControlService(svchandle, 0x1, ref stat);

                    Console.WriteLine("Get Last Error: " + GetLastError());

                    StartService(svchandle, 0, null);

                    Console.WriteLine("[!] Trying to start service");
                    bResult = StartService(svchandle, 0, null);
                    dwResult = GetLastError();
                    if (!bResult && dwResult != 1053)
                    {
                        Console.WriteLine("[!] StartServiceA failed to start the service. Error:{0}", GetLastError());
                    }
                    else
                    {
                        Console.WriteLine("[*] Service was started");
                    }
                }
                else if (!bResult && dwResult != 1053)
                {
                    Console.WriteLine("[!] StartServiceA failed to start the service. Error:{0}", GetLastError());
                }
                else
                {
                    Console.WriteLine("[*] Service was started");
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

        }


    }


}
