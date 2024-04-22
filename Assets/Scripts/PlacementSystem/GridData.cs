using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GridData
{
    Dictionary<Vector3Int, PlacementData> placedObjects = new();

    public void AddObjectAt(Vector3Int startPos, Vector3Int endPos, int id)
    {
        List<Vector3Int> positionsToOccupy = CalculatePositions(startPos, endPos);

        PlacementData data = new PlacementData(id, positionsToOccupy);

        foreach (Vector3Int pos in positionsToOccupy)
        {
            if (placedObjects.ContainsKey(pos))
                throw new Exception("Error, pos already used");

            placedObjects[pos] = data;
        }

        // Debug.Log($"-----------added logic positions ------------");

        // foreach (var kvp in placedObjects)
        // {
        //     Debug.Log($"POS: {kvp.Key}, Value: {kvp.Value}");
        // }
    }

    public bool CanAddObjectAt(Vector3Int startPos, Vector3Int endPos)
    {
        List<Vector3Int> positions = CalculatePositions(startPos, endPos);

        foreach (Vector3Int pos in positions)
        {
            if (placedObjects.ContainsKey(pos))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Calculates positions within a box defined by two Vector3Int positions.
    /// </summary>
    /// <param name="startPos">The starting position of the box.</param>
    /// <param name="endPos">The ending position of the box.</param>
    /// <returns>A list of Vector3Int cell positions within the box.</returns>
    public List<Vector3Int> CalculatePositions(Vector3Int startPos, Vector3Int endPos)
    {
        List<Vector3Int> positions = new List<Vector3Int>();

        int startX = startPos.x;
        int endX = endPos.x;
        int startY = startPos.y;
        int endY = endPos.y;
        int startZ = startPos.z;
        int endZ = endPos.z;

        // Swap start and end positions if necessary for X-axis
        if (endX < startX)
        {
            int temp = startX;
            startX = endX;
            endX = temp;
        }

        // Swap start and end positions if necessary for Y-axis
        if (endY < startY)
        {
            int temp = startY;
            startY = endY;
            endY = temp;
        }

        // Swap start and end positions if necessary for Z-axis
        if (endZ < startZ)
        {
            int temp = startZ;
            startZ = endZ;
            endZ = temp;
        }

        for (int x = startX; x <= endX; x++)
        {
            for (int y = startY; y <= endY; y++)
            {
                for (int z = startZ; z <= endZ; z++)
                {
                    positions.Add(new Vector3Int(x, y, z));
                }
            }
        }
        return positions;
    }


    static int Sign(int num)
    {
        return num < 0 ? -1 : 1;
    }


    /// <summary>
    /// Calculates the position and scale of a box defined by two corner points in grid.
    /// </summary>
    /// <param name="startPos">The starting corner position of the box.</param>
    /// <param name="endPos">The ending corner position of the box.</param>
    /// <param name="symmetric">Whether the box should be symmetric.</param>
    /// <returns>A tuple (Vector3Int, Vector3Int) containing the position and scale of the box.</returns>
    public static (Vector3Int, Vector3Int) CalculateBoxPosAndScale(Vector3Int startPos, Vector3Int endPos, bool symmetric = false)
    {
        Vector3Int offset = endPos - startPos;


        //Size
        Vector3Int size;

        int width = Mathf.Abs(endPos.x - startPos.x) + 1;
        int height = Mathf.Abs(endPos.y - startPos.y) + 1;
        int depth = Mathf.Abs(endPos.z - startPos.z) + 1;
        size = new Vector3Int(width, height, depth);

        if (symmetric)
        {
            int maxSize = Mathf.Max(width, height, depth);
            size = new Vector3Int(maxSize, height == 0 ? 0 : maxSize, maxSize);
        }
        //change scale sign
        size = new Vector3Int(Sign(offset.x) * size.x, Sign(offset.y) * size.y, Sign(offset.z) * size.z);

        //Position
        Vector3Int pos = startPos;

        if (offset.x < 0 && offset.z < 0)
            pos += new Vector3Int(1, 0, 1);
        else if (offset.x >= 0 && offset.z < 0)
            pos += new Vector3Int(0, 0, 1);
        else if (offset.x < 0 && offset.z >= 0)
            pos += new Vector3Int(1, 0, 0);


        //Debug.Log($"s: {startPos} e: {endPos} scale: {size}");

        return (pos, size);
    }
}

public class PlacementData
{
    public List<Vector3Int> occupiedPositions;
    public int id;

    public PlacementData(int _id, List<Vector3Int> positions)
    {
        occupiedPositions = positions;
        id = _id;
    }
}