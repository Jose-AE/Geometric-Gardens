using System;
using UnityEngine;
using UnityEngine.EventSystems;



public class InputManager : MonoBehaviour
{
    private static PlayerInputActions playerInputActions;
    private static InputManager instance;


    #region BUTTON PRESS EVENTS

    public static event Action OnStartPlaceInput; //      InputManager.OnJumpInput += MyCustomFun;
    public static event Action OnEndPlaceInput; //      InputManager.OnJumpInput += MyCustomFun;

    public static event Action OnExitPlacementModeInput;



    #endregion


    public static bool IsCursorOverUI;



    void Awake()
    {
        //Make sure there is only a single instance of input manager class
        if (instance != null && instance != this)
        {
            Debug.LogWarning("An extra instance of input manager exists in '" + gameObject.name + "' gameObject and will be removed", gameObject);
            Destroy(this);
        }
        else
            instance = this;


        playerInputActions = new PlayerInputActions();


        #region EVENT_BINDS
        playerInputActions.PlacementMode.Place.performed += context => OnStartPlaceInput?.Invoke();
        playerInputActions.PlacementMode.Place.canceled += context => OnEndPlaceInput?.Invoke();


        playerInputActions.PlacementMode.Exit.performed += context => OnExitPlacementModeInput?.Invoke();
        #endregion

    }

    void Update()
    {
        IsCursorOverUI = EventSystem.current.IsPointerOverGameObject();
    }


    public static bool IsPressingPlaceInput()
    {
        if (instance == null)
            return false;

        return playerInputActions.PlacementMode.Place.IsPressed();
    }


    public static Vector2 GetMousePosition()
    {
        if (instance == null)
        {
            Debug.LogWarning("Attempting to call GetMousePosition() without an input manager present in the scene.");
            return Vector2.zero;
        }
        return playerInputActions.PlacementMode.MousePosition.ReadValue<Vector2>();
    }



    #region ENABLE_PLAYER_INPUT

    private void OnEnable()
    {
        playerInputActions.Enable();
    }
    private void OnDisable()
    {
        playerInputActions.Disable();
    }

    #endregion


}


