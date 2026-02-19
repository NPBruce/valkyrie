using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ValkyrieTools;

namespace Assets.Scripts.Content
{
    public class RemoteContentPack
    {
        public Dictionary<string, string> languages_name;
        public Dictionary<string, string> languages_description;
        public string image;
        public string identifier;
        public string type;
        public string version;
        public string package_url;
        public DateTime latest_update;
        public string defaultLanguage = ValkyrieConstants.DefaultLanguage;

        // is package available locally
        public bool downloaded = false;
        public bool update_available = false;

        public bool valid = false;

        public RemoteContentPack(string identifier, Dictionary<string, string> iniData)
        {
            this.identifier = identifier;
            valid = Populate(iniData);
        }

        /// <summary>
        /// Create from ini data
        /// </summary>
        /// <param name="iniData">ini data to populate quest</param>
        /// <returns>true if the quest is valid</returns>
        public bool Populate(Dictionary<string, string> iniData)
        {
            const string keyName = "name.";
            languages_name = new Dictionary<string, string>();
            if (iniData.ContainsKey(keyName + defaultLanguage))
            {
                foreach (KeyValuePair<string, string> kv in iniData)
                {
                    if (kv.Key.StartsWith(keyName))
                    {
                        languages_name.Add(kv.Key.Substring(keyName.Length), kv.Value);
                    }
                }
            }

            const string keyDescription = "description.";
            languages_description = new Dictionary<string, string>();
            if (iniData.ContainsKey(keyDescription + defaultLanguage))
            {
                foreach (KeyValuePair<string, string> kv in iniData)
                {
                    if (kv.Key.StartsWith(keyDescription))
                    {
                        languages_description.Add(kv.Key.Substring(keyDescription.Length), kv.Value);
                    }
                }
            }

            type = string.Empty;
            if (iniData.ContainsKey("type"))
            {
                type = iniData["type"];
            }

            if (iniData.ContainsKey("image"))
            {
                string value = iniData["image"];
                image = value != null ? value.Replace('\\', '/') : value;
            }

            // parse data for scenario explorer
            version = "";
            if (iniData.ContainsKey("version"))
            {
                version = iniData["version"];
            }

            package_url = "";
            if (iniData.ContainsKey("url"))
            {
                package_url = iniData["url"];
            }

            latest_update = new System.DateTime(0);
            if (iniData.ContainsKey("latest_update"))
            {
                System.DateTime.TryParse(iniData["latest_update"], out latest_update);
            }

            return true;
        }

        // Save to string (editor)
        override public string ToString()
        {
            string nl = System.Environment.NewLine;
            StringBuilder r = new StringBuilder();
            r.AppendLine($"[{ValkyrieConstants.RemoteContentPackIniType}]");
            r.Append("identifier=").AppendLine(identifier.ToString());
            r.Append("type=").AppendLine(type.ToString());
            r.Append("type=").AppendLine(Game.Get().gameType.TypeName());
            r.Append("defaultlanguage=").AppendLine(defaultLanguage);

            if (image.Length > 0)
            {
                r.Append("image=").AppendLine(image);
            }

            if (version != "")
            {
                r.Append("version=").AppendLine(version);
            }

            foreach (KeyValuePair<string, string> kv in languages_name)
            {
                r.Append("name." + kv.Key + "=").AppendLine(kv.Value);
            }

            return r.ToString();
        }

        public string GetTitle(string userLanguage)
        {
            if (languages_name.ContainsKey(userLanguage))
            {
                return languages_name[userLanguage];
            }
            if (languages_name.ContainsKey(defaultLanguage))
            {
                return languages_name[defaultLanguage];
            }
            if (languages_name.Count > 0)
            {
                return languages_name.First().Value;
            }
            return "";
        }

        public string GetDescription(string userLanguage)
        {
            if (languages_description.ContainsKey(userLanguage))
            {
                return languages_description[userLanguage];
            }
            if (languages_description.ContainsKey(defaultLanguage))
            {
                return languages_description[defaultLanguage];
            }
            if (languages_description.Count > 0)
            {
                return languages_description.First().Value;
            }
            return "";
        }
    }
}
