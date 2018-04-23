using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.Content;

public class GeneratorMapSegment
{
    public List<MapComponent> components = new List<MapComponent>();

    public Dictionary<int, Dictionary<int, GeneratorMapSpace>> map = new Dictionary<int, Dictionary<int, GeneratorMapSpace>>();

    public List<GeneratorMapJoint> joints = new List<GeneratorMapJoint>();

    public GeneratorMapSegment()
    {
    }

    public GeneratorMapSegment(GeneratorMapSegment inSegment)
    {
        DuplicateFromSegment(inSegment);
    }

    public GeneratorMapSegment(GeneratorMapSegment inSegment, int rotation)
    {
         DuplicateFromSegment(inSegment, rotation);
    }

    /// <summary>
    /// Populate map data from existing segment</summary>
    /// <param name="in">Segment to copy</param>
    /// <param name="rotation">Rotate copy in degrees</param>
    private void DuplicateFromSegment(GeneratorMapSegment inSegment, int rotation = 0)
    {
        foreach (MapComponent c in inSegment.components)
        {
            components.Add(new MapComponent(c));
        }
        foreach (KeyValuePair<int, Dictionary<int, GeneratorMapSpace>> kv in inSegment.map)
        {
            foreach (KeyValuePair<int, GeneratorMapSpace> column in kv.Value)
            {
                SetSpace(GeneratorMapVector(kv.Key, column).Rotate(rotation), column.Value);
            }
        }
        foreach (GeneratorMapJoint j in inSegment.joints)
        {
            joints.Add(new GeneratorMapJoint(j.location.Rotate(rotation), j.type));
        }
    }

    /// <summary>
    /// Get a list of all valid merge joints between segments</summary>
    /// <param name="toMerge">Segment to check against</param>
    /// <returns>List of possible merge positions</returns>
    public List<GeneratorMapJoint[]> ValidMergeJoints(GeneratorMapSegment toMerge)
    {
        List<GeneratorMapJoint[]> toReturn = new List<GeneratorMapJoint[]>();

        foreach (GeneratorMapJoint inJoint in toMerge.joints)
        {
            foreach (GeneratorMapJoint availableJoint in joints)
            {
                if (CheckJoin(toMerge, availableJoint, inJoint))
                {
                    toReturn.Add(new GeneratorMapJoint[]{ availableJoint, inJoint });
                }
            }
        }
        return toReturn;
    }

    /// <summary>
    /// Get component use count</summary>
    /// <param name="name">Component to check</param>
    /// <returns>Number of component used</returns>
    public int GetComponentCount(string name)
    {
        int toReturn;
        foreach (MapComponent component in components)
        {
            if (component.componentName.Equals(name))
            {
                toReturn++;
            }
        }
        return toReturn;
    }

    /// <summary>
    /// Get all component usage counts</summary>
    /// <returns>All component usage counts by component name</returns>
    public Dictionary<string, int> GetComponentCounts()
    {
        Dictionary<string, int> toReturn = new Dictionary<string, int>();
        foreach (MapComponent component in components)
        {
            if (!toReturn.ContainsKey(component.componentName))
            {
                toReturn.Add(component.componentName, 0);
            }
            toReturn[component.componentName]++;
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

        int rotation = 180 + (availableJoint.location.rotation - inJoint.location.rotation);

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
                    List<GeneratorMapJoint> incomingJoints = toMerge.GetJointsByLocation(new GeneratorMapVector(row.Key, space.Key));
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

    /// <summary>
    /// Get all joints at a location</summary>
    /// <param name="location">Location to check</param>
    /// <returns>List of all joints at location</returns>
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

    /// <summary>
    /// Merge segment at a random matching location</summary>
    /// <param name="toMerge">segment to merge</param>
    /// <returns>True if merge successful</returns>
    public bool Merge(GeneratorMapSegment toMerge)
    {
        List<GeneratorMapJoint[]> joins = ValidMergeJoints(toMerge);
        if (joints.Count < 1) return false;

        GeneratorMapJoint[] randomJoin = joins[Random.Range(0, joins.Count)];
        return Merge(toMerge, randomJoin[0], randomJoin[1]);
    }

    /// <summary>
    /// Merge segment at specified mating joints</summary>
    /// <param name="toMerge">segment to merge</param>
    /// <param name="availableJoint">Existing joint to use</param>
    /// <param name="inJoint">Incoming joint to use</param>
    /// <returns>True if merge successful</returns>
    public bool Merge(GeneratorMapSegment toMerge, GeneratorMapJoint availableJoint, GeneratorMapJoint inJoint)
    {
        int rotation = 180 + (availableJoint.location.rotation - inJoint.location.rotation);

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

                SetSpace(offset.Add(new GeneratorMapVector(row.Key, space.Key)), space.Value);
            }
        }

        joints.AddRange(toMerge.joints);
        TrimJoints();
        return true;
    }

    /// <summary>
    /// Trim matching joints (used after merge)</summary>
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

    /// <summary>
    /// Get space type at location</summary>
    /// <param name="location">location to check</param>
    /// <returns>Space type</returns>
    public GeneratorMapSpace GetSpace(GeneratorMapVector location)
    {
        return GetSpace(location.x, location.y);
    }

    /// <summary>
    /// Get space type at location</summary>
    /// <param name="x">x position to check</param>
    /// <param name="y">y position to check</param>
    /// <returns>Space type</returns>
    public GeneratorMapSpace GetSpace(float x, float y)
    {
        return GetSpace(Mathf.RoundToInt(x), Mathf.RoundToInt(y));
    }

    /// <summary>
    /// Get space type at location</summary>
    /// <param name="x">x position to check</param>
    /// <param name="y">y position to check</param>
    /// <returns>Space type</returns>
    public GeneratorMapSpace GetSpace(int x, int y)
    {
        if (!map.ContainsKey(x)) return GeneratorMapSpace.Void;
        if (!map[x].ContainsKey(y)) return GeneratorMapSpace.Void;
        return map[x][y];
    }

    /// <summary>
    /// Set space type at location</summary>
    /// <param name="location">location to set</param>
    /// <param name="value">Space type</param>
    public void SetSpace(GeneratorMapVector location, GeneratorMapSpace value)
    {
        SetSpace(location.x, location.y, value);
    }

    /// <summary>
    /// Set space type at location</summary>
    /// <param name="location">location to set</param>
    /// <param name="value">Space type</param>
    public void SetSpace(float x, float y, GeneratorMapSpace value)
    {
        SetSpace(Mathf.RoundToInt(x), Mathf.RoundToInt(y), value);
    }

    /// <summary>
    /// Set space type at location</summary>
    /// <param name="x">x location to set</param>
    /// <param name="y">y location to set</param>
    /// <param name="value">Space type</param>
    public void SetSpace(int x, int y, GeneratorMapSpace value)
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

    /// <summary>
    /// Get number of spaces taken by map segment</summary>
    /// <returns>Number of spaces</returns>
    public int Size()
    {
        int size = 0;
        foreach (KeyValuePair<int, Dictionary<int, GeneratorMapSpace>> row in map)
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

    public class MapComponent
    {
        public string componentName;
        public GeneratorMapVector position;

        public MapComponent(MapComponent toCopy)
        {
            componentName = toCopy.componentName;
            position = new GeneratorMapVector(toCopy.position);
        }
    }
}
