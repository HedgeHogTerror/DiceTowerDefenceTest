using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Orbit Settings")]
    [SerializeField] private Transform target; // Center point to orbit around
    [SerializeField] private float baseDistance = 10f; // Default camera distance
    [SerializeField] private float horizontalSpeed = 100f; // Rotation speed left/right
    [SerializeField] private float verticalSpeed = 80f; // Rotation speed up/down
    
    [Header("Zoom Settings")]
    [SerializeField] private float zoomSpeed = 2f; // Zoom sensitivity
    [SerializeField] private float minZoomFactor = 0.5f; // Closest zoom (0.5x)
    [SerializeField] private float maxZoomFactor = 2f; // Farthest zoom (2x)
    
    [Header("Limits")]
    [SerializeField] private float minVerticalAngle = -80f; // Prevent camera from going too low
    [SerializeField] private float maxVerticalAngle = 80f; // Prevent camera from going too high
    
    [Header("Smoothing")]
    [SerializeField] private float movementSmoothing = 5f; // How smooth the rotation is
    [SerializeField] private float zoomSmoothing = 8f; // How smooth the zoom is
    
    [Header("Input Keys")]
    [SerializeField] private KeyCode zoomInKey = KeyCode.Q;
    [SerializeField] private KeyCode zoomOutKey = KeyCode.E;
    
    // Private variables
    private float currentHorizontalAngle;
    private float currentVerticalAngle;
    private float currentZoomFactor = 1f;
    private float targetHorizontalAngle;
    private float targetVerticalAngle;
    private float targetZoomFactor = 1f;
    
    private Camera cameraComponent;
    private GameManager gameManager;
    private Vector3 targetPosition;
    
    // Properties
    public float CurrentZoom => currentZoomFactor;
    public Vector3 TargetPosition => targetPosition;
    
    private void Start()
    {
        InitializeCamera();
        FindOrCreateTarget();
        SetInitialPosition();
    }
    
    private void Update()
    {
        // Don't update camera if game is paused
        if (gameManager != null && gameManager.IsGamePaused)
            return;
            
        HandleInput();
        UpdateCameraPosition();
    }
    
    private void InitializeCamera()
    {
        cameraComponent = GetComponent<Camera>();
        if (cameraComponent == null)
        {
            Debug.LogError("CameraController: No Camera component found!");
            enabled = false;
            return;
        }
        
        gameManager = GameManager.Instance;
        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
        }
    }
    
    private void FindOrCreateTarget()
    {
        if (target == null)
        {
            // Try to find a logical center point for the game field
            // Look for objects that might represent the game center
            GameObject centerObject = GameObject.Find("GameCenter");
            if (centerObject == null)
                centerObject = GameObject.Find("Center");
            if (centerObject == null)
                centerObject = GameObject.Find("Map");
            
            if (centerObject != null)
            {
                target = centerObject.transform;
            }
            else
            {
                // Create a center point at world origin
                GameObject newTarget = new GameObject("CameraTarget");
                newTarget.transform.position = Vector3.zero;
                target = newTarget.transform;
                Debug.Log("CameraController: Created camera target at world origin. You can move this object to set the orbit center.");
            }
        }
        
        targetPosition = target.position;
    }
    
    private void SetInitialPosition()
    {
        if (target == null) return;
        
        // Calculate initial angles based on current camera position relative to target
        Vector3 offset = transform.position - target.position;
        
        if (offset.magnitude < 0.1f)
        {
            // Camera is too close to target, set default position
            offset = new Vector3(0, 5, -10);
            transform.position = target.position + offset;
        }
        
        // Calculate angles from current position
        float distance = offset.magnitude;
        baseDistance = distance; // Use current distance as base
        
        currentHorizontalAngle = Mathf.Atan2(offset.x, offset.z) * Mathf.Rad2Deg;
        currentVerticalAngle = Mathf.Asin(offset.y / distance) * Mathf.Rad2Deg;
        
        // Clamp vertical angle to limits
        currentVerticalAngle = Mathf.Clamp(currentVerticalAngle, minVerticalAngle, maxVerticalAngle);
        
        // Set target angles to current angles
        targetHorizontalAngle = currentHorizontalAngle;
        targetVerticalAngle = currentVerticalAngle;
        
        // Make camera look at target
        transform.LookAt(target);
    }
    
    private void HandleInput()
    {
        // Rotation input (WASD)
        float horizontalInput = 0f;
        float verticalInput = 0f;
        if (Input.GetKey(KeyCode.D))
            horizontalInput = -1f;
        if (Input.GetKey(KeyCode.A))
            horizontalInput = 1f;
        if (Input.GetKey(KeyCode.W))
            verticalInput = 1f;
        if (Input.GetKey(KeyCode.S))
            verticalInput = -1f;
        
        // Apply rotation input
        if (Mathf.Abs(horizontalInput) > 0.1f)
        {
            targetHorizontalAngle += horizontalInput * horizontalSpeed * Time.deltaTime;
        }
        
        if (Mathf.Abs(verticalInput) > 0.1f)
        {
            targetVerticalAngle += verticalInput * verticalSpeed * Time.deltaTime;
            targetVerticalAngle = Mathf.Clamp(targetVerticalAngle, minVerticalAngle, maxVerticalAngle);
        }
        
        // Zoom input (Mouse scroll wheel)
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scrollInput) > 0.01f)
        {
            targetZoomFactor -= scrollInput * zoomSpeed;
        }
        
        // Zoom input (Q/E keys)
        if (Input.GetKey(zoomInKey))
        {
            targetZoomFactor -= zoomSpeed * Time.deltaTime;
        }
        if (Input.GetKey(zoomOutKey))
        {
            targetZoomFactor += zoomSpeed * Time.deltaTime;
        }
        
        // Clamp zoom factor
        targetZoomFactor = Mathf.Clamp(targetZoomFactor, minZoomFactor, maxZoomFactor);
    }
    
    private void UpdateCameraPosition()
    {
        if (target == null) return;
        
        // Update target position in case the target moves
        targetPosition = target.position;
        
        // Smoothly interpolate angles and zoom
        currentHorizontalAngle = Mathf.LerpAngle(currentHorizontalAngle, targetHorizontalAngle, movementSmoothing * Time.deltaTime);
        currentVerticalAngle = Mathf.Lerp(currentVerticalAngle, targetVerticalAngle, movementSmoothing * Time.deltaTime);
        currentZoomFactor = Mathf.Lerp(currentZoomFactor, targetZoomFactor, zoomSmoothing * Time.deltaTime);
        
        // Calculate new position using spherical coordinates
        float currentDistance = baseDistance * currentZoomFactor;
        
        // Convert angles to radians
        float horizontalRad = currentHorizontalAngle * Mathf.Deg2Rad;
        float verticalRad = currentVerticalAngle * Mathf.Deg2Rad;
        
        // Calculate position offset from target
        Vector3 offset = new Vector3(
            Mathf.Sin(horizontalRad) * Mathf.Cos(verticalRad),
            Mathf.Sin(verticalRad),
            Mathf.Cos(horizontalRad) * Mathf.Cos(verticalRad)
        ) * currentDistance;
        
        // Set camera position and rotation
        transform.position = targetPosition + offset;
        transform.LookAt(targetPosition);
    }
    
    // Public methods for external control
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        if (target != null)
        {
            targetPosition = target.position;
        }
    }
    
    public void SetZoom(float zoomFactor)
    {
        targetZoomFactor = Mathf.Clamp(zoomFactor, minZoomFactor, maxZoomFactor);
    }
    
    public void ResetCamera()
    {
        targetHorizontalAngle = 0f;
        targetVerticalAngle = 20f; // Slight downward angle
        targetZoomFactor = 1f;
    }
    
    public void FocusOnPosition(Vector3 position, float duration = 1f)
    {
        // This could be extended to smoothly move the target position
        if (target != null)
        {
            target.position = position;
            targetPosition = position;
        }
    }
    
    // Debug visualization
    private void OnDrawGizmosSelected()
    {
        if (target == null) return;
        
        // Draw orbit center
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(target.position, 0.5f);
        
        // Draw zoom range
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(target.position, baseDistance * minZoomFactor);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(target.position, baseDistance * maxZoomFactor);
        
        // Draw line from camera to target
        Gizmos.color = Color.white;
        Gizmos.DrawLine(transform.position, target.position);
    }
}
