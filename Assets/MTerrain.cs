using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MTerrain : MonoBehaviour
{
    [Range(1, 1000)]
    public float size;
    [Range(2, 128)]
    public int resolution;
    float [] heightmap;
    [SerializeField]
    [Range(0, 10)]
    private float noiseMult;

    MeshFilter meshFilter;
    MeshRenderer meshRenderer;

    public Material terrainMat;

    [SerializeField]
    private Slider noiseMultSlider;
    [SerializeField]
    private Slider sizeSlider;
    [SerializeField]
    private Slider resolutionSlider;

    void GenerateHeightmap(){
        heightmap = new float[resolution * resolution];
        for(int y = 0; y < resolution; y++){
            for(int x = 0; x < resolution; x++){
                int index = y * resolution + x;
                float perlin = Mathf.PerlinNoise(x * noiseMult, y * noiseMult);
                heightmap[index] = perlin;
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
        //setup ui
        noiseMultSlider.value = noiseMult;
        sizeSlider.value = size;
        resolutionSlider.value = resolution;

        //initial generation
        Regenerate();
    }

    public void SetNoiseMult(Slider slider) {
        noiseMult = slider.value;
        Regenerate();
    }

    public void SetSize(Slider slider) {
        size = slider.value;
        Regenerate();
    }

    public void SetResolution(Slider slider) {
        resolution = (int) slider.value;
        Regenerate();
    }

}
