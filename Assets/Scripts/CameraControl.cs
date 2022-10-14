using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{   
    [SerializeField] private float max_up_rotation_angle;
    [SerializeField] private float max_down_rotation_angle;

    // Locks the cursor to the game screen so that Unity
    // can use the Player's mouse movements for
    // camera rotation
    void Start() {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Perform camera rotation on LateUpdate so player's position is updated first before applying changes to the camera
    // This section of camera control was referenced, studied and adapted for use from the following resources:
    // https://answers.unity.com/questions/1179680/how-to-rotate-my-camera.html
    // https://gamedevacademy.org/unity-audio-tutorial/
    // https://docs.unity3d.com/ScriptReference/Vector3.html
    // https://docs.unity3d.com/ScriptReference/Transform-eulerAngles.html
    void LateUpdate() {
        float vertical_movement_y = Input.GetAxis("Mouse Y");
        
        // Using the vertical movement of the mouse to rotate about the x-axis (vertical rotation)
        // the vertical movement value is negated due to Unity's LHS coordinate system having counterintuitive
        // rotations. Essentially, negating it makes it so that moving the mouse upwards nets an upwards 
        // rotation and vice versa. The maximum rotation angles are determined by 2 parameters which are 
        // serialized and can be set in Unity.
        float post_rotation_x = transform.eulerAngles.x + -vertical_movement_y;

        // Apply rotation if the rotation is not past the maximum upwards or downwards rotation angles.
        if((post_rotation_x >= (360 - max_up_rotation_angle)) || (post_rotation_x <= max_down_rotation_angle)) {
            transform.eulerAngles += Vector3.right * -vertical_movement_y;
        }
    
    }
}