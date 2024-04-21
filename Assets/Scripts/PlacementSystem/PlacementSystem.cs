using System;
using UnityEngine;




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



    // Calculate the size of the selection rectangle based on two grid positions
    Vector2Int CalculateSelectionSize(Vector3Int v1, Vector3Int v2, bool symmetric = true)
    {
        Vector2Int startPos = new Vector2Int(v1.x, v1.z);
        Vector2Int endPos = new Vector2Int(v2.x, v2.z);


        Vector2Int selectionSize = endPos - startPos;

        // When calculating selection size, we need to ensure that it includes the cursor's position. 
        // Therefore, if the selection size in either dimension is positive, we increment it by one.
        if (selectionSize.x > 0)
            selectionSize.x++;
        if (selectionSize.y > 0)
            selectionSize.y++;

        //If size is 0 make it 1 to be able to have 1x1 
        if (selectionSize.x == 0)
            selectionSize.x++;
        if (selectionSize.y == 0)
            selectionSize.y++;

        // If symmetric is true, enforce square selection
        if (symmetric)
        {
            //get x and y symbols (-, +)
            int symbolX = selectionSize.x < 0 ? -1 : 1;
            int symbolY = selectionSize.y < 0 ? -1 : 1;

            int maxSize = Math.Abs(Mathf.Max(selectionSize.x, selectionSize.y));
            selectionSize = new Vector2Int(maxSize * symbolX, maxSize * symbolY);
        }


        return selectionSize;
    }


    void UpdateCellCursor()
    {
        if (objectToPlace == null) return;


        //If its dragable 
        if (InputManager.IsPressingPlaceInput() && objectToPlace.FreeformPlace)
        {
            Vector2Int selectionSize = CalculateSelectionSize(objectStartPosition, selectedCellPosition, objectToPlace.Symmetric);

            cellCursor.transform.localScale = new Vector3Int(selectionSize.x, 1, selectionSize.y);
            cellCursor.transform.position = objectStartPosition;
            //Debug.Log($"Size: {selectionSize}  cursor START: {startPos}  cursor end: {endPos}");
        }
        else
        {
            cellCursor.transform.localScale = Vector3.one;
            cellCursor.transform.position = selectedCellPosition;
        }


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
        return true;

    }

    private void PlaceObject()
    {
        if (InputManager.IsCursorOverUI) return;
        if (objectToPlace == null) return;
        if (!IsPlacementValid())
            return;


        GridData selectedGrid = GetGridOfObjectToPlace();





        if (objectToPlace.FreeformPlace)
        {
            GameObject obj = Instantiate(objectToPlace.Prefab, objectStartPosition, Quaternion.identity);
            Vector2Int objectSize = CalculateSelectionSize(objectStartPosition, selectedCellPosition, objectToPlace.Symmetric);
            obj.transform.localScale = new Vector3(objectSize.x, 1, objectSize.y);

            ////fix here 
            Vector3Int endPos = new Vector3Int(objectSize.x - 1, 0, objectSize.y - 1);

            selectedGrid.AddObjectAt(objectStartPosition, endPos, objectToPlace.id);
        }
        else
        {
            GameObject obj = Instantiate(objectToPlace.Prefab, selectedCellPosition, Quaternion.identity);
            selectedGrid.AddObjectAt(selectedCellPosition, new Vector3Int(objectToPlace.Size.x - 1, 0, objectToPlace.Size.y - 1), objectToPlace.id);
        }

        StopPlacement();
    }

    void SaveStartPos()
    {
        objectStartPosition = selectedCellPosition;
    }


    GridData GetGridOfObjectToPlace()
    {
        switch (objectToPlace.Type)
        {
            case PlaceableType.PLOT:
                return plotGridData;

            case PlaceableType.PLANT:
                return plantGridData;
            default:
                throw new Exception($"No grid found for type {objectToPlace.Type}");
        }
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
