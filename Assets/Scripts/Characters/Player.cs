/*****************************************************************************************************************
 - Player.cs -
-----------------------------------------------------------------------------------------------------------------
 Author:             Victor Dang
 Game/Program Name:  A Pirate's Journey
 Engine Version:     Unity 2018.3.3f1
-----------------------------------------------------------------------------------------------------------------
 Description: 
     The main player of the game. This script will contain all of the movement logic and input of the player.
*****************************************************************************************************************/

using UnityEngine;

public class Player : Ship
{
    #region Constants

    /// <summary>
    /// The furthest left the player can travel.
    /// </summary>
    const float MIN_X = 0;

    /// <summary>
    /// The furthest right the player can travel.
    /// </summary>
    const float MAX_X = 20;

    /// <summary>
    /// The furthest forward the player can travel.
    /// </summary>
    const float MIN_Z = -25;

    /// <summary>
    /// The furthest back the player can travel.
    /// </summary>
    const float MAX_Z = 25;

    /// <summary>
    /// The speed that the ships travels (from either tilting the phone or 
    /// holding and dragging the ship).
    /// </summary>
    const float SHIP_SPEED = 10;

    #endregion


    #region Fields

    public static Player Instance { get; private set; }

    [SerializeField] float knockbackTime,
                           knockbackForce;

    [SerializeField] Camera gameCamera;
    [SerializeField] ParticleSystem[] particleWater;

    // raycasting fields
    float distanceShoot, distanceMove;
    Ray rayShoot, rayMove;
    Plane plane;
    Vector3 correctedVector;

    // tilt calibration fields
    Matrix4x4 calibrationMatrix;
    Vector3 tiltInput, clampedPosition;

    // touch fields
    bool moving, justMoved;
    Vector2 start;
    Touch touch;

    // other fields
    bool isKnockedBack;
    float currentKnockbackTime, distanceTravelled;
    Vector3 normalizedPosition, moveToPoint;

    #endregion


    #region Built-in Methods

