using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[ExecuteInEditMode]
public class GeometryGrassPainter : MonoBehaviour
{

    private Mesh mesh;
    MeshFilter filter;

    public Color AdjustedColor;

    [Range(1, 600000)]
    public int grassLimit = 50000;

    private Vector3 lastPosition = Vector3.zero;

    public int toolbarInt = 0;

    [SerializeField]
    List<Vector3> positions = new List<Vector3>();
    [SerializeField]
    List<Color> colors = new List<Color>();
    [SerializeField]
    List<int> indicies = new List<int>();
    [SerializeField]
    List<Vector3> normals = new List<Vector3>();
    [SerializeField]
    List<Vector2> length = new List<Vector2>();

    public bool painting;
    public bool removing;
    public bool editing;

    public int i = 0;

    public float sizeWidth = 1f;
    public float sizeLength = 1f;
    public float density = 1f;


    public float normalLimit = 1;

    public float rangeR, rangeG, rangeB;
    public LayerMask hitMask = 1;
    public LayerMask paintMask = 1;
    public float brushSize;

    Vector3 mousePos;

    [HideInInspector]
    public Vector3 hitPosGizmo;

    Vector3 hitPos;

    [HideInInspector]
    public Vector3 hitNormal;

    int[] indi;
#if UNITY_EDITOR
    void OnFocus()
    {
        // Remove delegate listener if it has previously
        // been assigned.
        SceneView.duringSceneGui -= this.OnScene;
        // Add (or re-add) the delegate.
        SceneView.duringSceneGui += this.OnScene;
    }

    void OnDestroy()
    {
        // When the window is destroyed, remove the delegate
        // so that it will no longer do any drawing.
        SceneView.duringSceneGui -= this.OnScene;
    }

    private void OnEnable()
    {
        filter = GetComponent<MeshFilter>();
        SceneView.duringSceneGui += this.OnScene;
    }

    public void ClearMesh()
    {
        i = 0;
        positions = new List<Vector3>();
        indicies = new List<int>();
        colors = new List<Color>();
        normals = new List<Vector3>();
        length = new List<Vector2>();
    }

