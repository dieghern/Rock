using UnityEngine;
namespace GetThisRock
{
    public sealed class RockVisual : MonoBehaviour
    {
        public Material material;
        Mesh ownedVisual, ownedCollision;
        public void Refresh(int seed)
        {
            var old = GetComponent<Renderer>(); if (old != null) old.enabled = false;
            var child = transform.Find("Irregular rock visual");
            if (child == null) { child = new GameObject("Irregular rock visual").transform; child.SetParent(transform, false); child.gameObject.AddComponent<MeshFilter>(); child.gameObject.AddComponent<MeshRenderer>(); }
            child.gameObject.layer = gameObject.layer;
            ReleaseMeshes();
            ownedVisual = Create(seed, 48, 28);
            child.GetComponent<MeshFilter>().sharedMesh = ownedVisual;
            child.GetComponent<MeshRenderer>().sharedMaterial = material;
            var sphere = GetComponent<SphereCollider>();
            var hull = GetComponent<MeshCollider>();
            if (hull == null) hull = gameObject.AddComponent<MeshCollider>();
            if (sphere != null) { hull.sharedMaterial = sphere.sharedMaterial; sphere.enabled = false; if(Application.isPlaying)Destroy(sphere);else DestroyImmediate(sphere); }
            ownedCollision = Create(seed, 12, 8);
            hull.sharedMesh = ownedCollision; hull.convex = true;
        }
        static float Noise(Vector3 p, float seed) => (Mathf.PerlinNoise(p.x + seed, p.y + 17) + Mathf.PerlinNoise(p.y + seed, p.z + 43) + Mathf.PerlinNoise(p.z + seed, p.x + 71)) / 3;
        static Mesh Create(int seed, int lon, int lat)
        {
            var vertices = new Vector3[(lat + 1) * (lon + 1)]; var uv = new Vector2[vertices.Length];
            var triangles = new System.Collections.Generic.List<int>();
            float offset = (seed & 65535) * .137f;
            for (int y = 0; y <= lat; y++) for (int x = 0; x <= lon; x++)
            {
                float v = y / (float)lat, u = x / (float)lon;
                float phi = v * Mathf.PI, theta = (x == lon ? 0 : u) * Mathf.PI * 2;
                var n = new Vector3(Mathf.Sin(phi) * Mathf.Cos(theta), Mathf.Cos(phi), Mathf.Sin(phi) * Mathf.Sin(theta));
                float radius = .48f + (Noise(n * 2.1f, offset) - .5f) * .27f + (Noise(n * 7, offset) - .5f) * .035f;
                var p = n * radius;
                // Broad weathered faces, an asymmetric silhouette, and a flattened resting face.
                p.x *= 1.04f; p.z *= .91f; p.x += n.y * n.y * .025f;
                p.y = Mathf.Max(-.36f, p.y * .88f);
                vertices[y * (lon + 1) + x] = p; uv[y * (lon + 1) + x] = new Vector2(u, v);
            }
            for (int y = 0; y < lat; y++) for (int x = 0; x < lon; x++)
            {
                int a = y * (lon + 1) + x, b = a + 1, c = a + lon + 1, d = c + 1;
                if (y > 0) { triangles.Add(a); triangles.Add(b); triangles.Add(c); }
                if (y < lat - 1) { triangles.Add(b); triangles.Add(d); triangles.Add(c); }
            }
            var mesh = new Mesh { name = "Weathered granite " + seed };
            mesh.vertices = vertices; mesh.uv = uv; mesh.SetTriangles(triangles, 0); mesh.RecalculateNormals();
            var normals = mesh.normals;
            for (int y = 0; y <= lat; y++) { int a = y * (lon + 1), b = a + lon; var n = (normals[a] + normals[b]).normalized; normals[a] = normals[b] = n; }
            mesh.normals = normals; mesh.RecalculateTangents(); mesh.RecalculateBounds(); return mesh;
        }
        void ReleaseMeshes() { if (ownedVisual != null) { if(Application.isPlaying)Destroy(ownedVisual);else DestroyImmediate(ownedVisual); } if (ownedCollision != null) { if(Application.isPlaying)Destroy(ownedCollision);else DestroyImmediate(ownedCollision); } }
        void OnDestroy() => ReleaseMeshes();
    }
}

