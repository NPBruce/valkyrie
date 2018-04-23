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

    public GeneratorMapSegment(GeneratorMapSegment in)
    {
        DuplicateFromSegment(in);
    }

    public GeneratorMapSegment(GeneratorMapSegment in, int rotation)
    {
         DuplicateFromSegment(in, rotation);
    }

    private void DuplicateFromSegment(GeneratorMapSegment in, int rotation = 0)
    {
        foreach (Tuple<string, GeneratorMapVector> c in in.components)
        {
            components.Add(new Tuple<string, GeneratorMapVector>(c.Item1, new GeneratorMapVector(c.Item2)));
        }
        foreach (KeyValuePair<int, Dictionary<int, GeneratorMapSpace>> kv in in.map)
        {
            foreach (KeyValuePair<int, GeneratorMapSpace> column in kv.Value)
            {
                SetSpace(GeneratorMapVector(kv.Key, column).Rotate(rotation), column.Value);
            }
        }
        foreach (GeneratorMapJoint j in in.joints)
        {
            joints.Add(new GeneratorMapJoint(j.location.Rotate(rotation), j.type));
        }
    }

    /// <summary>
    /// Get a list of all valid merge joints between segments</summary>
    /// <param name="toMerge">Segment to check against</param>
    /// <returns>List of possible merge positions</returns>
    public List<GeneratorMapJoint> ValidMergeJoints(GeneratorMapSegment toMerge)
    {
        List<GeneratorMapJoint> toReturn = new List<GeneratorMapJoint>();

        foreach (GeneratorMapJoint inJoint in toMerge.joints)
        {
            foreach (GeneratorMapJoint availableJoint in joints)
            {
                if (CheckJoin(toMerge, availableJoint, inJoint))
                {
                    // FIXME
                    // t?
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

    public Dictionary<string, int> GetComponentCounts()
    {
        Dictionary<string, int> toReturn = new Dictionary<string, int>();
        foreach (Tuple<string, GeneratorMapVector> component in components)
        {
            if (!toReturn.ContainsKey(component.Item1))
            {
                toReturn.Add(component.Item1, 0);
            }
            toReturn[component.Item1]++;
        }

        return toReturn;
    }

    /// <summary>
    /// Check if segment can be merged using specified joints</summary>
    /// <param name="toMerge">Segment to check against</param>
    /// <param name="availableJoint">Joint on this segment to check</param>
    /// <param name="inJoint">Joint on merging segment to check</param>
    /// <returns>True if merge possible</returns>
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

        foreach (KeyValuePair<int, Dictionary<int, GeneratorMapSpace>> row in toMerge.map)
        {
            foreach (KeyValuePair<int, GeneratorMapSpace> space in row.Value)
            {
                if (space.Value == GeneratorMapSpace.Void) continue;

                GeneratorMapVector targetVector = offset.Add(new GeneratorMapVector(row.Key, space.Key));
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
        // Fixme
        // ????
        List<GeneratorMapJoint> j = ValidMergeJoints(toMerge);
    }


// FIXME
// Unused?
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
