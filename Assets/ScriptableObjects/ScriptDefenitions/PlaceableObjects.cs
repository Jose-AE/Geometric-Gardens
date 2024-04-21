using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum PlaceableType
{
    PLOT,
    PLANT,
}

[CreateAssetMenu]
public class PlaceableObjects : ScriptableObject
{

    public List<ObjectData> objects;

}


[Serializable]
public class ObjectData
{

    [field: SerializeField]

    public string Name { get; private set; }
    [field: SerializeField]
    public int id { get; private set; }
    [field: SerializeField]
    public Vector2Int Size { get; private set; } = Vector2Int.one;
    [field: SerializeField]
    public GameObject Prefab { get; private set; }

    [field: SerializeField]
    public PlaceableType Type { get; private set; }


}