using UnityEngine;
using System.Collections.Generic;
using System.Text;
using System.IO;

// WIP tool to extract content from localization files
class ExtractDataTool
{
    public static void MoM(byte[] data)
    {
        List<string> labels = ReadLabels(data);
        HashSet<string> monsters = new HashSet<string>();
        string move = "_MOVE_";
        string attack = "_ATTACK_";
        string evade = "_EVADE_";
        string horror = "_HORROR_";
        string monster = "MONSTER_";

        foreach (string m in labels)
        {
            if (m.IndexOf(monster) == 0)
            {
                if (m.IndexOf(move) == m.Length - move.Length - 2)
                {
                    monsters.Add(m.Substring(monster.Length, m.IndexOf(move) - monster.Length));
                }
            }
        }

        foreach (string m in monsters)
        {
            Debug.Log(m);
        }
    }

    public static List<string> ReadLabels(byte[] data)
    {
        List<string> list = new List<string>();
        StreamReader stream = new StreamReader(new MemoryStream(data));

        string text = string.Empty;
        string readLine = stream.ReadLine();
        while (readLine != null && readLine.Length == 0)
        {
            readLine = stream.ReadLine();
        }
        bool quote = false;
        int num = 0;
        bool comment = false;
        while (readLine != null)
        {
            if (quote)
            {
                readLine = readLine.Replace("\\n", "\n");
                text = text + "\n" + readLine;
            }
            else
            {
                text = readLine.Replace("\\n", "\n");
                num = 0;
                comment = false;
            }
            int i = num;
            int length = text.Length;
            while (i < length)
            {
                char c = text[i];
                if (c == ',')
                {
                    if (!quote)
                    {
                        // Only get the first element
                        if (num == 0)
                        {
                            list.Add(text.Substring(num, i - num));
                        }
                        num = i + 1;
                    }
                }
                else if (c == '"')
                {
                    if (quote)
                    {
                        if (i + 1 >= length)
                        {
                            //list.Add(text.Substring(num, i - num).Replace("\"\"", "\""));
                            return list;
                        }
                        if (text[i + 1] != '"')
                        {
                            //list.Add(text.Substring(num, i - num).Replace("\"\"", "\""));
                            quote = false;
                            if (text[i + 1] == ',')
                            {
                                i++;
                                num = i + 1;
                            }
                        }
                        else
                        {
                            i++;
                        }
                    }
                    else
                    {
                        num = i + 1;
                        quote = true;
                    }
                }
                else if (c == '/' && i + 1 < length && text[i + 1] == '/')
                {
                    comment = true;
                    break;
                }
                i++;
            }
            if (!comment)
            {
                if (num < text.Length)
                {
                    if (quote)
                    {
                        continue;
                    }
                    //list.Add(text.Substring(num, text.Length - num));
                }
            }
            readLine = stream.ReadLine();
            //if not in quote
            if (!quote)
            {
                while (readLine != null && readLine.Length == 0)
                {
                    readLine = stream.ReadLine();
                }
            }
        }

        return list;
    }

    class Monster
    {
        string id;
        string name;
        List<Activation> activations;
        List<string> horror;
        List<string> evade;

        public Monster(string id, string name, List<string> data)
        {

        }

        public string GetImage()
        {
            return "";
        }

        public override string ToString()
        {
            return "";
        }
    }

    class Activation
    {
        string number;
        string condition;
        string attack;
        string unableButton;
        string unableText;
    }
}