    /// <summary>
    /// Ensure that there are no other player instances on the map and creates a plane that the
    /// camera will raycast to.
    /// </summary>
    void Awake()
    {
        if (FindObjectsOfType<Player>().Length == 1)
        {
            Instance = this;
            plane = new Plane(Vector3.up, new Vector3(transform.parent.localPosition.x, transform.parent.localPosition.y + SHIP_HEIGHT_OFFSET, transform.parent.localPosition.z));
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Sets up and initializes all data and UI appropriately.
    /// </summary>
    protected override void Start()
    {
        base.Start();

        // update player stats every time the level starts
        SetPlayerStats();
        SetParticlesActive(true);

        InGameUIManager.Instance.UpdateHealthIcon(HitPoints);
        InGameUIManager.Instance.UpdateReloadIcon(1);
    }

    /// <summary>
    /// Overriden method, if the player hits anything besides the trigger boundary at the end of the
    /// map, then end the game with a failure, otherwise, end the game with a level compelete flag.
    /// </summary>
    /// <param name="collider"></param>
    protected override void OnTriggerEnter(Collider collider)
    {
        base.OnTriggerEnter(collider);

        if ((collider.CompareTag("Enemy") || collider.CompareTag("Obstacle") || collider.CompareTag("Map")))
        {
            normalizedPosition = (collider.transform.position - transform.parent.localPosition).normalized;

            // frontal impact
            if (normalizedPosition.x >= 0.9f && InGameUIManager.Instance.PlayerTakeDamage)
            {
                SinkShip();
            }
            // side impact, only do knocking back (or siue in this case) if the player hits
            // an enemy or obstacle from the side
            else if (Mathf.Abs(normalizedPosition.z) >= 0.5f)
            {
                StartCoroutine(IEnumKnockback(normalizedPosition));
                TakeDamage(ImpactDamage);
            }
            // rear impact
            else if (normalizedPosition.x >= -0.9f)
            {
                TakeDamage(ImpactDamage);
            }

            AudioManager.Instance.PlayShipColliding();
        }
        else if (collider.CompareTag("Island"))
        {
            GameManager.InvokeEndGame(true);
            SetParticlesActive(false);
        }
    }

    #endregion


    #region Update Manager

    /// <summary>
    /// Movement of the player's ship is updated and checked in this Update method. In endless mode, 
    /// this will also keep track of how far the player has travelled in order to increase levels
    /// as needed.
    /// </summary>
    public override void UpdateMe()
    {
        // if the player is playing in endless mode and the player has reached a distance greater
        // the distance specified in GameManager.DISTANCE_TO_INCREASE_LEVEL, the level of the game
        // will increase by 1 and likewise, the stats of the enemy will increase as well
        if (SaveManager.PlayerPersistentData.game_mode == GameManager.GameMode.Endless)
        {
            distanceTravelled += GameManager.MAP_SPEED * Time.deltaTime;

            if (distanceTravelled > GameManager.DISTANCE_TO_INCREASE_LEVEL)
            {
                distanceTravelled = 0;
                SaveManager.PlayerPersistentData.level++;
            }
        }

        // repair the ship if the ship's HP is less than the max hp and after a 5 second delay
        // after the ship was hit by a cannonball
        if (HitPoints < MaxHitPoints && Time.time > timeToRepairShip && RepairSpeed > 0)
            RepairShip();

        if (!isKnockedBack)
        {
            // movement logic
            #if UNITY_EDITOR
                if (SaveManager.PlayerSaveData.use_tilt)
                    CheckKeyboardInput();
                else
                    CheckMouseDragInput();

                CheckMouseShootInput();
            #else
                if (SaveManager.PlayerSaveData.use_tilt)
                    CheckTiltInput();
                else
                    CheckTouchDragInput();
                
                CheckTouchShootInput();
            #endif
        }
    }

    /// <summary>
    /// Checks to see if this game object is active.
    /// </summary>
    /// <returns></returns>
    public override bool Active()
    {
        return !isSinking && GameManager.IsPlaying();
    }

    #endregion


    #region Mouse/Keyboard

    /// <summary>
    /// Used for playing the game in the Editor, WASD to move the ship.
    /// </summary>
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

    /// <summary>
    /// Used for playing the editor, press and hold the left mouse button to move the ship to
    /// where the mouse cursor is.
    /// </summary>
    void CheckMouseDragInput()
    {
        moving = Input.GetMouseButton(0);

        if (moving)
            MoveTo(true, Input.mousePosition);
    }

    /// <summary>
    /// Used for playing in the editor, fires the cannons at the point where the right mouse 
    /// button clicked.
    /// </summary>
    void CheckMouseShootInput()
    {
        if (Input.GetMouseButtonDown(1))
            ShootAt(Input.mousePosition);
    }

    #endregion


    #region Touch/Tilt

    /// <summary>
    /// Determines which way to move the ship based on the accelometer input. This method will also
    /// take into account the current orientation of the device and adjut it to zero out the tilt
    /// based on how the player is currently holding the phone.
    /// </summary>
    void CheckTiltInput()
    {
        tiltInput = 3 * SHIP_SPEED * Time.deltaTime * calibrationMatrix.MultiplyVector(Input.acceleration);
        tiltInput.y = 0;
        tiltInput.z = -tiltInput.z;
        transform.parent.localPosition += tiltInput;

        ClampPosition();
    }

    /// <summary>
    /// If the player is not using tilt to move the ship, then the player can touch the screen and 
    /// drag their finger around to have the ship move to the where the finger is touching. This 
    /// method makes a very strong assumption that the first finger that touches the screen is the 
    /// finger that will drag and move the ship. If the finger does not move and is lifted off of the
    /// phone, then the ship will fire instead. If both fingers are on the phone, then the first 
    /// registered finger is the one that will move the ship and the second finger is the one that will
    /// shoot the cannons.
    /// </summary>
    void CheckTouchDragInput()
    {
        if (Input.touchCount > 0)
        {
            touch = Input.GetTouch(0);

            // keep track of the point where the player began touching
            if (touch.phase == TouchPhase.Began)
            {
                start = touch.position;
            }

            // only move the ship once the distance between the starting touch point and the current
            // touch point is greater than the treshold (currently 25 pixels)
            if (!moving && (touch.position.x - start.x > 25 || touch.position.y - start.y > 25))
            {
                moving = true;
            }
            else if (moving && touch.phase == TouchPhase.Ended)
            {
                moving = false;
                justMoved = true;
            }

            if (moving)
                MoveTo(touch.phase == TouchPhase.Moved, touch.position);
        }
    }

    /// <summary>
    /// If there is only one finger touching the screen and it does not move, then shoot the cannons,
    /// otherwise if there is a second finger, then keep track of the second finger and where it
    /// is pointed at.
    /// </summary>
    void CheckTouchShootInput()
    {
        if (justMoved)
        {
            justMoved = false;
        }
        else if (Input.touchCount > 0)
        {
            touch = (!moving && Input.touchCount < 2) ? Input.GetTouch(0) : Input.GetTouch(1);

            if (touch.phase == TouchPhase.Ended)
                ShootAt(touch.position);
        }
    }

    #endregion


    #region Utilities

    /// <summary>
    /// Initializes the player's stats based on how many upgrades were made in the upgrade menu (in 
    /// the main menu screen).
    /// </summary>
    void SetPlayerStats()
    {
        // stats increased through upgrade menu

        MaxHitPoints = Scaling.PlayerStatInitial[Scaling.max_hp] + (SaveManager.PlayerSaveData.UpgradeAmount[Scaling.max_hp] * Scaling.PlayerStatMultiplier[Scaling.max_hp]);
        HitPoints = MaxHitPoints;
        Armor = Scaling.PlayerStatInitial[Scaling.armor] + (SaveManager.PlayerSaveData.UpgradeAmount[Scaling.armor] * Scaling.PlayerStatMultiplier[Scaling.armor]);
        RepairSpeed = Scaling.PlayerStatInitial[Scaling.repair_speed] + (SaveManager.PlayerSaveData.UpgradeAmount[Scaling.repair_speed] * Scaling.PlayerStatMultiplier[Scaling.repair_speed]);

        NumberOfCannons = (int) (Scaling.PlayerStatInitial[Scaling.cannons] + (SaveManager.PlayerSaveData.UpgradeAmount[Scaling.cannons] * Scaling.PlayerStatMultiplier[Scaling.cannons]));
        ReloadTime = Scaling.PlayerStatInitial[Scaling.reload_time] + (SaveManager.PlayerSaveData.UpgradeAmount[Scaling.reload_time] * Scaling.PlayerStatMultiplier[Scaling.reload_time]);
        CannonDamage = Scaling.PlayerStatInitial[Scaling.damage] + (SaveManager.PlayerSaveData.UpgradeAmount[Scaling.damage] * Scaling.PlayerStatMultiplier[Scaling.damage]);
        Accuracy = Scaling.PlayerStatInitial[Scaling.accuracy] + (SaveManager.PlayerSaveData.UpgradeAmount[Scaling.accuracy] * Scaling.PlayerStatMultiplier[Scaling.accuracy]);

        // makes sure that the values are within acceptable range
        ClampStats();
        SpawnCannonballs();
        SetupCannons();
    }

    /// <summary>
    /// If the ship is not reloading then fire the cannons at the point where the player is touching
    /// on screen.
    /// </summary>
    /// <param name="position"></param>
    void ShootAt(Vector3 position)
    {
        if (!isReloading)
        {
            rayShoot = gameCamera.ScreenPointToRay(position);
            plane.Raycast(rayShoot, out distanceShoot);

            Shoot(rayShoot.GetPoint(distanceShoot));
        }
    }

    /// <summary>
    /// Move the ship to the point where the player is touching on screen. Note that to initiate
    /// moving of the ship, the player must first drag their finger around to signify a move and 
    /// then the ship will move to the point that the player is currently touching.
    /// </summary>
    /// <param name="fingerMoved"></param>
    /// <param name="position"></param>
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

    /// <summary>
    /// Ensures that the position of the ships doesn't exceed the respective constant values.
    /// </summary>
    void ClampPosition()
    {
        clampedPosition = transform.parent.localPosition;

        clampedPosition.x = Mathf.Clamp(clampedPosition.x, MIN_X, MAX_X);
        clampedPosition.y = 1.6f;
        clampedPosition.z = Mathf.Clamp(clampedPosition.z, MIN_Z, MAX_Z);

        transform.parent.localPosition = clampedPosition;
    }

    /// <summary>
    /// Play the water particles when the game starts and deactivate it when the game ends. This
    /// will be done within the Update method of the player.
    /// </summary>
    /// <param name="active"></param>
    void SetParticlesActive(bool active)
    {
        if (particleWater.Length > 0)
        {
            for (int i = 0; i < particleWater.Length; i++)
            {
                if (active)
                    particleWater[i].Play();
                else
                    particleWater[i].Stop();
            }
        }
    }

    /// <summary>
    /// Simulates the player's ship being knocked back whenever the player hits a ship or 
    /// obstacle. Currently implemented in a way where the ship will only be knocked to the
    /// side instead of back or forward (see OnTriggerEnter for implementation).
    /// </summary>
    /// <param name="knockbackDirection"></param>
    /// <returns></returns>
    System.Collections.IEnumerator IEnumKnockback(Vector3 knockbackDirection)
    {
        currentKnockbackTime = knockbackTime;
        isKnockedBack = true;

        while (currentKnockbackTime > 0 && !isSinking)
        {
            transform.parent.Translate(-knockbackForce * Time.deltaTime * knockbackDirection);
            ClampPosition();
            currentKnockbackTime -= Time.deltaTime;

            yield return null;
        }

        isKnockedBack = false;
    }

    /// <summary>
    /// Set the calibration matrix based current orientation of the phone. This method should be 
    /// called in the start method to initialize the matrix.
    /// </summary>
    public void Calibrate()
    {
        calibrationMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.FromToRotation(new Vector3(0, -1, 0), Input.acceleration), new Vector3(1, 1, 1)).inverse;
    }

    /// <summary>
    /// Reset the settings if the player switches from tilt to drag or vice versa.
    /// </summary>
    public void OnSettingsChanged()
    {
        moving = false;
        justMoved = false;
        touch = default(Touch);
    }

    #endregion


    #region Overwritten Inherited Methods

    /// <summary>
    /// Overriden method, adds an extra line of code that updates the player's UI on how much
    /// HP the ship has left.
    /// </summary>
    /// <param name="damage"></param>
    protected override void TakeDamage(float damage, bool rammed = false)
    {
        if (InGameUIManager.Instance.PlayerTakeDamage)
        {
            base.TakeDamage(damage, rammed);
            GameManager.PlayerTakenDamage = true;
            InGameUIManager.Instance.UpdateHealthIcon(HitPoints / MaxHitPoints);
        }
    }

    /// <summary>
    /// Overriden method, adds an extra line of code that updates the player's UI on how much
    /// HP is currently regening.
    /// </summary>
    protected override void RepairShip()
    {
        base.RepairShip();
        InGameUIManager.Instance.UpdateHealthIcon(HitPoints / MaxHitPoints);
    }

    /// <summary>
    /// Overriden method, adds an extra line of code that calls the InvokeGameEnd method in order
    /// to end the game in a failure.
    /// </summary>
    protected override void SinkShip(bool sunkByRamming = false)
    {
        base.SinkShip(sunkByRamming);
        SetParticlesActive(false);

        AudioManager.Instance.PlayShipSinkingSE();
        InGameUIManager.Instance.UpdateHealthIcon(0);
        GameManager.InvokeEndGame(false);
    }

    #endregion
}
