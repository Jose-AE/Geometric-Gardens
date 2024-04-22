using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;




public class PlacementSystem : MonoBehaviour
{
    private Vector3Int currentCellPosAtCursor;

    private Vector3Int cellPosAtCursorClick;


    private ObjectData objectToPlaceData;




    [SerializeField] private bool debugCells;
    [SerializeField] private GameObject debugCellPrefab;


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
        if (debugCells) DegubCellPos();




        StopPlacement();
        StartPlacement(0);

    }

    private void DegubCellPos()
    {

        Vector3Int startPos = new Vector3Int(5, 0, 5);
        Vector3Int endPos = new Vector3Int(-5, 0, -5);


        List<Vector3Int> list = plotGridData.CalculatePositions(startPos, endPos);
        foreach (Vector3Int pos in list)
        {
            GameObject obj = Instantiate(debugCellPrefab);
            obj.transform.position = pos;
            obj.GetComponentInChildren<TMP_Text>().text = $"({pos.x},{pos.z})";
        }

        Debug.Log(GridData.CalculateBoxPosAndScale(startPos, endPos));
    }

    // Update is called once per frame
    void Update()
    {
        UpdateSelectedCellPosition();

        UpdateCellCursor();
    }


    void UpdateCellCursor()
    {
        if (objectToPlaceData == null) return;


        Vector3Int pos;
        Vector3Int scale;
        Color color = Color.gray;

        if (InputManager.IsPressingPlaceInput() && objectToPlaceData.Size.x <= 0 && objectToPlaceData.Size.y <= 0)
        {
            if (objectToPlaceData.Size.magnitude == 0)
            {
                (pos, scale) = GridData.CalculateBoxPosAndScale(cellPosAtCursorClick, currentCellPosAtCursor);
                color = IsPlacementValid(cellPosAtCursorClick, currentCellPosAtCursor) ? Color.green : Color.red;
            }
            else
            {
                (pos, scale) = GridData.CalculateBoxPosAndScale(cellPosAtCursorClick, currentCellPosAtCursor, true);
                color = IsPlacementValid(cellPosAtCursorClick, cellPosAtCursorClick + (scale - Vector3Int.one)) ? Color.green : Color.red;
            }

            cellCursor.transform.localScale = scale;
            cellCursor.transform.position = pos;
        }
        else
        {
            Vector3Int endPos = currentCellPosAtCursor;

            if (objectToPlaceData.Size.x > 0 && objectToPlaceData.Size.y > 0)
                endPos += new Vector3Int(objectToPlaceData.Size.x - 1, 0, objectToPlaceData.Size.y - 1);

            (pos, scale) = GridData.CalculateBoxPosAndScale(currentCellPosAtCursor, endPos);

            cellCursor.transform.localScale = scale;
            cellCursor.transform.position = pos;

            color = IsPlacementValid(currentCellPosAtCursor, endPos) ? Color.green : Color.red;
        }
        cellCursor.transform.position += new Vector3(0, 0.01f, 0);//make the cursor not clip with floor

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
        objectToPlaceData = placeableObjects.objects[objectToPlaceIndex];

        gridVisualisation.SetActive(true);
        cellCursor.SetActive(true);

        InputManager.OnEndPlaceInput += PlaceObject;
        InputManager.OnStartPlaceInput += SaveCellPosAtCursor;
        InputManager.OnExitPlacementModeInput += StopPlacement;
    }

    private bool IsPlacementValid(Vector3Int startPos, Vector3Int endPos)
    {
        GridData grid = GetGridOfObjectToPlace();
        return grid.CanAddObjectAt(startPos, endPos);
    }

    private void PlaceObject()
    {
        if (InputManager.IsCursorOverUI) return;
        if (objectToPlaceData == null) return;

        GridData selectedGrid = GetGridOfObjectToPlace();



        if (objectToPlaceData.Size.x <= 0 && objectToPlaceData.Size.y <= 0)
        {

            Vector3Int pos;
            Vector3Int scale;

            if (objectToPlaceData.Size.magnitude == 0)
            {
                if (!IsPlacementValid(cellPosAtCursorClick, currentCellPosAtCursor))
                    return;
                (pos, scale) = GridData.CalculateBoxPosAndScale(cellPosAtCursorClick, currentCellPosAtCursor);
                selectedGrid.AddObjectAt(cellPosAtCursorClick, currentCellPosAtCursor, objectToPlaceData.id);
            }
            else
            {
                (pos, scale) = GridData.CalculateBoxPosAndScale(cellPosAtCursorClick, currentCellPosAtCursor, true);

                if (!IsPlacementValid(cellPosAtCursorClick, cellPosAtCursorClick + (scale - Vector3Int.one)))
                    return;

                selectedGrid.AddObjectAt(cellPosAtCursorClick, cellPosAtCursorClick + (scale - Vector3Int.one), objectToPlaceData.id);
            }

            GameObject obj = Instantiate(objectToPlaceData.Prefab, pos, Quaternion.identity);
            obj.transform.localScale = scale;
        }
        else
        {
            if (!IsPlacementValid(currentCellPosAtCursor, currentCellPosAtCursor))
                return;

            Instantiate(objectToPlaceData.Prefab, currentCellPosAtCursor, Quaternion.identity);
            selectedGrid.AddObjectAt(currentCellPosAtCursor, currentCellPosAtCursor + new Vector3Int(objectToPlaceData.Size.x - 1, 0, objectToPlaceData.Size.y - 1), objectToPlaceData.id);
        }

        StopPlacement();
    }

    void SaveCellPosAtCursor()
    {
        cellPosAtCursorClick = currentCellPosAtCursor;
    }


    GridData GetGridOfObjectToPlace()
    {
        switch (objectToPlaceData.Type)
        {
            case PlaceableType.PLOT:
                return plotGridData;

            case PlaceableType.PLANT:
                return plantGridData;
            default:
                throw new Exception($"No grid found for type {objectToPlaceData.Type}");
        }
    }

    void StopPlacement()
    {
        objectToPlaceData = null;
        gridVisualisation.SetActive(false);
        cellCursor.SetActive(false);
        InputManager.OnEndPlaceInput -= PlaceObject;
        InputManager.OnExitPlacementModeInput -= StopPlacement;
        InputManager.OnStartPlaceInput -= SaveCellPosAtCursor;
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
            currentCellPosAtCursor = grid.WorldToCell(hit.point);
        }
    }
}