    void OnScene(SceneView scene)
    {
        // only allow painting while this object is selected
        if ((Selection.Contains(gameObject)))
        {

            Event e = Event.current;
            RaycastHit terrainHit;
            mousePos = e.mousePosition;
            float ppp = EditorGUIUtility.pixelsPerPoint;
            mousePos.y = scene.camera.pixelHeight - mousePos.y * ppp;
            mousePos.x *= ppp;

            // ray for gizmo(disc)
            Ray rayGizmo = scene.camera.ScreenPointToRay(mousePos);
            RaycastHit hitGizmo;

            if (Physics.Raycast(rayGizmo, out hitGizmo, 200f, hitMask.value))
            {
                hitPosGizmo = hitGizmo.point;
            }

            if (e.type == EventType.MouseDrag && e.button == 1 && toolbarInt == 0)
            {
                // place based on density
                for (int k = 0; k < density; k++)
                {

                    // brushrange
                    float t = 2f * Mathf.PI * Random.Range(0f, brushSize);
                    float u = Random.Range(0f, brushSize) + Random.Range(0f, brushSize);
                    float r = (u > 1 ? 2 - u : u);
                    Vector3 origin = Vector3.zero;

                    // place random in radius, except for first one
                    if (k != 0)
                    {
                        origin.x += r * Mathf.Cos(t);
                        origin.y += r * Mathf.Sin(t);
                    }
                    else
                    {
                        origin = Vector3.zero;
                    }

                    // add random range to ray
                    Ray ray = scene.camera.ScreenPointToRay(mousePos);
                    ray.origin += origin;

                    // if the ray hits something thats on the layer mask,  within the grass limit and within the y normal limit
                    if (Physics.Raycast(ray, out terrainHit, 200f, hitMask.value) && i < grassLimit && terrainHit.normal.y <= (1 + normalLimit) && terrainHit.normal.y >= (1 - normalLimit))
                    {
                        if ((paintMask.value & (1 << terrainHit.transform.gameObject.layer)) > 0)
                        {
                            hitPos = terrainHit.point;
                            hitNormal = terrainHit.normal;
                            if (k != 0)
                            {
                                var grassPosition = hitPos;// + Vector3.Cross(origin, hitNormal);
                                grassPosition -= this.transform.position;

                                positions.Add((grassPosition));
                                indicies.Add(i);
                                length.Add(new Vector2(sizeWidth, sizeLength));
                                // add random color variations                          
                                colors.Add(new Color(AdjustedColor.r + (Random.Range(0, 1.0f) * rangeR), AdjustedColor.g + (Random.Range(0, 1.0f) * rangeG), AdjustedColor.b + (Random.Range(0, 1.0f) * rangeB), 1));

                                //colors.Add(temp);
                                normals.Add(terrainHit.normal);
                                i++;
                            }
                            else
                            {// to not place everything at once, check if the first placed point far enough away from the last placed first one
                                if (Vector3.Distance(terrainHit.point, lastPosition) > brushSize)
                                {
                                    var grassPosition = hitPos;
                                    grassPosition -= this.transform.position;
                                    positions.Add((grassPosition));
                                    indicies.Add(i);
                                    length.Add(new Vector2(sizeWidth, sizeLength));
                                    colors.Add(new Color(AdjustedColor.r + (Random.Range(0, 1.0f) * rangeR), AdjustedColor.g + (Random.Range(0, 1.0f) * rangeG), AdjustedColor.b + (Random.Range(0, 1.0f) * rangeB), 1));
                                    normals.Add(terrainHit.normal);
                                    i++;

                                    if (origin == Vector3.zero)
                                    {
                                        lastPosition = hitPos;
                                    }
                                }
                            }
                        }

                    }

                }
                e.Use();
            }
            // removing mesh points
            if (e.type == EventType.MouseDrag && e.button == 1 && toolbarInt == 1)
            {
                Ray ray = scene.camera.ScreenPointToRay(mousePos);

                if (Physics.Raycast(ray, out terrainHit, 200f, hitMask.value))
                {
                    hitPos = terrainHit.point;
                    hitPosGizmo = hitPos;
                    hitNormal = terrainHit.normal;
                    for (int j = 0; j < positions.Count; j++)
                    {
                        Vector3 pos = positions[j];

                        pos += this.transform.position;
                        float dist = Vector3.Distance(terrainHit.point, pos);

                        // if its within the radius of the brush, remove all info
                        if (dist <= brushSize)
                        {
                            positions.RemoveAt(j);
                            colors.RemoveAt(j);
                            normals.RemoveAt(j);
                            length.RemoveAt(j);
                            indicies.RemoveAt(j);
                            i--;
                            for (int i = 0; i < indicies.Count; i++)
                            {
                                indicies[i] = i;
                            }
                        }
                    }
                }
                e.Use();
            }

            if (e.type == EventType.MouseDrag && e.button == 1 && toolbarInt == 2)
            {
                Ray ray = scene.camera.ScreenPointToRay(mousePos);

                if (Physics.Raycast(ray, out terrainHit, 200f, hitMask.value))
                {
                    hitPos = terrainHit.point;
                    hitPosGizmo = hitPos;
                    hitNormal = terrainHit.normal;
                    for (int j = 0; j < positions.Count; j++)
                    {
                        Vector3 pos = positions[j];

                        pos += this.transform.position;
                        float dist = Vector3.Distance(terrainHit.point, pos);

                        // if its within the radius of the brush, remove all info
                        if (dist <= brushSize)
                        {

                            colors[j] = (new Color(AdjustedColor.r + (Random.Range(0, 1.0f) * rangeR), AdjustedColor.g + (Random.Range(0, 1.0f) * rangeG), AdjustedColor.b + (Random.Range(0, 1.0f) * rangeB), 1));

                            length[j] = new Vector2(sizeWidth, sizeLength);

                        }
                    }
                }
                e.Use();
            }
            // set all info to mesh
            mesh = new Mesh();
            mesh.SetVertices(positions);
            indi = indicies.ToArray();
            mesh.SetIndices(indi, MeshTopology.Points, 0);
            mesh.SetUVs(0, length);
            mesh.SetColors(colors);
            mesh.SetNormals(normals);
            filter.mesh = mesh;

        }
    }
#endif
}