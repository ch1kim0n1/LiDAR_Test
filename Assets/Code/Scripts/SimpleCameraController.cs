//by ch1kim0n1

using UnityEngine;


public class SimpleCameraController : MonoBehaviour
{
    // Represents the state of the camera (position and rotation)
    class CameraState
    {
        public float yaw;
        public float pitch;
        public float roll;
        public float x;
        public float y;
        public float z;

        // Sets the camera state from a Transform
        public void SetFromTransform(Transform t)
        {
            pitch = t.eulerAngles.x;
            yaw = t.eulerAngles.y;
            roll = t.eulerAngles.z;
            x = t.position.x;
            y = t.position.y;
            z = t.position.z;
        }

        // Translates the camera state by a given translation vector
        public void Translate(Vector3 translation)
        {
            Vector3 rotatedTranslation = Quaternion.Euler(pitch, yaw, roll) * translation;

            x += rotatedTranslation.x;
            y += rotatedTranslation.y;
            z += rotatedTranslation.z;
        }

        // Interpolates the camera state towards a target state
        public void LerpTowards(CameraState target, float positionLerpPct, float rotationLerpPct)
        {
            yaw = Mathf.Lerp(yaw, target.yaw, rotationLerpPct);
            pitch = Mathf.Lerp(pitch, target.pitch, rotationLerpPct);
            roll = Mathf.Lerp(roll, target.roll, rotationLerpPct);

            x = Mathf.Lerp(x, target.x, positionLerpPct);
            y = Mathf.Lerp(y, target.y, positionLerpPct);
            z = Mathf.Lerp(z, target.z, positionLerpPct);
        }

        // Updates the Transform with the camera state
        public void UpdateTransform(Transform t)
        {
            t.eulerAngles = new Vector3(pitch, yaw, roll);
            t.position = new Vector3(x, y, z);
        }
    }

    const float k_MouseSensitivityMultiplier = 0.01f;

    CameraState m_TargetCameraState = new CameraState();
    CameraState m_InterpolatingCameraState = new CameraState();

    [Header("Movement Settings")]
    [Tooltip("Exponential boost factor on translation, controllable by mouse wheel.")]
    public float boost = 3.5f;

    [Tooltip("Time it takes to interpolate camera position 99% of the way to the target."), Range(0.001f, 1f)]
    public float positionLerpTime = 0.2f;

    [Header("Rotation Settings")]
    [Tooltip("Multiplier for the sensitivity of the rotation.")]
    public float mouseSensitivity = 60.0f;

    [Tooltip("X = Change in mouse position.\nY = Multiplicative factor for camera rotation.")]
    public AnimationCurve mouseSensitivityCurve = new AnimationCurve(new Keyframe(0f, 0.5f, 0f, 5f), new Keyframe(1f, 2.5f, 0f, 0f));

    [Tooltip("Time it takes to interpolate camera rotation 99% of the way to the target."), Range(0.001f, 1f)]
    public float rotationLerpTime = 0.01f;

    [Tooltip("Whether or not to invert our Y axis for mouse input to rotation.")]
    public bool invertY = false;

    void OnEnable()
    {
        // Initialize camera states from the current transform
        m_TargetCameraState.SetFromTransform(transform);
        m_InterpolatingCameraState.SetFromTransform(transform);
    }

    // Get the translation direction based on input
    Vector3 GetInputTranslationDirection()
    {
        Vector3 direction = Vector3.zero;
        if (Input.GetKey(KeyCode.W))
        {
            direction += Vector3.forward;
        }
        if (Input.GetKey(KeyCode.S))
        {
            direction += Vector3.back;
        }
        if (Input.GetKey(KeyCode.A))
        {
            direction += Vector3.left;
        }
        if (Input.GetKey(KeyCode.D))
        {
            direction += Vector3.right;
        }
        if (Input.GetKey(KeyCode.Q))
        {
            direction += Vector3.down;
        }
        if (Input.GetKey(KeyCode.E))
        {
            direction += Vector3.up;
        }
        return direction;
    }

    void Update()
    {
        // Exit the application if the escape key is pressed
        if (IsEscapePressed())
        {
            Application.Quit();
        }

        // Hide and lock the cursor when the right mouse button is pressed
        if (IsRightMouseButtonDown())
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

        // Unlock and show the cursor when the right mouse button is released
        if (IsRightMouseButtonUp())
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        // Rotation
        if (IsCameraRotationAllowed())
        {
            var mouseMovement = GetInputLookRotation() * k_MouseSensitivityMultiplier * mouseSensitivity;
            if (invertY)
                mouseMovement.y = -mouseMovement.y;

            // Apply mouse sensitivity curve to the mouse movement
            var mouseSensitivityFactor = mouseSensitivityCurve.Evaluate(mouseMovement.magnitude);

            // Update the target camera state with the mouse movement
            m_TargetCameraState.yaw += mouseMovement.x * mouseSensitivityFactor;
            m_TargetCameraState.pitch += mouseMovement.y * mouseSensitivityFactor;
        }

        // Translation
        var translation = GetInputTranslationDirection() * Time.deltaTime;

        // Speed up movement when the boost key is held
        if (IsBoostPressed())
        {
            translation *= 10.0f;
        }

        // Modify the movement by a boost factor
        boost += GetBoostFactor();
        translation *= Mathf.Pow(2.0f, boost);

        // Translate the target camera state
        m_TargetCameraState.Translate(translation);

        // Framerate-independent interpolation
        // Calculate the lerp amount, such that we get 99% of the way to our target in the specified time
        var positionLerpPct = 1f - Mathf.Exp((Mathf.Log(1f - 0.99f) / positionLerpTime) * Time.deltaTime);
        var rotationLerpPct = 1f - Mathf.Exp((Mathf.Log(1f - 0.99f) / rotationLerpTime) * Time.deltaTime);

        // Interpolate between the current camera state and the target camera state
        m_InterpolatingCameraState.LerpTowards(m_TargetCameraState, positionLerpPct, rotationLerpPct);

        // Update the transform of the camera with the interpolated camera state
        m_InterpolatingCameraState.UpdateTransform(transform);
    }

    // Get the boost factor based on input
    float GetBoostFactor()
    {
        return Input.mouseScrollDelta.y * 0.01f;
    }

    // Get the input for camera rotation
    Vector2 GetInputLookRotation()
    {
        return new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
    }

    // Check if the boost key is pressed
    bool IsBoostPressed()
    {
        return Input.GetKey(KeyCode.LeftShift);
    }

    // Check if the escape key is pressed
    bool IsEscapePressed()
    {
        return Input.GetKey(KeyCode.Escape);
    }

    // Check if camera rotation is allowed
    bool IsCameraRotationAllowed()
    {
        return Input.GetMouseButton(1);
    }

    // Check if the right mouse button is pressed
    bool IsRightMouseButtonDown()
    {
        return Input.GetMouseButtonDown(1);
    }

    // Check if the right mouse button is released
    bool IsRightMouseButtonUp()
    {
        return Input.GetMouseButtonUp(1);
    }
}

