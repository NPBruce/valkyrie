using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.Content;

public class GeneratorMapSegment
{
    public List<Tuple<string, GeneratorMapVector>> components = new List<Tuple<string, GeneratorMapVector>>();

    public Dictionary<int, Dictionary<int, GeneratorMapSpace>> map = new Dictionary<int, Dictionary<int, GeneratorMapSpace>>();

    public List<GeneratorMapJoint> joints = new List<GeneratorMapJoint>();

    public GeneratorMapSegment()
    {
    }

    public List<GeneratorMapJoint> CheckMerge(GeneratorMapSegment toMerge)
    {
        List<GeneratorMapJoint> toReturn = new List<GeneratorMapJoint>();

        foreach (GeneratorMapJoint inJoint in toMerge.joints)
        {
            foreach (GeneratorMapJoint availableJoint in joints)
            {
                if (inJoint.type != availableJoint.type) continue;

                if (CheckJoin(toMerge, availableJoint, inJoint))
                {
                    toReturn.Add(t);
                }
            }
        }
        return toReturn;
    }

    public int GetComponentCount(string name)
    {
        int toReturn;
        foreach (Tuple<string, GeneratorMapVector> component in components)
        {
            if (component.Item1.Equals(name))
            {
                toReturn++;
            }
        }
        return toReturn;
    }

    public List<Dictionary<string, int>> GetComponentCounts()
    {
        List<Dictionary<string, int>> toReturn = new List<Tuple<string, int>>();
        foreach (Tuple<string, GeneratorMapVector> component in components)
        {
            if (!toReturn.ContainsKey(component.Item1))
            {
                toReturn.Add(component.Item1);
            }
            toReturn[component.Item1]++;
        }

        return toReturn;
    }

    private bool CheckJoin(GeneratorMapSegment toMerge, GeneratorMapJoint availableJoint, GeneratorMapJoint inJoint)
    {
        if (inJoint.type != availableJoint.type) return false;

        int rotation = 180 + (availableJoint.rotation - inJoint.rotation);

        if (rotation < 0)
        {
            rotation += 360;
        }

        if (rotation > 270)
        {
            rotation -= 360;
        }

        GeneratorMapVector offset = new GeneratorMapVector(availableJoint.location.x, availableJoint.location.y, rotation).Add(inJoint.location);

        foreach (KeyValuePair<int, Dictionary<int, GeneratorMapSpace> row in toMerge.map)
        {
            foreach (KeyValuePair<int, GeneratorMapSpace> space in row.Value)
            {
                if (space.Value == GeneratorMapSpace.Void) continue;

                GeneratorMapVector targetVector = offset.Add(new GeneratorMapVector(row.Key, space.Key))
                GeneratorMapSpace targetSpace = GetSpace(targetVector);
                if (targetSpace == GeneratorMapSpace.Void) continue;
                // join block checks go here
                foreach (GeneratorMapJoint j in GetJointsByLocation(targetVector))
                {
                    List<GeneratorMapJoint> incomingJoints = GetJointsByLocation(j.location);
                    if (incomingJoints.Count == 0) return false;

                    foreach (GeneratorMapJoint incomingJoint in incomingJoints)
                    {
                        if (!incomingJoint.MatingJoint(j)) return false;
                    }
                }
            }
        }
        return true;
    }

    public List<GeneratorMapJoint> GetJointsByLocation(GeneratorMapVector location)
    {
        List<GeneratorMapJoint> toReturn = new List<GeneratorMapJoint>();
        foreach (GeneratorMapJoint j in joints)
        {
            if (j.location.WithinASpace(location))
            {
                toReturn.Add(j);
            }
        }
        return toReturn;
    }

    public bool Merge(GeneratorMapSegment toMerge)
    {
        List<GeneratorMapJoint> j = CheckMerge(toMerge);
    }


    public bool Merge(GeneratorMapSegment toMerge, GeneratorMapJoint availableJoint, GeneratorMapJoint inJoint)
    {
        int rotation = 180 + (availableJoint.rotation - inJoint.rotation);

        if (rotation < 0)
        {
            rotation += 360;
        }

        if (rotation > 270)
        {
            rotation -= 360;
        }

        GeneratorMapVector offset = new GeneratorMapVector(availableJoint.location.x, availableJoint.location.y, rotation).Add(inJoint.location);

        foreach (KeyValuePair<int, Dictionary<int, GeneratorMapSpace> row in toMerge.map)
        {
            foreach (KeyValuePair<int, GeneratorMapSpace> space in row.Value)
            {
                if (space.Value == GeneratorMapSpace.Void) continue;

                SetSpace(offset.Add(new GeneratorMapVector(row.Key, space.Key)), space.Value);
            }
        }

        joints.AddRange(toMerge.joints);
        TrimJoints()
    }

    public void TrimJoints()
    {
        List<GeneratorMapJoint> toRemove = new List<GeneratorMapJoint>();
        foreach (GeneratorMapJoint j in joints)
        {
            foreach (GeneratorMapJoint jTwo in joints)
            {
                if (jTwo.MatingJoint(j))
                {
                    toRemove.Add(j);
                }
            }
        }

        foreach (GeneratorMapJoint j in toRemove)
        {
            joints.Remove(j);
        }
    }

    public GeneratorMapSpace GetSpace(GeneratorMapVector location)
    {
        GetSpace(Mathf.RoundToInt(location.x), Mathf.RoundToInt(location.y));
    }

    public GeneratorMapSpace GetSpace(int x, int y)
    {
        if (!map.ContainsKey(x)) return GeneratorMapSpace.Void;
        if (!map[x].ContainsKey(y)) return GeneratorMapSpace.Void;
        return map[x][y];
    }

    public GeneratorMapSpace SetSpace(GeneratorMapVector location, GeneratorMapSpace value)
    {
        SetSpace(Mathf.RoundToInt(location.x), Mathf.RoundToInt(location.y));
    }

    public GeneratorMapSpace SetSpace(int x, int y, GeneratorMapSpace value)
    {
        if (!map.ContainsKey(x))
        {
            map.Add(x, new Dictionary<int, GeneratorMapSpace>());
        }
        if (!map[x].ContainsKey(y))
        {
            map[x].Add(y, value);
        }
        map[x][y] = value;;
    }

    public GeneratorMapSegment Copy()
    {
        GeneratorMapSegment toReturn = new GeneratorMapSegment();

        toReturn.components = new List<Tuple<string, GeneratorMapVector>>(components);
        toReturn.map = new Dictionary<int, Dictionary<int, GeneratorMapSpace>>(map);
        toReturn.joints = new List<GeneratorMapJoint>(joints);

        return toReturn;
    }

    public int Size()
    {
        int size = 0;
        foreach (KeyValuePair<int, Dictionary<int, GeneratorMapSpace> row in map)
        {
            foreach (KeyValuePair<int, GeneratorMapSpace> space in row.Value)
            {
                if (space.Value != GeneratorMapSpace.Void)
                {
                    size++;
                }
            }
        }
        return size;
    }
}
