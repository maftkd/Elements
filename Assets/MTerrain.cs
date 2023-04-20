using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MTerrain : MonoBehaviour
{
    public Material terrainMat;

    [Range(1, 2000)]
    public float size;
    [Range(2, 128)]
    public int resolution;
    float [] heightmap;
    [SerializeField]
    [Range(0, 1)]
    private float noiseMult;
    [SerializeField]
    [Range(0, 40)]
    private float noiseAmp;

    MeshFilter meshFilter;
    MeshRenderer meshRenderer;


    [Range(-20f, 10f)]
    public float minHeight;
    [Range(-10f, 10f)]
    public float maxHeight;

    [Range(0f, 1f)]
    public float slopeStart;
    [Range(0f, 1f)]
    public float slopeEnd;


    void OnValidate() {
        if(Application.isPlaying) {
            Regenerate();
            Debug.Log("Regenerating");
        }
    }

    void GenerateHeightmap(){
        heightmap = new float[resolution * resolution];
        Vector4 bounds = GetBounds();
        for(int y = 0; y < resolution; y++){
            float y01 = y / (float) (resolution - 1);
            for(int x = 0; x < resolution; x++){
                float x01 = x / (float) (resolution - 1);
                int index = y * resolution + x;
                float perlin01 = Mathf.PerlinNoise(x * noiseMult, y * noiseMult);
                float perlin = Mathf.Lerp(-1f, 1f, perlin01) *  noiseAmp;
                float xPos = Mathf.Lerp(bounds.x, bounds.z, x01);
                float zPos = Mathf.Lerp(bounds.y, bounds.w, y01);
                float dist = (new Vector2(xPos, zPos) - Vector2.down * size * 0.5f).magnitude;
                float distNorm = dist/ (size * 0.5f);
                float height01 = MSmoothStep(slopeStart, slopeEnd, distNorm);
                perlin = Mathf.Lerp(0, perlin, height01);
                float height = Mathf.Lerp(maxHeight, minHeight,  height01);
                heightmap[index] = height + perlin;
            }
        }
    }

    void GenerateMesh() {

        Mesh mesh = new Mesh();
        int verts = resolution * resolution;
        int tris = (resolution - 1) * (resolution - 1) * 6;

        Vector3 [] vertices = new Vector3[verts];
        Vector2 [] uvs = new Vector2[verts];
        Vector3 [] normals = new Vector3[verts];
        int     [] triangles = new int[tris];

        Vector4 bounds = GetBounds();
        
        for(int y = 0; y < resolution; y++) {
            float y01 = y / (float) (resolution - 1);
            for(int x = 0; x < resolution; x++) {
                int index = y * resolution + x;
                float x01 = x / (float) (resolution - 1);
                float xPos = Mathf.Lerp(bounds.x, bounds.z, x01);
                float zPos = Mathf.Lerp(bounds.y, bounds.w, y01);
                vertices[index] = new Vector3(xPos, heightmap[index], zPos);
                uvs[index] = new Vector2(x01, y01);
            }
        }

        for(int y = 0; y < resolution; y++) {
            for(int x = 0; x < resolution; x++) {
                int i = y * resolution + x;
                Vector3 left = x == 0 ? vertices[i] : vertices[i - 1];
                Vector3 right = x == resolution - 1 ? vertices[i] : vertices[i + 1];
                Vector3 bottom = y == 0 ? vertices[i] : vertices[i - resolution];
                Vector3 top = y == resolution - 1 ? vertices[i] : vertices[i + resolution];

                Vector3 leftRight = right - left;
                Vector3 bottomTop = top - bottom;
                
                normals[i] = Vector3.Cross(leftRight, bottomTop);
            }
        }

        int triCount = 0;
        for(int y = 0; y < resolution - 1; y++) {
            for(int x = 0; x < resolution - 1; x++) {
                int i = y * resolution + x;
                triangles[triCount    ] = i;
                triangles[triCount + 1] = i + resolution;
                triangles[triCount + 2] = i + 1;

                triangles[triCount + 3] = i + 1 + resolution;
                triangles[triCount + 4] = i + 1;
                triangles[triCount + 5] = i + resolution;

                triCount += 6;
            }
        }

        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.triangles = triangles;
        mesh.uv = uvs;

        Debug.Log("v: " + mesh.vertices.Length);

        if(meshFilter == null) {
            meshFilter = gameObject.AddComponent<MeshFilter>();
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = terrainMat;
        }

        meshFilter.sharedMesh = mesh;

        transform.position = Vector3.forward * size * 0.5f;
    }

    private Vector4 GetBounds() {
        Vector4 bounds = new Vector4();
        bounds.x = -size * 0.5f;
        bounds.y = -size * 0.5f;
        bounds.z = size * 0.5f;
        bounds.w = size * 0.5f;
        return bounds;
    }

    public void Regenerate() {
        GenerateHeightmap();
        GenerateMesh();
    }

    // Start is called before the first frame update
    void Start()
    {
        Camera.main.depthTextureMode = DepthTextureMode.Depth;
        //initial generation
        Regenerate();
    }

    public static float MSmoothStep(float a, float b, float x)
    {
        float t = MSaturate((x - a)/(b - a));
        return (float) (t*t*(3.0 - (2.0*t)));
    }

    public static float MSaturate(float a) {
        return Mathf.Clamp(a, 0f, 1f);
    }

}
