using System;
using System.Linq;
using CommandLine;

namespace PnpFind
{
    internal class Program
    {
        private static int ParseEnumOptions(EnumOptions opts)
        {
            if (!opts.Match.HasValue)
            {
                foreach (var entry in DriverStoreEntry.Entries) Console.WriteLine($"{entry}\n");

                return 0;
            }

            try
            {
                foreach (var item in opts.Items)
                foreach (var entry in DriverStoreEntry.Entries.Where(p =>
                    p.GetType().GetProperty(opts.Match.ToString()).GetValue(p, null).Equals(item)))
                    Console.WriteLine($"{entry}\n");
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
            if (!opts.Match.HasValue)
            {
                Console.WriteLine("Missing match criteria, won't remove entire driver store!'");

                return 1;
            }

            try
            {
                foreach (var item in opts.Items)
                foreach (var entry in DriverStoreEntry.Entries.Where(p =>
                    p.GetType().GetProperty(opts.Match.ToString()).GetValue(p, null).Equals(item)))
                {
                    DriverStore.RemoveOemInf(entry.InfName);
                    Console.WriteLine($"Removed entry:\n\n{entry}\n");
                }
            }
            catch (ArgumentException ae)
            {
                Console.WriteLine($"Error: {ae.Message}");
                return 1;
            }

            return 0;
        }

        private static void Main(string[] args)
        {
            Parser.Default.ParseArguments<EnumOptions, DeleteOptions>(args)
                .MapResult(
                    (EnumOptions Opts) => ParseEnumOptions(Opts),
                    (DeleteOptions Opts) => ParseDeleteOptions(Opts),
                    Errs => 1);
        }
    }
}