using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
//using static UnityEditor.PlayerSettings;

public class PlayerScript : MonoBehaviour
{

    //Parameters
    [SerializeField] private float currentSpeed;
    public float walkTopSpeed = 7;
    public float sprintTopSpeed = 14;
    public float playerHeight;
    public LayerMask groundMask;
    public LayerMask interactableMask;
    public Vector2 mouseSensitivity;
    public Vector3 interactSize;

    [Header("Stamina Parameters")]
    public float playerStamina; 
    public float maxStamina = 100f; 
    public float staminaDrain;
    bool canSprint;
    bool isMoving;
    private float timeBeforeRegen  = 3;
    private float staminaIncrement = 2;
    private float staminaTimeIncrement = 0.1f;
    private Coroutine regeneratingStamina;

    //References
    public new Transform camera;
    public new AudioCallerScript audio;

    //Data Stache
    [HideInInspector] public new Transform transform;
    [HideInInspector] public Rigidbody rb;
    [HideInInspector] public new Collider collider;
    private InputControls input;
    GameStateManagerScript stateManager;
    bool onGround;
    Vector2 inputDirection;
    Vector2 viewRotation;
    Vector2 viewInput;
    //bool mouseActive = true;

    [HideInInspector] public bool hasKeycard;

    [HideInInspector] public bool canMove = true;





    void Start()
    {
        input = new InputControls();
        input.Enable();
        transform = base.transform;
        rb = GetComponent<Rigidbody>();
        collider= GetComponent<Collider>();
        stateManager = GameStateManagerScript.instance;
        animator = GetComponent<Animator>();

        Cursor.lockState = CursorLockMode.Locked;
        Mouse.current.WarpCursorPosition(new Vector2(0.5f, 0.5f));
        Cursor.visible = false;

        playerStamina = maxStamina;


    }

    void Update()
    {
        //onGround = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, Ground);
        //if (onGround) rb.drag = groundDrag;
        //else rb.drag = 0;



        OtherControls();

        Sprint();

        if (canMove)
        {
            inputDirection = input.Main.Movement.ReadValue<Vector2>();

            LookControls();
        }

        if (interactPressed) InteractAction();

        if(pausePressed) stateManager.TogglePause();
        //if (Input.GetKeyDown(KeyCode.Escape)) Application.Quit();
    }
    private void FixedUpdate()
    {
        Vector3 rotatedDirection = transform.forward * inputDirection.y + transform.right * inputDirection.x;

        //rb.AddForce(rotatedDirection.normalized * moveTopSpeed * 10f, ForceMode.Force); //Nick's OG solution

        //rb.velocity += rotatedDirection * moveAccel; //My First Solution
        //if (rb.velocity.magnitude >= moveTopSpeed) rb.velocity = rb.velocity.normalized * moveTopSpeed;

        rb.velocity = rotatedDirection * currentSpeed; //My Second Solution (Little to no Easing.)

        animator.SetFloat("WalkSpeed", new Vector3(rb.velocity.x, 0, rb.velocity.z).magnitude);
    }

    void LookControls()
    {
        viewInput = input.Main.Looking.ReadValue<Vector2>();
        Vector2 viewInputAdj = viewInput * mouseSensitivity / (Screen.width/10);


        //Debug Function for Deactivating mouse movement when not needed.

        viewRotation.x += viewInputAdj.x;
        viewRotation.y -= viewInputAdj.y;
        

        // Make it so you can't look up/down > 90 degrees
        viewRotation.y = Mathf.Clamp(viewRotation.y, -90f, 90f);

        transform.eulerAngles = Vector3.up * viewRotation.x;
        camera.eulerAngles = transform.eulerAngles + Vector3.right * viewRotation.y;
    }

    private void CapSpeed()
    {
        Vector3 baseVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        // Cap the velocity
        if (baseVel.magnitude > currentSpeed)
        {
            Vector3 cappedVel = baseVel.normalized * currentSpeed;
            rb.velocity = new Vector3(cappedVel.x, rb.velocity.y, cappedVel.z);
        }
    }

    private void Sprint()
    {   
        // Check if the player is moving
        if (inputDirection.x != 0 || inputDirection.y != 0)
        {
            isMoving = true;
        }
        else
        {
            isMoving = false;
        }


        if(playerStamina > 0)
            canSprint = true;
        
        
        if (sprintHeld && canSprint)
        {
            if (sprintHeld)
            {   
                if (regeneratingStamina != null)
                {
                    StopCoroutine(regeneratingStamina);
                    regeneratingStamina = null;
                }

                currentSpeed = sprintTopSpeed;

                if (isMoving)
                {
                    playerStamina -= staminaDrain * Time.deltaTime; 
                }
            }    

            if (playerStamina < 0)
                playerStamina = 0;
        }
        else { currentSpeed = walkTopSpeed; }
        

        if (playerStamina == 0)
            canSprint = false; 

        if (!sprintHeld && playerStamina < maxStamina && regeneratingStamina == null)
        {
            regeneratingStamina = StartCoroutine(RegenStamina());
        }
    }

    private IEnumerator RegenStamina()
    {
        yield return new WaitForSeconds(timeBeforeRegen);
        WaitForSeconds timeToWait = new WaitForSeconds(staminaTimeIncrement);


        while(playerStamina < maxStamina)
        {
            if (playerStamina > 0)
                canSprint = true;

            playerStamina += staminaIncrement;

            if (playerStamina > maxStamina)
                playerStamina = maxStamina;

            yield return timeToWait;
        }

        regeneratingStamina = null;
    }


    void InteractAction()
    {
        Collider[] results = Physics.OverlapBox(camera.position + (camera.forward * (interactSize.z/2)), interactSize/2, camera.rotation, interactableMask);
        if(results.Length > 0)
        {
            Debug.Log("Interacted with Something.");
            results[0].GetComponent<InteractableScript>()?.Interact(this);
            if (results[0].GetComponent<PageItemScript>())
            {
                canMove = !canMove;
                audio.PlaySoundOneShot(3);
            }
            if (results[0].GetComponent<KeycardItemScript>())
            {
                audio.PlaySoundOneShot(2);
            }
        }
    }


    void OtherControls()
    {
        //This is for later once we establish additional functionality.

        jumpPressed = input.Main.Jump.WasPressedThisFrame();
        blinkPressed = input.Main.Blink.WasPressedThisFrame();
        sprintHeld = input.Main.Sprint.IsPressed();
        pausePressed = input.Main.Pause.WasPressedThisFrame();
        interactPressed = input.Main.Interact.WasPressedThisFrame();

    }

    //Other Controls.
    bool sprintHeld;
    bool jumpPressed;
    bool blinkPressed;
    bool pausePressed;
    bool interactPressed;


    public void SetPause(bool value)
    {

    }

    Animator animator;

    public void BeginDeath()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        animator.Play("Death");
        enabled = false;
    }
    public void EndDeath()
    {
        SceneManager.LoadScene(SceneSwap.LoseScene);
    }

}
