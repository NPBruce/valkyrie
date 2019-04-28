using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace Fabric.Internal.Editor.ThirdParty.xcodeapi
{
    public class JsonElement
    {
        protected JsonElement() {}

        // convenience methods
        public string AsString() { return ((JsonElementString)this).value; }
        public int AsInteger()   { return ((JsonElementInteger)this).value; }
        public bool AsBoolean()  { return ((JsonElementBoolean)this).value; }
        public JsonElementArray AsArray() { return (JsonElementArray)this; }
        public JsonElementDict AsDict()   { return (JsonElementDict)this; }

        public JsonElement this[string key]
        {
            get { return AsDict()[key]; }
            set { AsDict()[key] = value; }
        }
    }

    public class JsonElementString : JsonElement
    {
        public JsonElementString(string v) { value = v; }

        public string value;
    }

    public class JsonElementInteger : JsonElement
    {
        public JsonElementInteger(int v) { value = v; }

        public int value;
    }

    public class JsonElementBoolean : JsonElement
    {
        public JsonElementBoolean(bool v) { value = v; }

        public bool value;
    }

    public class JsonElementDict : JsonElement
    {
        public JsonElementDict() : base() {}

        private SortedDictionary<string, JsonElement> m_PrivateValue = new SortedDictionary<string, JsonElement>();
        public IDictionary<string, JsonElement> values { get { return m_PrivateValue; }}

        new public JsonElement this[string key]
        {
            get {
                if (values.ContainsKey(key))
                    return values[key];
                return null;
            }
            set { this.values[key] = value; }
        }

        public bool Contains(string key)
        {
            return values.ContainsKey(key);
        }

        public void Remove(string key)
        {
            values.Remove(key);
        }

        // convenience methods
        public void SetInteger(string key, int val)
        {
            values[key] = new JsonElementInteger(val);
        }

        public void SetString(string key, string val)
        {
            values[key] = new JsonElementString(val);
        }

        public void SetBoolean(string key, bool val)
        {
            values[key] = new JsonElementBoolean(val);
        }

        public JsonElementArray CreateArray(string key)
        {
            var v = new JsonElementArray();
            values[key] = v;
            return v;
        }

        public JsonElementDict CreateDict(string key)
        {
            var v = new JsonElementDict();
            values[key] = v;
            return v;
        }
    }

    public class JsonElementArray : JsonElement
    {
        public JsonElementArray() : base() {}
        public List<JsonElement> values = new List<JsonElement>();

        // convenience methods
        public void AddString(string val)
        {
            values.Add(new JsonElementString(val));
        }

        public void AddInteger(int val)
        {
            values.Add(new JsonElementInteger(val));
        }

        public void AddBoolean(bool val)
        {
            values.Add(new JsonElementBoolean(val));
        }

        public JsonElementArray AddArray()
        {
            var v = new JsonElementArray();
            values.Add(v);
            return v;
        }

        public JsonElementDict AddDict()
        {
            var v = new JsonElementDict();
            values.Add(v);
            return v;
        }
    }

    public class JsonDocument
    {
        public JsonElementDict root;
        public string indentString = "  ";

        public JsonDocument()
        {
            root = new JsonElementDict();
        }

        void AppendIndent(StringBuilder sb, int indent)
        {
            for (int i = 0; i < indent; ++i)
                sb.Append(indentString);
        }

        void WriteString(StringBuilder sb, string str)
        {
            // TODO: escape
            sb.Append('"');
            sb.Append(str);
            sb.Append('"');
        }

        void WriteBoolean(StringBuilder sb, bool value)
        {
            sb.Append(value ? "true" : "false");
        }

        void WriteInteger(StringBuilder sb, int value)
        {
            sb.Append(value.ToString());
        }

        void WriteDictKeyValue(StringBuilder sb, string key, JsonElement value, int indent)
        {
            sb.Append("\n");
            AppendIndent(sb, indent);
            WriteString(sb, key);
            sb.Append(" : ");
            if (value is JsonElementString)
                WriteString(sb, value.AsString());
            else if (value is JsonElementInteger)
                WriteInteger(sb, value.AsInteger());
            else if (value is JsonElementBoolean)
                WriteBoolean(sb, value.AsBoolean());
            else if (value is JsonElementDict)
                WriteDict(sb, value.AsDict(), indent);
            else if (value is JsonElementArray)
                WriteArray(sb, value.AsArray(), indent);
        }

        void WriteDict(StringBuilder sb, JsonElementDict el, int indent)
        {
            sb.Append("{");
            bool hasElement = false;
            foreach (var key in el.values.Keys)
            {
                if (hasElement)
                    sb.Append(","); // trailing commas not supported
                WriteDictKeyValue(sb, key, el[key], indent+1);
                hasElement = true;
            }
            sb.Append("\n");
            AppendIndent(sb, indent);
            sb.Append("}");
        }

        void WriteArray(StringBuilder sb, JsonElementArray el, int indent)
        {
            sb.Append("[");
            bool hasElement = false;
            foreach (var value in el.values)
            {
                if (hasElement)
                    sb.Append(","); // trailing commas not supported
                sb.Append("\n");
                AppendIndent(sb, indent+1);

                if (value is JsonElementString)
                    WriteString(sb, value.AsString());
                else if (value is JsonElementInteger)
                    WriteInteger(sb, value.AsInteger());
                else if (value is JsonElementBoolean)
                    WriteBoolean(sb, value.AsBoolean());
                else if (value is JsonElementDict)
                    WriteDict(sb, value.AsDict(), indent+1);
                else if (value is JsonElementArray)
                    WriteArray(sb, value.AsArray(), indent+1);
                hasElement = true;
            }
            sb.Append("\n");
            AppendIndent(sb, indent);
            sb.Append("]");
        }

        public void WriteToFile(string path)
        {
            File.WriteAllText(path, WriteToString());
        }

        public void WriteToStream(TextWriter tw)
        {
            tw.Write(WriteToString());
        }

        public string WriteToString()
        {
            var sb = new StringBuilder();
            WriteDict(sb, root, 0);
            return sb.ToString();
        }
    }


} // namespace UnityEditor.iOS.Xcode