using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// enumeration of different player states that affect accel, drag and movement
// abilities
enum playerState
{
    Ground,
    Steep,
    Glide,
    Rise,
    Fall
}
public class PlayerControl : MonoBehaviour
{   
    [Header("Lateral Movemnt")]
    [SerializeField] private float maxSpeed = 5f;
    private float maxSpeedSq;
    [SerializeField] private Transform orientation;

    [Header("Jumping")]
    [SerializeField] private float jumpCooldown = 0.5f;
    [SerializeField] private float jumpForce = 5f;
    public KeyCode jumpKey;
    [SerializeField] private bool canJump = true;

    [Header("Gliding")]
    [SerializeField] private float glideGravProp = 0.8f;
    [SerializeField] private bool canGlide = false;

    [Header("Checks for Groundedness")]
    [SerializeField] private float heightEpsilon = 0.005f;
    [SerializeField] private float playerHeight;    
    public LayerMask groundMask;

    [Header("Walking on Slopes")]
    [SerializeField] private float maxAngle = 30f;
    [SerializeField] private float slopeThresholdAngle = 5f;
    [SerializeField] private float playerLength;
    [SerializeField] private RaycastHit slopeHit;
    [SerializeField] private float slopeAngle;

    [Header("Player State")]
    [SerializeField] private playerState state = playerState.Ground;

    [Header("Acceleration Constants")]
    [SerializeField] private float groundAccel = 25f;
    [SerializeField] private float airAccel = 5f;
    
    [Header("Drag Constants")]
    [SerializeField] private float groundDrag = 1f;
    [SerializeField] private float airDrag = 0.2f;

    // Audio Components. Ideas were studied, referenced and adapted from https://gamedevacademy.org/unity-audio-tutorial/
    [Header("Audio Components")]
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip jumpSound;
    [SerializeField] AudioClip[] footstepSounds;
    // determines the rate at which to play footstep audio clips
    [SerializeField] float playFootstepAudioRate; 
    // determines the threshold for the player's velocity at which footstep audio clips should be played
    [SerializeField] float playerVelocityFootstepThreshold; 

    

    // storing inputs
    private float sidewaysInput;
    private float forwardsInput;
    private bool jumpInput;

    private Vector3 playerDir;
    private Rigidbody rigidBody;
    private float accel;

    // Some variables to track for playing audio
    private float lastFootstepTime;

    // store inputs from movement keys
    private void ProcessInput()
    {
        forwardsInput = Input.GetAxisRaw("Vertical");
        sidewaysInput = Input.GetAxisRaw("Horizontal");
        jumpInput = Input.GetKey(jumpKey);
    }

    // move player according to movement keys and current player state
    private void MovePlayer()
    {
        // calculate direction of player based on move keys and orientation
        playerDir = orientation.forward * forwardsInput + orientation.right * sidewaysInput;
        
        // project direction onto plane of a slope, if on slope
        // (disable gravity while on slope, to prevent sliding down,
        // may be replaced with explicit slope type state handling)
        if (slopeAngle > slopeThresholdAngle && slopeAngle < maxAngle) {
            //rigidBody.useGravity = false;
            playerDir = Vector3.ProjectOnPlane(playerDir, slopeHit.normal);
        }
        else {
            //rigidBody.useGravity = true;
        }

        Debug.DrawLine(transform.position, transform.position + playerDir * 5, Color.green, 0.05f, false);

        rigidBody.AddForce(playerDir.normalized * rigidBody.mass * accel);
        
        // If the player's acceleration is higher than the threshold parameter and the time elapsed since
        // the last footstep sound was played is sufficient, then log the current time (to be used for the
        // next footstep sound) and play a random footstep sound clip.
        if(rigidBody.velocity.magnitude > playerVelocityFootstepThreshold && (Time.time - lastFootstepTime) > playFootstepAudioRate) {
            Vector3 vertVel = Vector3.up * rigidBody.velocity.y;
            
            if(state == playerState.Ground) {
                lastFootstepTime = Time.time;
                audioSource.PlayOneShot(footstepSounds[Random.Range(0, footstepSounds.Length-1)]);
            }
        }

    }

    // limit non vertical speed to a designated max speed
    private void SpeedLimiter()
    {
        Vector3 vertVel = Vector3.up * rigidBody.velocity.y;
        Vector3 flatVel = rigidBody.velocity - vertVel;
        if (flatVel.sqrMagnitude > maxSpeedSq) {
            rigidBody.velocity = flatVel.normalized * maxSpeed + vertVel;
        }
    }

