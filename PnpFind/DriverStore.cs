﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace PnpFind
{
    public class DriverStore
    {
        private const int INF_STYLE_OLDNT = 0x00000001;
        private const int INF_STYLE_WIN4 = 0x00000002;
        private const long INVALID_HANDLE_VALUE = -1;

        public Dictionary<string, string> OemInfEntities = new Dictionary<string, string>();

        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SetupOpenInfFile([MarshalAs(UnmanagedType.LPTStr)] string FileName,
            [MarshalAs(UnmanagedType.LPTStr)] string InfClass, int InfStyle, out uint ErrorLine);

        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetupFindFirstLine(IntPtr InfHandle, [MarshalAs(UnmanagedType.LPTStr)] string Section,
            [MarshalAs(UnmanagedType.LPTStr)] string Key, ref INFCONTEXT Context);

        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetupFindNextLine(ref INFCONTEXT ContextIn, out INFCONTEXT ContextOut);

        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetupGetStringField(ref INFCONTEXT Context, int FieldIndex,
            [MarshalAs(UnmanagedType.LPTStr)] string ReturnBuffer,
            int ReturnBufferSize, out int RequiredSize);

        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetupGetStringField(ref INFCONTEXT Context, int FieldIndex,
            [MarshalAs(UnmanagedType.LPTStr)] StringBuilder ReturnBuffer,
            int ReturnBufferSize, out int RequiredSize);

        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern void SetupCloseInfFile(IntPtr InfHandle);

        public static int RemoveOemInf(string oemInfFileName)
        {
            var processPnpUtil = new Process
            {
                StartInfo =
                {
                    UseShellExecute = false,
                    FileName = "pnputil.exe",
                    Arguments = "-f -d " + oemInfFileName,
                    CreateNoWindow = true
                }
            };

            if (processPnpUtil.Start())
            {
                processPnpUtil.WaitForExit();
                return processPnpUtil.ExitCode;
            }

            return -1;
        }

        public static DirectoryInfo GetWindowsDirectory()
        {
            var sys32 = Environment.GetFolderPath(Environment.SpecialFolder.System);
            var diWindows = new DirectoryInfo(sys32);
            return diWindows.Parent;
        }

        public static DirectoryInfo GetWindowsInfDirectory()
        {
            return new DirectoryInfo(Path.Combine(GetWindowsDirectory().FullName, "inf"));
        }

        public static FileInfo GetOemInfFullPath(string name)
        {
            if (!name.ToLower().EndsWith(".inf"))
                name += ".inf";

            return new FileInfo(Path.Combine(GetWindowsDirectory().FullName, name));
        }

        public static List<FileInfo> GetOemInfFileList()
        {
            return new List<FileInfo>(GetWindowsInfDirectory().GetFiles("oem*.inf"));
        }

        public static bool GetInfSection(string infFile, string section,
            out Dictionary<string, List<string>> OemInfEntities)
        {
            uint ErrorLine = 0;
            OemInfEntities = new Dictionary<string, List<string>>();
            var infHandle = SetupOpenInfFile(infFile, null, INF_STYLE_OLDNT | INF_STYLE_WIN4, out ErrorLine);
            var iCode = Marshal.GetLastWin32Error();
            if (infHandle.ToInt64() != INVALID_HANDLE_VALUE)
            {
                var Context = new INFCONTEXT();
                if (SetupFindFirstLine(infHandle, section, null, ref Context))
                    do
                    {
                        var sb = new StringBuilder(1024);
                        var valueName = string.Empty;
                        var valueText = new List<string>();
                        var fieldIndex = 0;
                        int requiredSize;

                        while (SetupGetStringField(ref Context, fieldIndex++, sb, sb.Capacity, out requiredSize))
                            if (fieldIndex == 1)
                                valueName = sb.ToString(0, requiredSize - 1);
                            else
                                valueText.Add(sb.ToString(0, requiredSize - 1));
                        try
                        {
                            OemInfEntities.Add(valueName, valueText);
                        }
                        catch
                        {
                            Console.WriteLine("Skipping duplicate {0}", valueName);
                        }
                    } while (SetupFindNextLine(ref Context, out Context));
                else
                    Console.WriteLine("Can't find {0} section.", section);

                SetupCloseInfFile(infHandle);
            }
            else
            {
                Console.WriteLine("Failed to open INF file. Error code - {0}.", iCode);
                if (ErrorLine != 0) Console.WriteLine("Failure line - {0}.", ErrorLine);
            }

            return false;
        }


        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct INFCONTEXT
        {
            private readonly IntPtr Inf;
            private readonly IntPtr CurrentInf;
            private readonly uint Section;
            private readonly uint Line;
        }
    }
}