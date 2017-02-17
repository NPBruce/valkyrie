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
        Dictionary<string, Monster> monsters = new Dictionary<string, Monster>();
        string attacks = "";

        foreach (string m in labels)
        {
            string mName = ExtractMonsterName(m);
            if (mName.Length > 0)
            {
                if (!monsters.ContainsKey(mName))
                {
                    monsters.Add(mName, new Monster(mName));
                }
                monsters[mName].Add(m);
            }
            
            if (m.IndexOf("ATTACK_") == 0)
            {
                attacks += GetAttack(m);
            }
        }
    }

    public static string GetAttack(string label)
    {
        string nameCamel = "";

        string[] elements = label.Split("_".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries);
        foreach (string e in elements)
        {
            string eFixed = e[0] + e.Substring(1).ToLower();
            nameCamel += eFixed;
        }

        string ret = "[Attack" + nameCamel + "]\r\n";
        ret += "text={ffg:" + label + "}\r\n";
        ret += "text={ffg:" + label + "}\r\n";
        //FIXME ATTACK_FIREARM_VS_BEAST_01
        ret += "target=" + label.Substring(0,1) + "}\r\n";
        ret += "attacktype=" + elements[1].ToLower() + "}\r\n";
        return ret;
    }

    public static string ExtractMonsterName(string label)
    {
        string move = "_MOVE_";
        string attack = "_ATTACK_";
        string evade = "_EVADE_";
        string horror = "_HORROR_";
        string monster = "MONSTER_";

        if (label.IndexOf(monster) != 0) return "";
        string name = label.Substring(monster.Length);

        if (name.IndexOf(move) != -1) return label.Substring(0, name.IndexOf(move));
        if (name.IndexOf(evade) != -1) return label.Substring(0, name.IndexOf(evade));
        if (name.IndexOf(attack) != -1) return label.Substring(0, name.IndexOf(attack));
        if (name.IndexOf(horror) != -1) return label.Substring(0, name.IndexOf(horror));
        return "";
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
        string nameFFG;
        string nameCamel;
        string nameUnderScoreLower;
        string nameReadable;
        List<Activation> activations;
        List<string> horror;
        List<string> evade;

        public Monster(string name)
        {
            horror = new List<string>();
            evade = new List<string>();
            horror = new List<string>();
            activations = new List<Activation>();

            nameFFG = name;
            string[] elements = name.Split("_".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries);
            foreach (string e in elements)
            {
                string eFixed = e[0] + e.Substring(1).ToLower();
                nameCamel += eFixed;
                nameUnderScoreLower += eFixed + "_";
                nameReadable += eFixed + " ";
            }
            nameUnderScoreLower = nameUnderScoreLower.Substring(0, nameUnderScoreLower.Length - 1);
            nameReadable = nameReadable.Substring(0, nameReadable.Length - 1);
        }

        public void Add(string label)
        {
            string moveStr = "_MOVE_";
            string attackStr = "_ATTACK_";
            string evadeStr = "_EVADE_";
            string horrorStr = "_HORROR_";
            
            string l = label.Substring(label.IndexOf(nameFFG) + nameFFG.Length);
            if (l.IndexOf(moveStr) == 0)
            {
                AddMove(l.Substring(moveStr.Length));
            }
            if (l.IndexOf(attackStr) == 0)
            {
                AddAttack(l.Substring(attackStr.Length));
            }
            if (l.IndexOf(evadeStr) == 0)
            {
                evade.Add(label);
            }
            if (l.IndexOf(horrorStr) == 0)
            {
                horror.Add(label);
            }
        }

        public void AddMove(string m)
        {
        }

        public void AddAttack(string m)
        {
        }

        public string GetEvade()
        {
            string ret = "";
            foreach (string s in evade)
            {
                ret += "[Evade" + nameCamel + s.Substring(s.Length - 2, 2) + "]\r\n";
                ret += "monster=" + nameCamel + "\r\n";
                ret += "text={ffg:" + s + "}\r\n\r\n";
            }
            return ret;
        }

        public string GetHorror()
        {
            string ret = "";
            foreach (string s in horror)
            {
                ret += "[Horror" + nameCamel + s.Substring(s.Length - 2, 2) + "]\r\n";
                ret += "monster=" + nameCamel + "\r\n";
                ret += "text={ffg:" + s + "}\r\n\r\n";
            }
            return ret;
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

