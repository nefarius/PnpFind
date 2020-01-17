using System.Collections.Generic;
using CommandLine;

namespace PnpFind
{
    enum MatchOptions
    {
        InfName,
        Class,
        ClassGUID,
        DriverDate,
        DriverVersion,
        Provider,
        Signature
    }

    abstract class MatchOptionsBase
    {
        [Value(0)]
        public MatchOptions? Match { get; set; }

        [Value(1, Min = 1)]
        public IEnumerable<string> Items { get; set; }
    }

    [Verb("enum", HelpText = "Enumerate all 3rd party driver packages in the driver store.")]
    class EnumOptions : MatchOptionsBase
    {
        
    }

    [Verb("delete", HelpText = "Delete one or more driver packages from the driver store.")]
    class DeleteOptions : MatchOptionsBase
    {
        //normal options here
    }

    public class Options
    {
        [Option('e', "enum-drivers", 
            HelpText = "Enumerate all 3rd party driver packages in the driver store.")]
        public bool EnumDrivers { get; set; }
    }
}