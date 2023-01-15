using System;
using System.IO;
using ValkyrieTools;

namespace Assets.Scripts.Content
{
    internal class ManifestManager
    {
        public readonly string Path;

        public ManifestManager(string path)
        {
            Path = path;
        }

        public IniData GetLocalManifestIniData()
        {
            if (string.IsNullOrWhiteSpace(Path))
            {
                throw new ArgumentNullException(nameof(Path));
            }

            IniData localManifest = IniRead.ReadFromString("");
            if(File.Exists(Path + ValkyrieConstants.ScenarioManifestPath))
            {
                localManifest = IniRead.ReadFromIni(Path + ValkyrieConstants.ScenarioManifestPath);
            }

            return localManifest;
        }
    }
}
