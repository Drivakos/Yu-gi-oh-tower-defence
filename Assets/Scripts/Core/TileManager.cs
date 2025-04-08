using UnityEngine;
using System.Collections.Generic;
using YuGiOhTowerDefense.Monsters;

namespace YuGiOhTowerDefense.Core
{
    public class TileManager : MonoBehaviour
    {
        [Header("Tile Settings")]
        [SerializeField] private GameObject tilePrefab;
        [SerializeField] private int gridWidth = 10;
        [SerializeField] private int gridHeight = 10;
        [SerializeField] private float tileSize = 1f;
        [SerializeField] private Color validPlacementColor = new Color(0f, 1f, 0f, 0.5f);
        [SerializeField] private Color invalidPlacementColor = new Color(1f, 0f, 0f, 0.5f);
        [SerializeField] private Color defaultTileColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        
        [Header("References")]
        [SerializeField] private Transform tileContainer;
        [SerializeField] private Transform monsterContainer;
        
        private Tile[,] tiles;
        private Monster selectedMonster;
        private GameObject placementPreview;
        private bool isPlacingMonster;
        
        private void Start()
        {
            CreateGrid();
        }
        
        private void Update()
        {
            if (isPlacingMonster && selectedMonster != null)
            {
                UpdatePlacementPreview();
                
                if (Input.GetMouseButtonDown(0))
                {
                    TryPlaceMonster();
                }
                
                if (Input.GetMouseButtonDown(1))
                {
                    CancelPlacement();
                }
            }
        }
        
        private void CreateGrid()
        {
            tiles = new Tile[gridWidth, gridHeight];
            
            for (int x = 0; x < gridWidth; x++)
            {
                for (int z = 0; z < gridHeight; z++)
                {
                    Vector3 position = new Vector3(x * tileSize, 0, z * tileSize);
                    GameObject tileObj = Instantiate(tilePrefab, position, Quaternion.identity, tileContainer);
                    tileObj.name = $"Tile_{x}_{z}";
                    
                    Tile tile = tileObj.GetComponent<Tile>();
                    if (tile != null)
                    {
                        tile.Initialize(x, z);
                        tiles[x, z] = tile;
                    }
                }
            }
        }
        
        public void StartMonsterPlacement(Monster monster)
        {
            if (isPlacingMonster)
            {
                CancelPlacement();
            }
            
            selectedMonster = monster;
            isPlacingMonster = true;
            
            // Create placement preview
            placementPreview = new GameObject("PlacementPreview");
            placementPreview.transform.parent = monsterContainer;
            
            // Add visual components to preview
            MeshRenderer renderer = placementPreview.AddComponent<MeshRenderer>();
            MeshFilter filter = placementPreview.AddComponent<MeshFilter>();
            
            // Use a simple cube for preview
            filter.mesh = GameObject.CreatePrimitive(PrimitiveType.Cube).GetComponent<MeshFilter>().sharedMesh;
            renderer.material = new Material(Shader.Find("Standard"));
            renderer.material.color = validPlacementColor;
            
            // Scale to match tile size
            placementPreview.transform.localScale = new Vector3(tileSize * 0.8f, tileSize * 0.8f, tileSize * 0.8f);
        }
        
        private void UpdatePlacementPreview()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit) && hit.collider.CompareTag("Tile"))
            {
                Tile tile = hit.collider.GetComponent<Tile>();
                if (tile != null)
                {
                    // Position preview on tile
                    Vector3 position = tile.transform.position + Vector3.up * (tileSize * 0.5f);
                    placementPreview.transform.position = position;
                    
                    // Update color based on placement validity
                    bool canPlace = CanPlaceMonsterOnTile(tile);
                    placementPreview.GetComponent<MeshRenderer>().material.color = 
                        canPlace ? validPlacementColor : invalidPlacementColor;
                }
            }
        }
        
        private bool CanPlaceMonsterOnTile(Tile tile)
        {
            if (tile == null || tile.IsOccupied()) return false;
            
            // Check if player can afford the monster
            if (!GameManager.Instance.CanAffordMonster(selectedMonster.GetStats().cost)) return false;
            
            return true;
        }
        
        private void TryPlaceMonster()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit) && hit.collider.CompareTag("Tile"))
            {
                Tile tile = hit.collider.GetComponent<Tile>();
                if (tile != null && CanPlaceMonsterOnTile(tile))
                {
                    PlaceMonsterOnTile(tile);
                }
            }
        }
        
        private void PlaceMonsterOnTile(Tile tile)
        {
            // Instantiate the monster
            GameObject monsterObj = Instantiate(selectedMonster.gameObject, monsterContainer);
            monsterObj.transform.position = tile.transform.position + Vector3.up * (tileSize * 0.5f);
            
            // Register with game manager
            Monster monster = monsterObj.GetComponent<Monster>();
            if (monster != null)
            {
                GameManager.Instance.RegisterMonster(monster);
                GameManager.Instance.SpendDuelPoints(monster.GetStats().cost);
            }
            
            // Mark tile as occupied
            tile.SetOccupied(true);
            
            // End placement
            CancelPlacement();
        }
        
        private void CancelPlacement()
        {
            isPlacingMonster = false;
            selectedMonster = null;
            
            if (placementPreview != null)
            {
                Destroy(placementPreview);
                placementPreview = null;
            }
        }
        
        public Tile GetTileAt(int x, int z)
        {
            if (x >= 0 && x < gridWidth && z >= 0 && z < gridHeight)
            {
                return tiles[x, z];
            }
            return null;
        }
        
        public Vector3 GetTilePosition(int x, int z)
        {
            return new Vector3(x * tileSize, 0, z * tileSize);
        }
    }
} 