    //
    private bool GroundCheck()
    {
        // shoot ray downward, only as far as player could be standing on
        // steepest walkable slope
        slopeAngle = 0;
        if (Physics.Raycast(
                transform.position,
                Vector3.down,
                out slopeHit,
                0.5f * (playerHeight + playerLength * Mathf.Tan((maxAngle * 180) / Mathf.PI)) + heightEpsilon,
                groundMask
            ))
        {
            // calculate angle of slope
            slopeAngle = Vector3.Angle(Vector3.up, slopeHit.normal);

            if (slopeAngle > maxAngle) {
                return false;
            }

            // calculate maximum distance away from surface according to slope angle
            float maxDist = 0.5f * (playerHeight + playerLength * Mathf.Tan((slopeAngle * 180) / Mathf.PI)) + heightEpsilon;

            // if slope isnt too steep
            if (slopeHit.distance < maxDist) {
                return true;
            }
        }
        return false;
    }

    // cause the player to jump
    private void Jump()
    {
        // immediately disallow jumping
        canJump = false;

        audioSource.PlayOneShot(jumpSound);

        // set y velocity to 0 to make every jump the same height
        rigidBody.velocity = new Vector3(rigidBody.velocity.x, 0, rigidBody.velocity.z);
        
        // add upward force
        rigidBody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    // cause the player to glide, reduces gravity by glideGravProp*100 percent
    // for fixed timestep in which its called
    private void Glide()
    {
        rigidBody.AddForce(Physics.gravity * rigidBody.mass * -glideGravProp);
    }

    // allow player to jump again
    private void MakeReadyToJump()
    {
        canJump = true;
    }

    // set accel and drag multipliers according to player state
    private void SetAccelAndDrag()
    {
        if (state == playerState.Ground) {
            accel = groundAccel;
            rigidBody.drag = groundDrag;
        } else {
            accel = airAccel;
            rigidBody.drag = airDrag;
        } 
    }
    
    private void Start()
    {
        // store rigidbody and disallow rotation, allow jumping from start
        Cursor.lockState = CursorLockMode.Locked;
        rigidBody = GetComponent<Rigidbody>();
        rigidBody.freezeRotation = true;
        canJump = true;
        canGlide = false;
        maxSpeedSq = maxSpeed * maxSpeed;
    }

    private void Update()
    {
        // process movement input keys
        ProcessInput();

        // store if player is on the ground for current frame
        if (GroundCheck() && state != playerState.Rise) {
            state = playerState.Ground;
            canGlide = false;
        } 
        else if (state == playerState.Ground) {
            if (rigidBody.velocity.y > 0) {
                state = playerState.Rise;
            }
            else {
                state = playerState.Fall;
            }        
        }

        // when player starts falling, set new state and allow gliding
        if (state == playerState.Rise & rigidBody.velocity.y < 0) {
            state = playerState.Fall;
            canGlide = true;
        }

        if (jumpInput) {

            // if player is on ground and can jump, jump
            if (state == playerState.Ground && canJump) {

                Jump();

                // set new player state
                state = playerState.Rise;

                // disallow jumping for set time
                Invoke(nameof(MakeReadyToJump), jumpCooldown);
            }
            // if player is not on ground and can glide, glide
            else if (canGlide) {
                state = playerState.Glide;
            }
        }
        // if jump key is not pressed and player was gliding, change state
        // to falling and disallow gliding
        else if (state == playerState.Glide) {
            state = playerState.Fall;
            canGlide = false;
        }
        
        // set drag and acceleration values according to air/ground state
        SetAccelAndDrag();
    }

    private void FixedUpdate()
    {
        // move player
        MovePlayer();
        
        // glide if player is in glide player state
        if (state == playerState.Glide) Glide();
    }

    // When player moves their mouse horizontally, Chiki also turns, and the camera turns along with Chiki
    // This section of Chiki + camera control was referenced, studied and adapted for use from the following resources:
    // https://answers.unity.com/questions/1179680/how-to-rotate-my-camera.html
    // https://gamedevacademy.org/unity-audio-tutorial/
    // https://docs.unity3d.com/ScriptReference/Vector3.html
    // https://docs.unity3d.com/ScriptReference/Transform-eulerAngles.html
    private void LateUpdate()
    {
        // trigger speed limiter to limit speed if needed
        SpeedLimiter();

        float horizontal_movement_x = Input.GetAxis("Mouse X");
        
        // Doing a rotation around the y-axis makes the camera rotate the view horizontally. Vector3.up is a 
        // shorthand way of writing (0, 1, 0). The code is rotating the y-axis using the horizontal
        // movement of the player's mouse.
        transform.eulerAngles += Vector3.up * horizontal_movement_x; 
    }

}