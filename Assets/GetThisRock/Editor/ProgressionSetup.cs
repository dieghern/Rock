using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace GetThisRock.Editor
{
    public static class ProgressionSetup
    {
        const string Root="Assets/GetThisRock/Data/";
        [MenuItem("Get This Rock/Upgrade Contracts and Shop")]
        public static void Upgrade(){if(!Application.isBatchMode&&!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;EditorSceneManager.OpenScene(GardenRockSetup.ScenePath);Configure();EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());AssetDatabase.SaveAssets();}
        public static void Configure()
        {
            Directory.CreateDirectory(Root);Directory.CreateDirectory("Assets/GetThisRock/Resources");
            ConfigureTexture("Assets/GetThisRock/Art/Textures/GraniteAlbedo.png");ConfigureTexture("Assets/GetThisRock/Art/Textures/GroundAlbedo.png");
            var content=AssetDatabase.LoadAssetAtPath<GameContent>("Assets/GetThisRock/Resources/RockContent.asset");if(content==null){content=ScriptableObject.CreateInstance<GameContent>();AssetDatabase.CreateAsset(content,"Assets/GetThisRock/Resources/RockContent.asset");}
            content.contracts=new[]{
                Job("GardenRock","001","01 · Limpieza de jardín","Dos rocas pequeñas alrededor de una vivienda.",1,25,25,ScenarioPreset.Garden,1439,new[]{Spec(3,.28f,-.2f,0),Spec(5,.34f,1.4f,-.3f)}),
                Job("PathRock","002","02 · Huerto bloqueado","Tres pequeñas y una mediana en un huerto diferente.",2,70,45,ScenarioPreset.Orchard,2861,new[]{Spec(4,.3f,-1,0),Spec(6,.35f,.2f,-1),Spec(3,.28f,2,-.5f),Spec(55,.9f,1,1)}),
                Job("HeavyRock","003","03 · Zona de obras","Dos pequeñas y dos medianas entre suministros de obra.",3,130,70,ScenarioPreset.Construction,3911,new[]{Spec(6,.35f,-1,-.5f),Spec(8,.4f,2,-1),Spec(85,1.1f,.3f,1),Spec(100,1.2f,2.5f,1)}),
                Job("BigCleanup","004","04 · Camino de las colinas","Una pequeña, una mediana y una roca de 280 kg en terreno rural.",4,220,100,ScenarioPreset.Hills,5441,new[]{Spec(8,.4f,2,-1),Spec(90,1.15f,-.5f,0),Spec(280,1.85f,1.5f,1)})};
            content.tools=new[]{Tool("Lever",ToolKind.Lever,"Palanca","Levanta un borde apoyado en el suelo.",60),Tool("Rope",ToolKind.Rope,"Cuerda de arrastre","Engancha con E y transmite tensión horizontal.",100),Tool("Hammer",ToolKind.Hammer,"Martillo de demolición","Fractura progresivamente rocas medianas y grandes en fragmentos transportables.",80),Tool("Wheelbarrow",ToolKind.Wheelbarrow,"Carretilla","Despliega con E un cajón físico para transportar varias piedras.",140)};
            PresentationAssets.Generate(content);
            content.rockMaterial=TexturedMaterial("Rock surface","Assets/GetThisRock/Art/Textures/GraniteAlbedo.png",new Color(.82f,.82f,.82f),.18f);
            content.grassMaterial=TexturedMaterial("Ground surface","Assets/GetThisRock/Art/Textures/GroundAlbedo.png",Color.white,.12f);
            content.dirtMaterial=Mat("Dirt",new Color(.28f,.2f,.12f));content.woodMaterial=Mat("Wood",new Color(.27f,.15f,.07f));content.leavesMaterial=Mat("Leaves",new Color(.14f,.3f,.1f));
            EditorUtility.SetDirty(content);AssetDatabase.SaveAssets();
            var m=Object.FindAnyObjectByType<ContractManager>();SessionInstaller.Configure(m,false);m.activeContract=null;
            var rock=m.targetRock;rock.gameObject.SetActive(true);rock.mass=3;rock.ApplyMass();rock.transform.localScale=Vector3.one*.28f;rock.transform.position=new Vector3(-.2f,.2f,0);rock.GetComponent<PhysicalObject>().allowPickup=true;
            var breaker=rock.GetComponent<RockBreaker>()??rock.gameObject.AddComponent<RockBreaker>();breaker.rock=rock;
            var visual=rock.GetComponent<RockVisual>()??rock.gameObject.AddComponent<RockVisual>();visual.material=content.rockMaterial;visual.Refresh(1439);
            var ground=GameObject.Find("Ground");if(ground!=null){ground.GetComponent<Renderer>().sharedMaterial=content.grassMaterial;ground.transform.localScale=new Vector3(30,.35f,30);}
            var light=Object.FindAnyObjectByType<Light>();if(light!=null){light.intensity=1.55f;light.color=new Color(1,.92f,.78f);}
            RenderSettings.ambientLight=new Color(.42f,.48f,.52f);RenderSettings.fog=true;RenderSettings.fogColor=new Color(.6f,.7f,.75f);RenderSettings.fogDensity=.006f;
            var sign=GameObject.Find("Delivery sign");if(sign!=null){var text=sign.GetComponent<TextMesh>();text.text="ENTREGA\nSUELTA + ENTER";text.fontSize=96;text.characterSize=.032f;}
            foreach(var item in Object.FindObjectsByType<PhysicalObject>(FindObjectsSortMode.None))if(item.name=="Small crate - 4 kg")Object.DestroyImmediate(item.gameObject);
            EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        }
        static void ConfigureTexture(string path){AssetDatabase.ImportAsset(path);var importer=AssetImporter.GetAtPath(path) as TextureImporter;if(importer==null)return;importer.wrapMode=TextureWrapMode.Repeat;importer.filterMode=FilterMode.Trilinear;importer.anisoLevel=8;importer.textureCompression=TextureImporterCompression.CompressedHQ;importer.SaveAndReimport();}
        static Material TexturedMaterial(string name,string texture,Color tint,float smooth){var mat=Mat(name,tint);mat.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(texture);mat.mainTextureScale=new Vector2(5,5);mat.SetFloat("_Smoothness",smooth);EditorUtility.SetDirty(mat);return mat;}
        static Material Mat(string name,Color color){string path="Assets/GetThisRock/Art/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(m==null){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,path);}m.SetColor("_BaseColor",color);EditorUtility.SetDirty(m);return m;}
        static RockSpawn Spec(float mass,float diameter,float x,float z)=>new RockSpawn{mass=mass,diameter=diameter,position=new Vector3(x,diameter*.5f+.08f,z)};
        static Contract Job(string file,string id,string title,string description,int level,int reward,int xp,ScenarioPreset preset,int seed,RockSpawn[] rocks){string path=Root+file+".asset";var c=AssetDatabase.LoadAssetAtPath<Contract>(path);if(c==null){c=ScriptableObject.CreateInstance<Contract>();AssetDatabase.CreateAsset(c,path);}c.contractId=id;c.displayName=title;c.description=description;c.requiredLevel=level;c.reward=reward;c.experienceReward=xp;c.scenario=preset;c.scenarioSeed=seed;c.rocks=rocks;c.objective="Entrega todas las rocas y pulsa Enter.";EditorUtility.SetDirty(c);return c;}
        static ToolDefinition Tool(string file,ToolKind kind,string title,string description,int price){string path=Root+file+".asset";var t=AssetDatabase.LoadAssetAtPath<ToolDefinition>(path);if(t==null){t=ScriptableObject.CreateInstance<ToolDefinition>();AssetDatabase.CreateAsset(t,path);}t.kind=kind;t.displayName=title;t.description=description;t.price=price;EditorUtility.SetDirty(t);return t;}
    }
    [InitializeOnLoad]static class GardenRockPlayStart{static GardenRockPlayStart()=>EditorApplication.delayCall+=()=>{var scene=AssetDatabase.LoadAssetAtPath<SceneAsset>(GardenRockSetup.ScenePath);if(scene!=null)EditorSceneManager.playModeStartScene=scene;};}
}
