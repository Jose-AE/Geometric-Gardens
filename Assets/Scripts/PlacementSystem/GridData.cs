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

        Debug.Log($"-----------added logic positions ------------");

        foreach (var kvp in placedObjects)
        {
            Debug.Log($"Key: {kvp.Key}, Value: {kvp.Value}");
        }
    }

    public bool CanAddObjectAtPos(Vector3Int startPos, Vector3Int endPos)
    {
        List<Vector3Int> positions = CalculatePositions(startPos, endPos);

        foreach (Vector3Int pos in positions)
        {
            if (placedObjects.ContainsKey(pos))
                return false;
        }

        return true;
    }

    private List<Vector3Int> CalculatePositions(Vector3Int startPos, Vector3Int endPos)
    {
        List<Vector3Int> positions = new List<Vector3Int>();

        int startX = startPos.x;
        int endX = endPos.x;
        int startY = startPos.z;
        int endY = endPos.z;

        // Swap start and end positions if necessary
        if (endX < startX)
        {
            int temp = startX;
            startX = endX;
            endX = temp;
        }
        if (endY < startY)
        {
            int temp = startY;
            startY = endY;
            endY = temp;
        }

        for (int x = startX; x <= endX; x++)
        {
            for (int y = startY; y <= endY; y++)
            {
                positions.Add(new Vector3Int(x, 0, y));
            }
        }
        return positions;
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