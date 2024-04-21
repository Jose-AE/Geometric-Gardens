using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GridData
{
    Dictionary<Vector3Int, PlacementData> placedObjects = new();

    public void AddObjectAt(Vector3Int gridPos, Vector2Int objectSize, int id)
    {

        List<Vector3Int> positionsToOccupy = CalculatePositions(gridPos, objectSize);
        PlacementData data = new PlacementData(id, positionsToOccupy);

        foreach (Vector3Int pos in positionsToOccupy)
        {
            if (placedObjects.ContainsKey(pos))
                throw new Exception("Error, pos already used");

            placedObjects[pos] = data;
        }
    }

    public bool CanAddObjectAtPos(Vector3Int gridPos, Vector2Int objectSize)
    {
        List<Vector3Int> positions = CalculatePositions(gridPos, objectSize);

        foreach (Vector3Int pos in positions)
        {
            if (placedObjects.ContainsKey(pos))
                return false;
        }

        return true;
    }

    private List<Vector3Int> CalculatePositions(Vector3Int startPos, Vector2Int size)
    {

        List<Vector3Int> positions = new();

        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                positions.Add(startPos + new Vector3Int(x, 0, y));
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