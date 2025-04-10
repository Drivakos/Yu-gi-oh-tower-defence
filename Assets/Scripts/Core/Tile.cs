using UnityEngine;

namespace YuGiOhTowerDefense.Core
{
    public class Tile : MonoBehaviour
    {
        private Vector2Int gridPosition;
        private Card occupiedCard;
        private MeshRenderer meshRenderer;
        
        public Vector2Int GridPosition => gridPosition;
        public bool IsOccupied => occupiedCard != null;
        public Card OccupiedCard => occupiedCard;
        
        private void Awake()
        {
            meshRenderer = GetComponent<MeshRenderer>();
            if (meshRenderer == null)
            {
                meshRenderer = gameObject.AddComponent<MeshRenderer>();
            }
            
            // Add a simple quad mesh for visualization
            MeshFilter meshFilter = GetComponent<MeshFilter>();
            if (meshFilter == null)
            {
                meshFilter = gameObject.AddComponent<MeshFilter>();
                meshFilter.mesh = CreateQuadMesh();
            }
        }
        
        public void Initialize(Vector2Int position, Material defaultMaterial)
        {
            gridPosition = position;
            SetMaterial(defaultMaterial);
        }
        
        public void Occupy(Card card)
        {
            occupiedCard = card;
            card.transform.parent = transform;
        }
        
        public void Vacate()
        {
            if (occupiedCard != null)
            {
                occupiedCard.transform.parent = null;
                occupiedCard = null;
            }
        }
        
        public void SetMaterial(Material material)
        {
            if (meshRenderer != null)
            {
                meshRenderer.material = material;
            }
        }
        
        private Mesh CreateQuadMesh()
        {
            Mesh mesh = new Mesh();
            
            Vector3[] vertices = new Vector3[4]
            {
                new Vector3(-0.5f, 0, -0.5f),
                new Vector3(0.5f, 0, -0.5f),
                new Vector3(-0.5f, 0, 0.5f),
                new Vector3(0.5f, 0, 0.5f)
            };
            
            int[] triangles = new int[6]
            {
                0, 2, 1,
                2, 3, 1
            };
            
            Vector2[] uv = new Vector2[4]
            {
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(1, 1)
            };
            
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uv;
            mesh.RecalculateNormals();
            
            return mesh;
        }
    }
} 