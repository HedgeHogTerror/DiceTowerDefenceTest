using UnityEngine;
using UnityEngine.EventSystems;

public class TowerPlacer : MonoBehaviour
{
    [Header("Tower Placement")]
    [SerializeField] private GameObject[] towerPrefabs;
    [SerializeField] private LayerMask placementLayerMask = 1;
    [SerializeField] private LayerMask obstacleLayerMask = 0;
    [SerializeField] private Material previewMaterial;
    [SerializeField] private Material validPlacementMaterial;
    [SerializeField] private Material invalidPlacementMaterial;
    
    [Header("Placement Settings")]
    [SerializeField] private float placementRange = 100f;
    [SerializeField] private bool showPlacementPreview = true;
    [SerializeField] private bool snapToGrid = false;
    [SerializeField] private float gridSize = 1f;
    
    private int selectedTowerIndex = -1;
    private GameObject previewTower;
    private Camera playerCamera;
    private GameManager gameManager;
    private bool isPlacingTower = false;
    
    public bool IsPlacingTower => isPlacingTower;
    public int SelectedTowerIndex => selectedTowerIndex;
    
    private void Start()
    {
        playerCamera = Camera.main;
        if (playerCamera == null)
        {
            playerCamera = FindObjectOfType<Camera>();
        }
        
        gameManager = GameManager.Instance;
        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
        }
    }
    
    private void Update()
    {
        HandleInput();
        
        if (isPlacingTower)
        {
            UpdatePreview();
        }
    }
    
    private void HandleInput()
    {
        // Cancel placement with right click or escape
        if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
        {
            CancelPlacement();
        }
        
        // Place tower with left click
        if (Input.GetMouseButtonDown(0) && isPlacingTower && !IsPointerOverUI())
        {
            TryPlaceTower();
        }
        
        // Quick select towers with number keys
        for (int i = 1; i <= towerPrefabs.Length && i <= 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha0 + i))
            {
                SelectTower(i - 1);
            }
        }
    }
    
    private void UpdatePreview()
    {
        if (previewTower == null || playerCamera == null) return;
        
        Vector3 mousePosition = Input.mousePosition;
        Ray ray = playerCamera.ScreenPointToRay(mousePosition);
        
        if (Physics.Raycast(ray, out RaycastHit hit, placementRange, placementLayerMask))
        {
            Vector3 targetPosition = hit.point;
            
            previewTower.transform.position = targetPosition;
            previewTower.SetActive(true);
            
            // Update preview material based on placement validity
            bool canPlace = CanPlaceTowerAt(targetPosition);
            UpdatePreviewMaterial(canPlace);
        }
        else
        {
            previewTower.SetActive(false);
        }
    }
    
    private Vector3 SnapToGrid(Vector3 position)
    {
        float snappedX = Mathf.Round(position.x / gridSize) * gridSize;
        float snappedZ = Mathf.Round(position.z / gridSize) * gridSize;
        return new Vector3(snappedX, position.y, snappedZ);
    }
    
    private void UpdatePreviewMaterial(bool canPlace)
    {
        if (previewTower == null) return;
        
        Material materialToUse = canPlace ? validPlacementMaterial : invalidPlacementMaterial;
        if (materialToUse == null) materialToUse = previewMaterial;
        
        Renderer[] renderers = previewTower.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            renderer.material = materialToUse;
        }
    }
    
    public void SelectTower(int towerIndex)
    {
        if (towerIndex < 0 || towerIndex >= towerPrefabs.Length)
        {
            CancelPlacement();
            return;
        }
        
        selectedTowerIndex = towerIndex;
        StartPlacement();
    }
    
    private void StartPlacement()
    {
        if (selectedTowerIndex < 0 || selectedTowerIndex >= towerPrefabs.Length) return;
        
        // Check if player can afford the tower
        Tower towerComponent = towerPrefabs[selectedTowerIndex].GetComponent<Tower>();
        if (towerComponent != null && gameManager != null)
        {
            if (!gameManager.CanAfford(towerComponent.Cost))
            {
                Debug.Log("Not enough money to place this tower!");
                return;
            }
        }
        
        isPlacingTower = true;
        CreatePreview();
    }
    
    private void CreatePreview()
    {
        if (previewTower != null)
        {
            DestroyImmediate(previewTower);
        }
        
        if (selectedTowerIndex >= 0 && selectedTowerIndex < towerPrefabs.Length)
        {
            previewTower = Instantiate(towerPrefabs[selectedTowerIndex]);
            
            // Disable components that shouldn't be active in preview
            Tower towerComponent = previewTower.GetComponent<Tower>();
            if (towerComponent != null)
            {
                towerComponent.enabled = false;
            }
            
            Collider[] colliders = previewTower.GetComponentsInChildren<Collider>();
            foreach (Collider col in colliders)
            {
                col.enabled = false;
            }
            
            // Set preview material
            if (previewMaterial != null)
            {
                Renderer[] renderers = previewTower.GetComponentsInChildren<Renderer>();
                foreach (Renderer renderer in renderers)
                {
                    renderer.material = previewMaterial;
                }
            }
            
            previewTower.SetActive(false);
        }
    }
    
    private void TryPlaceTower()
    {
        if (previewTower == null) return;
        
        Vector3 placementPosition = previewTower.transform.position;
        
        if (!CanPlaceTowerAt(placementPosition))
        {
            Debug.Log("Cannot place tower at this location!");
            return;
        }
        
        // Check if player can afford the tower
        Tower towerComponent = towerPrefabs[selectedTowerIndex].GetComponent<Tower>();
        if (towerComponent != null && gameManager != null)
        {
            if (!gameManager.SpendMoney(towerComponent.Cost))
            {
                Debug.Log("Not enough money to place this tower!");
                return;
            }
        }
        
        // Place the tower
        GameObject newTower = Instantiate(towerPrefabs[selectedTowerIndex], placementPosition, previewTower.transform.rotation);
        
        Debug.Log($"Tower placed at {placementPosition}");
        
        // Continue placing if shift is held, otherwise cancel
        if (!Input.GetKey(KeyCode.LeftShift))
        {
            CancelPlacement();
        }
    }
    
    private bool CanPlaceTowerAt(Vector3 position)
    {
        // Check for obstacles
        Collider[] obstacles = Physics.OverlapSphere(position, 0.5f, obstacleLayerMask);
        if (obstacles.Length > 0)
        {
            return false;
        }
        
        // Check for existing towers
        Collider[] existingTowers = Physics.OverlapSphere(position, 0.5f);
        foreach (Collider col in existingTowers)
        {
            if (col.GetComponent<Tower>() != null)
            {
                return false;
            }
        }
        
        return true;
    }
    
    public void CancelPlacement()
    {
        isPlacingTower = false;
        selectedTowerIndex = -1;
        
        if (previewTower != null)
        {
            DestroyImmediate(previewTower);
            previewTower = null;
        }
    }
    
    private bool IsPointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }
    
    public bool CanAffordTower(int towerIndex)
    {
        if (towerIndex < 0 || towerIndex >= towerPrefabs.Length || gameManager == null)
        {
            return false;
        }
        
        Tower towerComponent = towerPrefabs[towerIndex].GetComponent<Tower>();
        if (towerComponent != null)
        {
            return gameManager.CanAfford(towerComponent.Cost);
        }
        
        return false;
    }
    
    public int GetTowerCost(int towerIndex)
    {
        if (towerIndex < 0 || towerIndex >= towerPrefabs.Length)
        {
            return 0;
        }
        
        Tower towerComponent = towerPrefabs[towerIndex].GetComponent<Tower>();
        return towerComponent != null ? towerComponent.Cost : 0;
    }
    
    public GameObject GetTowerPrefab(int towerIndex)
    {
        if (towerIndex < 0 || towerIndex >= towerPrefabs.Length)
        {
            return null;
        }
        
        return towerPrefabs[towerIndex];
    }
    
    public int GetTowerCount()
    {
        return towerPrefabs.Length;
    }
    
    private void OnDrawGizmos()
    {
        if (snapToGrid && isPlacingTower)
        {
            // Draw grid
            Gizmos.color = Color.white * 0.3f;
            Vector3 center = transform.position;
            
            for (float x = -50; x <= 50; x += gridSize)
            {
                Gizmos.DrawLine(new Vector3(center.x + x, center.y, center.z - 50), 
                               new Vector3(center.x + x, center.y, center.z + 50));
            }
            
            for (float z = -50; z <= 50; z += gridSize)
            {
                Gizmos.DrawLine(new Vector3(center.x - 50, center.y, center.z + z), 
                               new Vector3(center.x + 50, center.y, center.z + z));
            }
        }
    }
}
