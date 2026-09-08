using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace GetThisRock.Editor
{
    public static class RealismValidation
    {
        static void Capture(Camera camera,Vector3 position,Vector3 target,string filename)
        {
            if(SystemInfo.graphicsDeviceType==UnityEngine.Rendering.GraphicsDeviceType.Null)return;
            camera.transform.position=position;camera.transform.LookAt(target);
            var rt=new RenderTexture(1280,720,24);var old=RenderTexture.active;camera.targetTexture=rt;
            camera.Render();RenderTexture.active=rt;var image=new Texture2D(1280,720,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1280,720),0,0);image.Apply();
            File.WriteAllBytes("Logs/"+filename,image.EncodeToPNG());camera.targetTexture=null;RenderTexture.active=old;UnityEngine.Object.DestroyImmediate(image);UnityEngine.Object.DestroyImmediate(rt);
        }
        static void Check(bool value,string message) { if(!value)throw new Exception("REALISM FAILED: "+message); Debug.Log("PASS: "+message); }
        [MenuItem("Get This Rock/Validate Realism")]
        public static void Run()
        {
            var previous=Physics.simulationMode;
            try
            {
                EditorSceneManager.OpenScene(GardenRockSetup.ScenePath);
                var manager=UnityEngine.Object.FindAnyObjectByType<ContractManager>();SessionInstaller.Configure(manager,false);
                manager.progression.Restore(500); manager.interaction.inventory.ResetInventory();
                Check(manager.TryAccept(manager.catalog[3]),"hills contract starts");
                var terrain=manager.scenarios;
                float height=terrain.HeightAt(2,0);
                Check(height>.1f&&height<2,"terrain relief is bounded and nonzero");
                Check(Physics.Raycast(new Vector3(2,5,-4),Vector3.down,out var hit,8)&&Mathf.Abs(hit.point.y-terrain.HeightAt(2,-4))<.03f,"terrain collider matches rendered height");
                Check(Mathf.Abs(terrain.HeightAt(0,6))<.001f,"delivery zone remains level");
                foreach(var rock in manager.Targets)
                {
                    var hull=rock.GetComponent<MeshCollider>();
                    Check(hull!=null&&hull.convex&&hull.sharedMesh.vertexCount>50,"rock has irregular convex collision");
                    Check(rock.GetComponent<SphereCollider>()==null,"old spherical collider removed");
                }
                // Flat unloading area isolates cargo transport from the terrain test above.
                var player=manager.interaction;player.GetComponent<CharacterController>().enabled=false;
                player.transform.SetPositionAndRotation(new Vector3(8,.05f,7),Quaternion.identity);
                player.inventory.AddTool(ToolKind.Wheelbarrow);player.equipment.TryEquip(ToolKind.Wheelbarrow);player.input.SetUIBlocked(false);
                var cart=player.gameObject.AddComponent<WheelbarrowController>();cart.interaction=player;cart.Initialize(Resources.Load<GameContent>("RockContent").toolMaterial);cart.Toggle();
                Check(cart.Active&&cart.Deployed,"wheelbarrow deploys within reach");cart.Park();
                var body=GameObject.Find("Carretilla").GetComponent<Rigidbody>();
                Check(!body.isKinematic,"cart uses a dynamic rigidbody");
                var cargo=GameObject.CreatePrimitive(PrimitiveType.Sphere);cargo.name="Validation cargo";cargo.transform.localScale=Vector3.one*.3f;
                cargo.transform.position=body.position+Vector3.up*.8f;var rb=cargo.AddComponent<Rigidbody>();rb.mass=5;rb.collisionDetectionMode=CollisionDetectionMode.ContinuousDynamic;
                var cargoVisual=cargo.AddComponent<RockVisual>();cargoVisual.material=manager.rockMaterial;cargoVisual.Refresh(42);
                Physics.simulationMode=SimulationMode.Script;Physics.SyncTransforms();
                for(int i=0;i<150;i++)Physics.Simulate(.02f);
                var local=body.transform.InverseTransformPoint(rb.position);
                Check(local.y>.5f&&local.y<.9f&&Mathf.Abs(local.x)<.5f,"parked tray supports loose cargo");
                cart.Hide();Check(cart.Deployed&&!cart.Active,"changing tools keeps cart and cargo in world");
                cart.Toggle();Check(cart.Active,"parked cart can be taken again");
                var start=body.position;
                for(int i=0;i<180;i++)
                {
                    player.transform.position+=Vector3.forward*.012f;
                    typeof(WheelbarrowController).GetMethod("FixedUpdate",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).Invoke(cart,null);Physics.Simulate(.02f);
                }
                local=body.transform.InverseTransformPoint(rb.position);
                Check(Vector3.Distance(start,body.position)>1,"cart transports under bounded driving force");
                Check(Mathf.Abs(local.x)<.5f&&Mathf.Abs(local.z)<.65f&&local.y>.48f,"cargo remains inside during transport");
                Capture(manager.interaction.view,body.position+new Vector3(2,2,-2),body.position+Vector3.up*.5f,"Cart-realism.png");
                Capture(manager.interaction.view,new Vector3(6,4,-5),new Vector3(1,.5f,0),"Terrain-realism.png");
                cart.Park();player.transform.position+=Vector3.left*8;cart.Toggle();Check(!cart.Active,"cannot retrieve a distant cart by teleporting");
                Directory.CreateDirectory("Logs");File.WriteAllText("Logs/RealismValidation.txt","PASS: terrain height/collision, level delivery zone, convex rock geometry, dynamic cart, parked cargo, tool change persistence, loaded transport and proximity requirement.\n");
                Debug.Log("REALISM_VALIDATION_PASS");
            }
            finally { Physics.simulationMode=previous;EditorSceneManager.OpenScene(GardenRockSetup.ScenePath); }
        }
    }
}

