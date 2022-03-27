using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControllerMy : MonoBehaviour
{
    public float movementSpeed = 10;
    public float rotationSpeed = 100;
    public float desiredSpeed;
    public float currentSpeed;
    public float jumpForce = 30000f;
    public float groundRayCastLength = 1f;
    public LayerMask groundLayerMask;
    public bool isGrounded = false;

    private float accelarationPos = 5;
    private float accelarationNeg = 15;
    public Vector2 moveDirection;
    public float jumpDirection;
    private Animator playerAnim;
    private Rigidbody playerRb;
    private bool jumpLaunch = false;
    
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
        Movement(moveDirection);
        Jump(jumpDirection);
        GroundCheckRaycast();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveDirection = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        jumpDirection = context.ReadValue<float>();
    }

    public void Movement(Vector2 direction)
    {
        float directionToRotate = moveDirection.x;
        float directionToMove = moveDirection.y;
        if (direction.sqrMagnitude > 1f)
        {
            direction.Normalize();
        }

        desiredSpeed = direction.magnitude * movementSpeed * Mathf.Sign(directionToMove);
        float accelaration = IsInputMove ? accelarationPos : accelarationNeg;

        currentSpeed = Mathf.MoveTowards(currentSpeed, desiredSpeed, accelaration * Time.deltaTime);
        transform.Rotate(Vector3.up * directionToRotate * rotationSpeed * Time.deltaTime);

        playerAnim.SetFloat("ForwardSpeed", currentSpeed);

        //transform.Translate(new Vector3(moveDirection.x, 0, moveDirection.y) * movementSpeed * Time.deltaTime);
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
        Debug.Log("Land Called");
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
}
