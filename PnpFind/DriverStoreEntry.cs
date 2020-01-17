using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace PnpFind
{
    public class DriverStoreEntry
    {
        public string InfName { get; set; }

        public string Signature { get; set; }

        public string Provider { get; set; }

        public Guid ClassGUID { get; set; }

        public string Class { get; set; }

        public Version DriverVersion { get; set; }

        public DateTime? DriverDate { get; set; }

        public static IEnumerable<DriverStoreEntry> Entries
        {
            get
            {
                foreach (var oemFileInfo in DriverStore.GetOemInfFileList())
                {
                    DriverStore.GetInfSection(oemFileInfo.FullName, "Version", out var infEntities);

                    yield return new DriverStoreEntry
                    {
                        InfName = oemFileInfo.Name,
                        Class = infEntities["Class"][0],
                        ClassGUID = Guid.Parse(infEntities["ClassGUID"][0]),
                        DriverDate = DateTime.Parse(infEntities["DriverVer"][0], new CultureInfo("en-US")),
                        DriverVersion = Version.Parse(infEntities["DriverVer"][1]),
                        Provider = infEntities["Provider"][0],
                        Signature = infEntities["Signature"][0]
                    };
                }
            }
        }
    }
}