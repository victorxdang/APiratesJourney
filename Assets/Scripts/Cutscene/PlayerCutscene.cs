/*****************************************************************************************************************
 - StartUI.cs -
-----------------------------------------------------------------------------------------------------------------
 Author:             Victor Dang
 Game/Program Name:  A Pirate's Journey
 Engine Version:     Unity 2018.3.3f1
-----------------------------------------------------------------------------------------------------------------
 Description: 
     Movement logic for the player's ship in the start menu. There will be no documentation in this class since
     the movement logic is a copy and paste from the Player class.
*****************************************************************************************************************/

using UnityEngine;

public class PlayerCutscene : MonoBehaviour, IUpdatableObject
{
    const float SHIP_SPEED = 10;


    public static PlayerCutscene Instance { get; private set; }

    [SerializeField] Camera gameCamera;

    bool moving;
    float distanceMove;
    Plane plane;
    Vector2 start;
    Vector3 tiltInput, clampedPosition, moveToPoint, correctedVector;
    Touch touch;
    Ray rayMove;
    Matrix4x4 calibrationMatrix;


    void Awake()
    {
        if (FindObjectsOfType<PlayerCutscene>().Length == 1)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        plane = new Plane(Vector3.up, new Vector3(transform.parent.localPosition.x, transform.parent.localPosition.y + 3.6f, transform.parent.localPosition.z));
        Calibrate();

        UpdateManager.obj.Add(this);   
    }

    public void UpdateMe()
    {
        #if UNITY_EDITOR
            if (SaveManager.PlayerSaveData.use_tilt)
                CheckKeyboardInput();
            else
                CheckMouseDragInput();
        #else
            if (SaveManager.PlayerSaveData.use_tilt)
                CheckTiltInput();
            else
                CheckTouchDragInput();
        #endif
    }

    public bool Active()
    {
        return MainMenuUIManager.Instance.StartMenuActive() && !MainMenuUIManager.Instance.WatchAdMenuOpen && !MainMenuUIManager.Instance.OptionsMenuOpen;
    }

    void CheckKeyboardInput()
    {
        if (Input.GetKey(KeyCode.W))
            transform.parent.Translate(SHIP_SPEED * Time.deltaTime, 0, 0);
        else if (Input.GetKey(KeyCode.S))
            transform.parent.Translate(-SHIP_SPEED * Time.deltaTime, 0, 0);

        if (Input.GetKey(KeyCode.A))
            transform.parent.Translate(0, 0, SHIP_SPEED * Time.deltaTime);
        else if (Input.GetKey(KeyCode.D))
            transform.parent.Translate(0, 0, -SHIP_SPEED * Time.deltaTime);

        ClampPosition();
    }

    void CheckMouseDragInput()
    {
        moving = Input.GetMouseButton(0);

        if (moving)
            MoveTo(true, Input.mousePosition);
    }

    void CheckTiltInput()
    {
        tiltInput = 3 * SHIP_SPEED * Time.deltaTime * calibrationMatrix.MultiplyVector(Input.acceleration);
        tiltInput.y = 0;
        tiltInput.z = -tiltInput.z;
        transform.parent.localPosition += tiltInput;

        ClampPosition();
    }

    void CheckTouchDragInput()
    {
        if (Input.touchCount > 0)
        {
            touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                start = touch.position;
            }

            if (!moving && (touch.position.x - start.x > 25 || touch.position.y - start.y > 25))
            {
                moving = true;
            }
            else if (moving && touch.phase == TouchPhase.Ended)
            {
                moving = false;
            }

            if (moving)
                MoveTo(touch.phase == TouchPhase.Moved, touch.position);
        }
    }

    void MoveTo(bool fingerMoved, Vector3 position)
    {
        if (fingerMoved)
        {
            rayMove = gameCamera.ScreenPointToRay(position);
            plane.Raycast(rayMove, out distanceMove);
            moveToPoint = rayMove.GetPoint(distanceMove);
        }

        correctedVector = -1.5f * SHIP_SPEED * Time.deltaTime * (transform.parent.localPosition - moveToPoint).normalized;
        transform.parent.localPosition += correctedVector;

        ClampPosition();
    }

    void ClampPosition()
    {
        clampedPosition = transform.parent.localPosition;

        clampedPosition.x = Mathf.Clamp(clampedPosition.x, 0, 20);
        clampedPosition.y = 1.6f;
        clampedPosition.z = Mathf.Clamp(clampedPosition.z, -25, 25);

        transform.parent.localPosition = clampedPosition;
    }

    public void Calibrate()
    {
        calibrationMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.FromToRotation(new Vector3(0, -1, 0), Input.acceleration), new Vector3(1, 1, 1)).inverse;
    }

    public void OnSettingsChanged()
    {
        moving = false;
        touch = default(Touch);
    }
}
