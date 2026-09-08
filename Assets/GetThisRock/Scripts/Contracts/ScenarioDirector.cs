using UnityEngine;
namespace GetThisRock
{
    public sealed class ScenarioDirector : MonoBehaviour
    {
        public Material grass,dirt,stone,wood,leaves;
        Transform generated; System.Random random; Mesh terrainMesh; int seed; float relief;
        public float HeightAt(float x, float z)
        {
            // Keep the road, house foundation and fence perimeter at their original elevation.
            float road = 1 - Mathf.SmoothStep(0,1,Mathf.InverseLerp(3.0f,4.5f,z));
            float edge = 1 - Mathf.SmoothStep(0,1,Mathf.InverseLerp(12,14.2f,Mathf.Max(Mathf.Abs(x),Mathf.Abs(z))));
            float house = Mathf.SmoothStep(0,1,Mathf.InverseLerp(-3.2f,-1.7f,x));
            float n = Mathf.PerlinNoise(x*.16f+seed*.017f,z*.16f+31);
            return relief * (.2f + n*.8f) * road * edge * house;
        }
        public void Build(Contract contract,int iteration)
        {
            var player = Object.FindAnyObjectByType<PlayerInteraction>();
            float oldHeight = player != null ? HeightAt(player.transform.position.x,player.transform.position.z) : 0;
            if(generated!=null) { generated.gameObject.SetActive(false); if(Application.isPlaying)Destroy(generated.gameObject);else DestroyImmediate(generated.gameObject); }
            if(terrainMesh!=null){if(Application.isPlaying)Destroy(terrainMesh);else DestroyImmediate(terrainMesh);}
            generated=new GameObject("Generated scenario").transform;
            random=new System.Random(contract.scenarioSeed+iteration*7919); seed=contract.scenarioSeed;
            relief=contract.scenario==ScenarioPreset.Hills?1.65f:contract.scenario==ScenarioPreset.Construction?.65f:1.0f;
            BuildGround();
            if(player!=null)
            {
                var motor=player.GetComponent<CharacterController>(); bool enabled=motor!=null&&motor.enabled;
                if(enabled)motor.enabled=false;
                var p=player.transform.position; p.y+=HeightAt(p.x,p.z)-oldHeight+.05f; player.transform.position=p;
                if(enabled)motor.enabled=true;
                player.GetComponent<WheelbarrowController>()?.PlaceOnTerrain(this);
            }
            RenderSettings.fog=true;RenderSettings.fogColor=new Color(.58f,.68f,.71f);RenderSettings.fogDensity=.006f;
            switch(contract.scenario)
            {
                case ScenarioPreset.Orchard:for(int i=0;i<9;i++)Tree(new Vector3(-10+(i%3)*4,0,-9+(i/3)*4));for(int i=0;i<5;i++)Crate(new Vector3(-4+i*1.1f,.25f,3));break;
                case ScenarioPreset.Construction:for(int i=0;i<8;i++)Crate(new Vector3(-8+(i%4)*1.2f,.3f,-6+(i/4)*1.1f));for(int i=0;i<5;i++)Prop(PrimitiveType.Cylinder,"Traffic barrel",new Vector3(-5+i*2,.45f,3),new Vector3(.32f,.45f,.32f),stone);break;
                case ScenarioPreset.Hills:for(int i=0;i<14;i++)Prop(PrimitiveType.Sphere,"Shrub",new Vector3(-11+(float)random.NextDouble()*22,.35f,-10+(float)random.NextDouble()*14),Vector3.one*(.45f+(float)random.NextDouble()*.6f),leaves);break;
                default:for(int i=0;i<8;i++)Prop(PrimitiveType.Sphere,"Garden shrub",new Vector3(-10+i*2.5f,.35f,-9),new Vector3(.8f,.55f,.8f),leaves);break;
            }
            Physics.SyncTransforms();
        }
        void BuildGround()
        {
            var original=GameObject.Find("Ground");
            if(original!=null) { var r=original.GetComponent<Renderer>();if(r!=null)r.enabled=false; var c=original.GetComponent<Collider>();if(c!=null)c.enabled=false; }
            const int cells=120; const float step=30f/cells;
            var vertices=new Vector3[(cells+1)*(cells+1)];var uv=new Vector2[vertices.Length];var indices=new int[cells*cells*6];int t=0;
            for(int z=0;z<=cells;z++)for(int x=0;x<=cells;x++) { float px=-15+x*step,pz=-15+z*step;int i=z*(cells+1)+x;vertices[i]=new Vector3(px,HeightAt(px,pz),pz);uv[i]=new Vector2(px*.25f,pz*.25f); }
            for(int z=0;z<cells;z++)for(int x=0;x<cells;x++){int a=z*(cells+1)+x,b=a+1,c=a+cells+1,d=c+1;indices[t++]=a;indices[t++]=c;indices[t++]=b;indices[t++]=b;indices[t++]=c;indices[t++]=d;}
            terrainMesh=new Mesh{name="Undulating terrain"};terrainMesh.vertices=vertices;terrainMesh.uv=uv;terrainMesh.triangles=indices;terrainMesh.RecalculateNormals();terrainMesh.RecalculateBounds();
            var ground=new GameObject("Terrain surface");ground.transform.SetParent(generated,false);ground.AddComponent<MeshFilter>().sharedMesh=terrainMesh;ground.AddComponent<MeshRenderer>().sharedMaterial=grass;ground.AddComponent<MeshCollider>().sharedMesh=terrainMesh;
        }
        public Vector3 Resolve(Vector3 source,int index)
        {
            float x=Mathf.Clamp(source.x+(float)(random.NextDouble()*1.6-.8),-2.4f,3),z=Mathf.Clamp(source.z+(float)(random.NextDouble()*1.3-.65),-2.2f,2);
            return new Vector3(x,source.y+HeightAt(x,z),z);
        }
        void Tree(Vector3 p){Prop(PrimitiveType.Cylinder,"Tree trunk",p+Vector3.up*1.25f,new Vector3(.28f,1.25f,.28f),wood);Prop(PrimitiveType.Sphere,"Tree crown",p+Vector3.up*2.8f,new Vector3(1.4f,1.5f,1.4f),leaves);}
        void Crate(Vector3 p)=>Prop(PrimitiveType.Cube,"Site supplies",p,new Vector3(.75f,.5f,.7f),wood);
        void Prop(PrimitiveType type,string label,Vector3 p,Vector3 scale,Material mat){var go=GameObject.CreatePrimitive(type);go.name=label;go.transform.SetParent(generated);p.y+=HeightAt(p.x,p.z);go.transform.position=p;go.transform.localScale=scale;go.GetComponent<Renderer>().sharedMaterial=mat;var c=go.GetComponent<Collider>();if(c!=null){c.enabled=false;if(Application.isPlaying)Destroy(c);else DestroyImmediate(c);}}
        void OnDestroy(){if(generated!=null){if(Application.isPlaying)Destroy(generated.gameObject);else DestroyImmediate(generated.gameObject);}if(terrainMesh!=null){if(Application.isPlaying)Destroy(terrainMesh);else DestroyImmediate(terrainMesh);}}
    }
}




