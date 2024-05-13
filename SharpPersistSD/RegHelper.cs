using System;
using System.Management;
using System.Security.AccessControl;
using System.Security.Principal;


namespace SharpPersistSD
{
    public class RegHelper
    {

        public static string ConvertSDDLAppend(string SDDL, string aSDDL, string type)
        {
            string returnString = "";

            if (type == "SCM")
            {
                returnString = "O:SYG:SYD:" + aSDDL + SDDL.Remove(0, 10);
            }
            else if (type == "SVC")
            {

                returnString = "O:SYG:SYD:" + aSDDL + SDDL.Remove(0, 10);
            }
            else if (type == "DCOM")
            {
                returnString = SDDL + aSDDL;
            }

            return returnString;

        }

        public static string ConvertSDDLCreate(string aSDDL, string type)
        {

            string defaultSCMSDDL = @"O:SYG:SYD:";
            string defaultSVCSDDL = @"O:SYG:SYD:";

            string returnString = "";

            if (type == "SCM")
            {
                returnString = "O:SYG:SYD:" + aSDDL + defaultSCMSDDL.Remove(0, 10);
            }
            else if (type == "SVC")
            {

                returnString = "O:SYG:SYD:" + aSDDL + defaultSVCSDDL.Remove(0, 10);
            }

            return returnString;

        }

        public static string ConvertPrincipalToSID(string principal)
        {
            
            NTAccount account = new NTAccount(principal);
            SecurityIdentifier data = (SecurityIdentifier)account.Translate(typeof(SecurityIdentifier));

            Console.WriteLine("[+] Converted " + principal + " to SID:" + data.ToString());

            return data.ToString();

        }

        public static byte[] SDDLToBinarySD(string SDDL)
        {
            Console.WriteLine("[+] NewSDDL: " + SDDL);

            ManagementClass mc = new ManagementClass("Win32_SecurityDescriptorHelper");
            ManagementBaseObject inParams = mc.GetMethodParameters("SDDLToBinarySD");
            inParams["SDDL"] = SDDL;
            ManagementBaseObject outParams = mc.InvokeMethod("SDDLToBinarySD", inParams, null);
            byte[] Data = (byte[])outParams["BinarySD"];
            Console.WriteLine("[+] Converted New SDDL to byte array of length: " + Data.Length);
            return Data;
        }

        public static string BinarySDToSDDL(byte[] BinarySD)
        {
            Console.WriteLine("[+] Converted Old SDDL to byte array of length: " + BinarySD.Length);

            ManagementClass mc = new ManagementClass("Win32_SecurityDescriptorHelper");
            ManagementBaseObject inParams = mc.GetMethodParameters("BinarySDToSDDL");
            inParams["BinarySD"] = BinarySD;
            ManagementBaseObject outParams = mc.InvokeMethod("BinarySDToSDDL", inParams, null);
            string SDDL = (string)outParams["SDDL"];
            Console.WriteLine("[+] OldSDDL: " + SDDL);
            return SDDL;
        }


        public static void ShowSecurity(RegistrySecurity security)
        {
            
            foreach (RegistryAccessRule ar in
                security.GetAccessRules(true, true, typeof(NTAccount)))
            {
                Console.WriteLine("        User: {0}", ar.IdentityReference);
                Console.WriteLine("        Type: {0}", ar.AccessControlType);
                Console.WriteLine("      Rights: {0}", ar.RegistryRights);
                Console.WriteLine();
            }
        }

        public static byte[] HexToBin(string hex)
        {
            var result = new byte[hex.Length / 2];
            for (int i = 0; i < hex.Length; i += 2)
            {
                result[i / 2] = byte.Parse(hex.Substring(i, 2), System.Globalization.NumberStyles.HexNumber);
            }
            return result;
        }


    }
}
