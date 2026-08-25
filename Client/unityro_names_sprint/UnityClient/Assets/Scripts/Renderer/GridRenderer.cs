using UnityEngine;
using UnityEngine.AddressableAssets;

public class GridRenderer : MonoBehaviour {

    private Texture2D gridIcon;

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private Mesh mesh;
    private Material material;

    private Vector3[] vertices;
    private Vector2[] uvs;
    private int[] triangles;
    public bool IsCurrentPositionValid { get; private set; }

    private PathFinder PathFinder;

    private void Awake() {
        DontDestroyOnLoad(this);
        PathFinder = FindObjectOfType<PathFinder>();
        transform.localPosition = new Vector3(0.5f, 0f, 0.5f);
    }

    public async void Start() {
        gridIcon = await Addressables.LoadAssetAsync<Texture2D>("data/texture/grid.png").Task;
        LoadGridTexture();
    }

    /// <summary>
    /// Receives the ground hit from EntityControl so pointer selection only performs
    /// one physics raycast per frame for gameplay targeting/movement.
    /// </summary>
    public void SetPointerWorldPosition(Vector3 worldPosition) {
        if (PathFinder == null) {
            PathFinder = FindObjectOfType<PathFinder>();
        }

        if (PathFinder == null || meshRenderer == null || gridIcon == null) {
            IsCurrentPositionValid = false;
            return;
        }

        var target = new Vector2(Mathf.FloorToInt(worldPosition.x), Mathf.FloorToInt(worldPosition.z));
        RenderGridSelector(target);
    }

    public void Hide() {
        IsCurrentPositionValid = false;
        if (meshRenderer != null) {
            meshRenderer.enabled = false;
        }
    }

    private void LoadGridTexture() {
        material = Resources.Load<Material>("Materials/GridSelectorMaterial");
        material.SetFloat("_Glossiness", 0f);
        material.mainTexture = gridIcon;
        material.color = Color.red;
        material.doubleSidedGI = false;
        material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
        material.enableInstancing = false;

        meshFilter = gameObject.AddComponent<MeshFilter>();
        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.material = material;
        meshRenderer.enabled = false;
    }

    private void RenderGridSelector(Vector2 targetPosition) {
        var cell = PathFinder.GetCell(targetPosition.x, targetPosition.y);
        var target = PathFinder.GetClosestTileTopToPoint(targetPosition, transform.position);
        IsCurrentPositionValid = PathFinder.IsWalkable(target.x, target.y);

        if (!IsCurrentPositionValid) {
            meshRenderer.enabled = false;
            return;
        }

        meshRenderer.enabled = true;
        material.mainTexture = gridIcon;
        material.color = new Color(50 / 255f, 240 / 255f, 160 / 255f, 0.6f);

        if (vertices == null) {
            mesh = new Mesh();
            vertices = new Vector3[4];
            uvs = new Vector2[4];
            triangles = new[] { 0, 1, 2, 1, 3, 2 };
        } else {
            mesh.Clear();
        }

        var offset = new Vector3(0f, 0.015f, 0f);

        vertices[0] = new Vector3(target.x, cell.Heights[0] / 5f, target.y + 1) + offset;
        vertices[1] = new Vector3(target.x + 1, cell.Heights[1] / 5f, target.y + 1) + offset;
        vertices[2] = new Vector3(target.x, cell.Heights[2] / 5f, target.y) + offset;
        vertices[3] = new Vector3(target.x + 1, cell.Heights[3] / 5f, target.y) + offset;

        uvs[0] = new Vector2(0, 1);
        uvs[1] = new Vector2(1, 1);
        uvs[2] = new Vector2(0, 0);
        uvs[3] = new Vector2(1, 0);

        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;

        meshFilter.sharedMesh = mesh;
        meshRenderer.enabled = true;
    }
}
