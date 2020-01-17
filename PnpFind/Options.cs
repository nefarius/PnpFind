using CommandLine;

namespace PnpFind
{
    [Verb("enum", HelpText = "Enumerate all 3rd party driver packages in the driver store.")]
    class EnumOptions
    {
        //normal options here
    }

    [Verb("delete", HelpText = "Delete one or more driver packages from the driver store.")]
    class DeleteOptions
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