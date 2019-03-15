using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

// Basr classes for section handling

namespace Fabric.Internal.Editor.ThirdParty.xcodeapi.PBX
{

    // common base
    internal abstract class SectionBase
    {
        public abstract void AddObject(string key, PBXElementDict value);
        public abstract void WriteSection(StringBuilder sb, GUIDToCommentMap comments);
    }

    // known section: contains objects that we care about
    internal class KnownSectionBase<T> : SectionBase where T : PBXObjectData, new()
    {
        private Dictionary<string, T> m_Entries = new Dictionary<string, T>();

        private string m_Name;

        public KnownSectionBase(string sectionName)
        {
            m_Name = sectionName;
        }
 
        public IEnumerable<KeyValuePair<string, T>> GetEntries()
        { 
            return m_Entries; 
        }

        public IEnumerable<string> GetGuids()
        {
            return m_Entries.Keys;
        }
        
        public IEnumerable<T> GetObjects()
        {
            return m_Entries.Values;
        }

        public override void AddObject(string key, PBXElementDict value)
        {
            T obj = new T();
            obj.guid = key;
            obj.SetPropertiesWhenSerializing(value);
            obj.UpdateVars();
            m_Entries[obj.guid] = obj;
        }

        public override void WriteSection(StringBuilder sb, GUIDToCommentMap comments)
        {
            if (m_Entries.Count == 0)
                return;            // do not write empty sections

            sb.AppendFormat("\n\n/* Begin {0} section */", m_Name);
            var keys = new List<string>(m_Entries.Keys);
            keys.Sort(StringComparer.Ordinal);
            foreach (string key in keys)
            {
                T obj = m_Entries[key];
                obj.UpdateProps();
                sb.Append("\n\t\t");
                comments.WriteStringBuilder(sb, obj.guid);
                sb.Append(" = ");
                Serializer.WriteDict(sb, obj.GetPropertiesWhenSerializing(), 2, 
                                     obj.shouldCompact, obj.checker, comments);
                sb.Append(";");
            }
            sb.AppendFormat("\n/* End {0} section */", m_Name);
        }

        // returns null if not found
        public T this[string guid]
        {
            get {
                if (m_Entries.ContainsKey(guid))
                    return m_Entries[guid];
                return null;
            }
        }
        
        public bool HasEntry(string guid)
        {
            return m_Entries.ContainsKey(guid);
        }      

        public void AddEntry(T obj)
        {
            m_Entries[obj.guid] = obj;
        }

        public void RemoveEntry(string guid)
        {
            if (m_Entries.ContainsKey(guid))
                m_Entries.Remove(guid);
        }
    }

    // we assume there is only one PBXProject entry
    internal class PBXProjectSection : KnownSectionBase<PBXProjectObjectData>
    {
        public PBXProjectSection() : base("PBXProject")
        {
        }

        public PBXProjectObjectData project
        {
            get {
                foreach (var kv in GetEntries())
                    return kv.Value;
                return null;
            }
        }
    }

} // UnityEditor.iOS.Xcode
