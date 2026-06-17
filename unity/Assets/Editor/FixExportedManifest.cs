using System.IO;
using System.Xml;
using UnityEditor.Android;

public class FixExportedManifest : IPostGenerateGradleAndroidProject
{
    public int callbackOrder { get { return 99; } }

    public void OnPostGenerateGradleAndroidProject(string path)
    {
        string manifestPath = path + "/src/main/AndroidManifest.xml";
        if (File.Exists(manifestPath))
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(manifestPath);

            XmlNamespaceManager nsMgr = new XmlNamespaceManager(doc.NameTable);
            nsMgr.AddNamespace("android", "http://schemas.android.com/apk/res/android");

            string[] nodesToFix = { "activity", "service", "receiver" };

            bool changed = false;

            foreach (string nodeName in nodesToFix)
            {
                XmlNodeList nodes = doc.SelectNodes("//" + nodeName, nsMgr);
                foreach (XmlNode node in nodes)
                {
                    // Check if it has an intent-filter
                    XmlNode intentFilter = node.SelectSingleNode("intent-filter");
                    if (intentFilter != null)
                    {
                        // Check if android:exported is already defined
                        XmlAttribute exportedAttr = node.Attributes["android:exported"];
                        // Or check using namespace URI
                        if (exportedAttr == null)
                        {
                            exportedAttr = node.Attributes["exported", "http://schemas.android.com/apk/res/android"];
                        }

                        if (exportedAttr == null)
                        {
                            exportedAttr = doc.CreateAttribute("android", "exported", "http://schemas.android.com/apk/res/android");
                            exportedAttr.Value = "true";
                            node.Attributes.Append(exportedAttr);
                            changed = true;
                        }
                    }
                }
            }

            if (changed)
            {
                doc.Save(manifestPath);
            }
        }
    }
}
