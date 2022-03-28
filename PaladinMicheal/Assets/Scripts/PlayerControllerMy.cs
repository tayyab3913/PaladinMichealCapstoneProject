using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControllerMy : MonoBehaviour
{
    public float animationSpeed = 10;
    public float movementSpeed = 10;
    public float rotationSpeed = 100;
    public float desiredSpeed;
    public float currentSpeed;
    public float jumpForce = 30000f;
    public float groundRayCastLength = 1f;
    public LayerMask groundLayerMask;
    public bool isGrounded = false;

    public Transform weapon;
    public Transform handLocation;
    public Transform legLocation;
    public Transform spine;

    public LineRenderer laser;
    public GameObject aimCross;

    private float accelarationPos = 5;
    private float accelarationNeg = 15;
    public Vector2 moveDirection;
    public Vector2 lookDirection;
    private Vector2 lastLookDirection;
    public float jumpDirection;
    private Animator playerAnim;
    private Rigidbody playerRb;
    private bool jumpLaunch = false;

    public float xSensitivity = 0.5f;
    public float ySensitivity = 0.5f;

    public bool IsInputMove
    {
        get { return !Mathf.Approximately(moveDirection.sqrMagnitude, 0f); }
    }


    // Start is called before the first frame update
    void Start()
    {
        playerAnim = GetComponent<Animator>();
        playerRb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if(!playerAnim.GetBool("JumpReady"))
        {
            Movement(moveDirection);
        }
        Jump(jumpDirection);
        GroundCheckRaycast();
        LaserPointRaycast();
    }

    void LateUpdate()
    {
        lastLookDirection += new Vector2(-lookDirection.y * ySensitivity, lookDirection.x * xSensitivity);
        lastLookDirection.x = Mathf.Clamp(lastLookDirection.x, -45, 45);
        lastLookDirection.y = Mathf.Clamp(lastLookDirection.y, -15, 75);
        spine.localEulerAngles = lastLookDirection;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveDirection = context.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        lookDirection = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        jumpDirection = context.ReadValue<float>();
    }

    public void OnEquipped(InputAction.CallbackContext context)
    {
        playerAnim.SetBool("Equiped", !playerAnim.GetBool("Equiped"));
    }

    public void OnShoot(InputAction.CallbackContext context)
    {
        if (!playerAnim.GetBool("Equiped")) return;
        if((int)context.ReadValue<float>() == 1)
        {
            playerAnim.SetTrigger("Shoot");
        }
    }
    public void Movement(Vector2 direction)
    {
        float directionToRotate = moveDirection.x;
        float directionToMove = moveDirection.y;
        if (direction.sqrMagnitude > 1f)
        {
            direction.Normalize();
        }

        desiredSpeed = directionToMove * animationSpeed;
        float accelaration = IsInputMove ? accelarationPos : accelarationNeg;

        currentSpeed = Mathf.MoveTowards(currentSpeed, desiredSpeed, accelaration * Time.deltaTime);
        transform.Rotate(Vector3.up * directionToRotate * rotationSpeed * Time.deltaTime);
        transform.Translate(new Vector3(0, 0, currentSpeed/ animationSpeed) * movementSpeed * Time.deltaTime);
        playerAnim.SetFloat("ForwardSpeed", currentSpeed);

    }

    public void Jump(float direction)
    {
        if(direction > 0 && isGrounded)
        {
            playerAnim.SetBool("JumpReady", true);
            jumpLaunch = true;
        } else if(jumpLaunch && isGrounded)
        {
            playerAnim.SetBool("JumpLaunch", true);
            playerAnim.SetBool("JumpReady", false);
            isGrounded = false;
        }
    }

    public void LaunchJump()
    {
        playerRb.AddForce(Vector3.up * jumpForce);
        playerAnim.SetBool("JumpLand", false);
    }

    public void LandJump()
    {
        isGrounded = true;
        jumpLaunch = false;
        playerAnim.SetBool("JumpLand", true);
        playerAnim.SetBool("JumpReady", false);
        playerAnim.SetBool("JumpLaunch", false);
    }

    void GroundCheckRaycast()
    {
        bool rayCastHit = Physics.Raycast(transform.position, Vector3.down, groundRayCastLength, groundLayerMask);
        if(rayCastHit)
        {
            if(!isGrounded)
            {
                isGrounded = true;
            }
        }
        Debug.DrawRay(transform.position, Vector3.down * groundRayCastLength, Color.red);
    }

    void LaserPointRaycast()
    {
        if (!playerAnim.GetBool("Equiped"))
        {
            laser.gameObject.SetActive(false);
            aimCross.gameObject.SetActive(false);
            return;
        }
        laser.gameObject.SetActive(true);
        aimCross.gameObject.SetActive(true);
        RaycastHit laserHit;
        Ray laserRay = new Ray(laser.transform.position, laser.transform.forward);
        if (Physics.Raycast(laserRay, out laserHit))
        {
            laser.SetPosition(1, laser.transform.InverseTransformPoint(laserHit.point));
            aimCross.transform.localPosition = new Vector3(0, 0, laser.GetPosition(1).z * 0.9f);
        } else
        {
            aimCross.gameObject.SetActive(false);
        }
        laser.gameObject.SetActive(true);
    }

    public void EquipGun()
    {
        weapon.SetParent(handLocation);
        weapon.localPosition = new Vector3(0.072f, -0.032f, 0.022f);
        weapon.localRotation = Quaternion.Euler(6.508f, 88.307f, 100.448f);
        weapon.localScale = new Vector3(1, 1, 1);
    }

    public void UnEquipGun()
    {
        weapon.SetParent(legLocation);
        weapon.localPosition = new Vector3(0.138f, 0.123f, -0.074f);
        weapon.localRotation = Quaternion.Euler(90f, 0f, 0f);
        weapon.localScale = new Vector3(1, 1, 1);
    }
}
