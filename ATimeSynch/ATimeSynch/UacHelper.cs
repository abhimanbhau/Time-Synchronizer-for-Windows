using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;
using Microsoft.Win32;

namespace ATimeSynch
{
    public static class UacHelper
    {
        public enum TokenEvaluationType
        {
            TokenElevationTypeDefault = 1,
            TokenElevationTypeFull,
            TokenElevationTypeLimited
        }

        public enum TokenInformationClass
        {
            TokenUser = 1,
            TokenGroups,
            TokenPrivileges,
            TokenOwner,
            TokenPrimaryGroup,
            TokenDefaultDacl,
            TokenSource,
            TokenType,
            TokenImpersonationLevel,
            TokenStatistics,
            TokenRestrictedSids,
            TokenSessionId,
            TokenGroupsAndPrivileges,
            TokenSessionReference,
            TokenSandBoxInert,
            TokenAuditPolicy,
            TokenOrigin,
            TokenElevationType,
            TokenLinkedToken,
            TokenElevation,
            TokenHasRestrictions,
            TokenAccessInformation,
            TokenVirtualizationAllowed,
            TokenVirtualizationEnabled,
            TokenIntegrityLevel,
            TokenUiAccess,
            TokenMandatoryPolicy,
            TokenLogonSid,
            MaxTokenInfoClass
        }

        private const string UacRegistryKey = "Software\\Microsoft\\Windows\\CurrentVersion\\Policies\\System";
        private const string UacRegistryValue = "EnableLUA";

        private const uint StandardRightsRead = 0x00020000;
        private const uint TokenQuery = 0x0008;
        private const uint TokenRead = (StandardRightsRead | TokenQuery);

        public static bool IsUacEnabled
        {
            get
            {
                RegistryKey uacKey = Registry.LocalMachine.OpenSubKey(UacRegistryKey, false);
                bool result = uacKey != null && uacKey.GetValue(UacRegistryValue).Equals(1);
                return result;
            }
        }

        public static bool IsProcessElevated
        {
            get
            {
                if (IsUacEnabled)
                {
                    IntPtr tokenHandle;
                    if (!OpenProcessToken(Process.GetCurrentProcess().Handle, TokenRead, out tokenHandle))
                    {
                        throw new ApplicationException("Could not get process token.  Win32 Error Code: " +
                                                       Marshal.GetLastWin32Error());
                    }

                    var elevationResult = TokenEvaluationType.TokenElevationTypeDefault;

                    int elevationResultSize = Marshal.SizeOf((int) elevationResult);
                    uint returnedSize;
                    IntPtr elevationTypePtr = Marshal.AllocHGlobal(elevationResultSize);

                    bool success = GetTokenInformation(tokenHandle, TokenInformationClass.TokenElevationType,
                        elevationTypePtr, (uint) elevationResultSize, out returnedSize);
                    if (success)
                    {
                        elevationResult = (TokenEvaluationType) Marshal.ReadInt32(elevationTypePtr);
                        bool isProcessAdmin = elevationResult == TokenEvaluationType.TokenElevationTypeFull;
                        return isProcessAdmin;
                    }
                    throw new ApplicationException("Unable to determine the current elevation.");
                }
                WindowsIdentity identity = WindowsIdentity.GetCurrent();
                bool result = false;
                if (identity != null)
                {
                    var principal = new WindowsPrincipal(identity);
                    result = principal.IsInRole(WindowsBuiltInRole.Administrator);
                }
                return result;
            }
        }

        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool OpenProcessToken(IntPtr processHandle, UInt32 desiredAccess, out IntPtr tokenHandle);

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool GetTokenInformation(IntPtr tokenHandle, TokenInformationClass tokenInformationClass,
            IntPtr tokenInformation, uint tokenInformationLength, out uint returnLength);
    }
}