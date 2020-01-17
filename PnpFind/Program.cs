using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandLine;

namespace PnpFind
{
    internal class Program
    {
        private static bool _deleteMode;
        private static string _matchText = string.Empty;

        private static readonly List<Dictionary<string, List<string>>> OemFileInfoSelectedList =
            new List<Dictionary<string, List<string>>>();

        private static int ParseEnumOptions(EnumOptions opts)
        {
            if (!opts.Match.HasValue)
            {
                foreach (var entry in DriverStoreEntry.Entries)
                {
                    Console.WriteLine($"{entry}\n");
                }

                return 0;
            }

            try
            {
                foreach (var item in opts.Items)
                {
                    foreach (var entry in DriverStoreEntry.Entries.Where(p =>
                        p.GetType().GetProperty(opts.Match.ToString()).GetValue(p, null).Equals(item)))
                    {
                        Console.WriteLine($"{entry}\n");
                    }
                }
            }
            catch (ArgumentException ae)
            {
                Console.WriteLine($"Error: {ae.Message}");
                return 1;
            }

            return 0;
        }

        private static int ParseDeleteOptions(DeleteOptions opts)
        {
            return 0;
        }

        private static void Main(string[] args)
        {
            Parser.Default.ParseArguments<EnumOptions, DeleteOptions>(args)
                .MapResult(
                    (EnumOptions Opts) => ParseEnumOptions(Opts),
                    (DeleteOptions Opts) => ParseDeleteOptions(Opts),
                    Errs => 1);

            return;

            StreamWriter sw = null;
            foreach (var arg in args)
                if (arg.ToLower().StartsWith("/out:"))
                {
                    sw = File.CreateText(arg.Substring(5));
                    Console.SetOut(sw);
                }
                else if (arg.ToLower() == "/delete")
                {
                    _deleteMode = true;
                }
                else if (arg.ToLower() == "/?" ||
                         arg.ToLower() == "--help")
                {
                    Console.WriteLine("PnpFind - Driverstore Destroyer");
                    Console.WriteLine("Copyright (C) 2011 Travis Robinson");
                    Console.WriteLine("");
                    Console.WriteLine("Usage: PnpFind [/out:file.ext] [/delete] [matchtext]");
                    return;
                }
                else
                {
                    _matchText = arg;
                }

            var oemFileList = DriverStore.GetOemInfFileList();
            foreach (var oemFileInfo in oemFileList)
            {
                var matched = _matchText == string.Empty || oemFileInfo.Name.ToLower().Contains(_matchText);

                DriverStore.GetInfSection(oemFileInfo.FullName, "Version", out var infEntities);
                infEntities.Add("Inf", new List<string>(new[] {oemFileInfo.Name}));
                infEntities.Remove("Signature");
                infEntities.Remove("signature");
                foreach (var infEntity in infEntities)
                {
                    foreach (var value in infEntity.Value)
                    {
                        if (_matchText == string.Empty)
                            matched = true;
                        else if (value.ToLower().Contains(_matchText.ToLower()))
                            matched = true;

                        if (matched) break;
                    }

                    if (matched) break;
                }

                if (matched) OemFileInfoSelectedList.Add(infEntities);
            }

            foreach (var infEntities in OemFileInfoSelectedList)
            {
                var infFile = string.Empty;
                foreach (var infEntity in infEntities)
                {
                    var valueDisplayText = infEntity.Value.Aggregate(string.Empty, (Current, InfValue) => Current + (InfValue + ", "));

                    if (valueDisplayText.Length >= 2)
                        valueDisplayText = valueDisplayText.Substring(0, valueDisplayText.Length - 2);

                    Console.WriteLine("{0,-15} : {1}", infEntity.Key, valueDisplayText);

                    if (infEntity.Key.ToLower() == "inf") infFile = valueDisplayText;
                }

                Console.WriteLine();

                if (_deleteMode && infFile != string.Empty)
                {
                    if (_matchText == string.Empty)
                        throw new Exception(
                            "aborting because empty match string would result in total driverstore annihilation.");
                    var result = DriverStore.RemoveOemInf(infFile);
                    switch (result)
                    {
                        case 0:
                            Console.WriteLine("{0} deleted successfully.", infFile);
                            break;
                        case 2: // does not exist.
                            try
                            {
                                DriverStore.GetOemInfFullPath(infFile).Delete();
                            }
                            catch (Exception)
                            {
                            }

                            break;
                        default:
                            Console.WriteLine("Failed deleting {0} result={1}", infFile, result);
                            break;
                    }
                }
            }

            if (!ReferenceEquals(null, sw))
            {
                sw.Flush();
                sw.Close();
            }
        }
    }
}
