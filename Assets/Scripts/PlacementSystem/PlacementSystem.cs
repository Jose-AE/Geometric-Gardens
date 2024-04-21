using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;




public class PlacementSystem : MonoBehaviour
{
    private Vector3Int selectedCellPosition;
    private ObjectData objectToPlace;
    private Vector3Int objectStartPosition;





    [SerializeField] private LayerMask placementLayerMask;

    [SerializeField] private GameObject cellCursor;

    [SerializeField] private Grid grid;

    [SerializeField] private GameObject gridVisualisation;

    [SerializeField] private PlaceableObjects placeableObjects;



    private GridData plotGridData, plantGridData;


    void Awake()
    {
        plotGridData = new();
        plantGridData = new();
    }


    // Start is called before the first frame update
    void Start()
    {
        StopPlacement();
        StartPlacement(0);
    }

    // Update is called once per frame
    void Update()
    {
        UpdateSelectedCellPosition();

        UpdateCellCursor();
    }


    void UpdateCellCursor()
    {
        if (objectToPlace == null) return;




        if (InputManager.IsPressingPlaceInput() && objectToPlace.Type == PlaceableType.PLOT)
        {
            Vector2Int startPos = new Vector2Int(objectStartPosition.x, objectStartPosition.z);
            Vector2Int endPos = new Vector2Int(selectedCellPosition.x, selectedCellPosition.z);

            Vector2Int selectionSize = startPos - endPos;


            Debug.Log($"Size: {selectionSize} diff: {0}  cursor START: {objectStartPosition}  cursor end: {selectedCellPosition}");
            cellCursor.transform.localScale = new Vector3Int(selectionSize.x, 1, selectionSize.y);



        }
        else
            cellCursor.transform.localScale = Vector3.one;




        cellCursor.transform.position = selectedCellPosition;

        Color color = IsPlacementValid() ? Color.green : Color.red;
        cellCursor.GetComponentInChildren<Renderer>().material.SetColor("_Color", color);
    }




    public void StartPlacement(int id)
    {
        int objectToPlaceIndex = placeableObjects.objects.FindIndex(i => i.id == id);
        if (objectToPlaceIndex < 0)
        {
            Debug.LogError($"Failed to start placement for object with ID {id} ");
            return;
        }
        objectToPlace = placeableObjects.objects[objectToPlaceIndex];

        gridVisualisation.SetActive(true);
        cellCursor.SetActive(true);

        InputManager.OnEndPlaceInput += PlaceObject;
        InputManager.OnStartPlaceInput += SaveStartPos;
        InputManager.OnExitPlacementModeInput += StopPlacement;
    }

    private bool IsPlacementValid()
    {
        switch (objectToPlace.Type)
        {
            case PlaceableType.PLOT:
                return plotGridData.CanAddObjectAtPos(selectedCellPosition, objectToPlace.Size);

            case PlaceableType.PLANT:
                return plantGridData.CanAddObjectAtPos(selectedCellPosition, objectToPlace.Size);

            default:
                return true;
        }
    }

    private void PlaceObject()
    {
        if (InputManager.IsCursorOverUI) return;
        if (objectToPlace == null) return;
        if (!IsPlacementValid())
            return;

        switch (objectToPlace.Type)
        {
            case PlaceableType.PLOT:
                plotGridData.AddObjectAt(selectedCellPosition, objectToPlace.Size, objectToPlace.id);
                break;
            case PlaceableType.PLANT:
                plantGridData.AddObjectAt(selectedCellPosition, objectToPlace.Size, objectToPlace.id);
                break;
        }


        Debug.Log($"place at: {selectedCellPosition}");
        GameObject obj = Instantiate(objectToPlace.Prefab, selectedCellPosition, Quaternion.identity);
        StopPlacement();
    }

    void SaveStartPos()
    {
        objectStartPosition = selectedCellPosition;
    }

    void StopPlacement()
    {
        objectToPlace = null;
        gridVisualisation.SetActive(false);
        cellCursor.SetActive(false);
        InputManager.OnEndPlaceInput -= PlaceObject;
        InputManager.OnExitPlacementModeInput -= StopPlacement;
        InputManager.OnStartPlaceInput -= SaveStartPos;
    }



    public void UpdateSelectedCellPosition()
    {
        Camera camera = LevelManager.mainCamera;
        Vector2 mousePos = InputManager.GetMousePosition();
        Ray ray = camera.ScreenPointToRay(new Vector3(mousePos.x, mousePos.y, camera.nearClipPlane));

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 1000, placementLayerMask))
        {
            //Debug.Log("Hit Pos: " + hit.point);
            //Debug.Log("Grid Pos: " + grid.WorldToCell(hit.point));
            selectedCellPosition = grid.WorldToCell(hit.point);
        }
    }




}
