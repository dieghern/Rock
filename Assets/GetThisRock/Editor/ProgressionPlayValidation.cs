using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
namespace GetThisRock.Editor
{
    [InitializeOnLoad]
    public static class ProgressionPlayValidation
    {
        const string Pending="Rock.InventoryPlayValidation";
        static int stage;static double next;static bool finishing;
        static Keyboard keyboard;static Mouse mouse;static ContractManager manager;static GameHUD hud;
        static ProgressionPlayValidation()=>EditorApplication.playModeStateChanged+=StateChanged;
        public static void Run()
        {
            Environment.SetEnvironmentVariable("ROCK_SAVE_PATH",Path.GetFullPath("Logs/play-profile-"+Guid.NewGuid().ToString("N")+".json"));
            EditorSceneManager.OpenScene(GardenRockSetup.ScenePath);
            SessionState.SetBool(Pending,true);EditorApplication.EnterPlaymode();
        }
        static void StateChanged(PlayModeStateChange state)
        {
            if(!SessionState.GetBool(Pending,false))return;
            if(state==PlayModeStateChange.EnteredPlayMode)
            {
                stage=0;finishing=false;next=EditorApplication.timeSinceStartup+1.2;
                InputSystem.settings=UnityEngine.Object.Instantiate(InputSystem.settings);
                InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
                InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
                keyboard=InputSystem.AddDevice<Keyboard>();mouse=InputSystem.AddDevice<Mouse>();EditorApplication.update+=Tick;
            }
            if(state==PlayModeStateChange.EnteredEditMode&&finishing)
            {SessionState.SetBool(Pending,false);Environment.SetEnvironmentVariable("ROCK_SAVE_PATH",null);EditorApplication.Exit(SessionState.GetInt(Pending+".exit",1));}
        }
        static void Tick()
        {
            if(EditorApplication.timeSinceStartup<next)return;next=EditorApplication.timeSinceStartup+.4;
            try
            {
                switch(stage++)
                {
                    case 0:
                        manager=UnityEngine.Object.FindAnyObjectByType<ContractManager>();hud=UnityEngine.Object.FindAnyObjectByType<GameHUD>();
                        Check(hud.TabletOpen&&hud.presentation.IsReady&&manager.catalog.Length==4&&hud.equipment!=null,"Tablet asset animation and nonempty catalog");
                        Check(manager.wallet.Money==0&&manager.interaction.inventory.Used==0,"Fresh profile only has hands");
                        CaptureCamera("Tablet-model.png");
                        Check(hud.AcceptFromTablet(0),"Same acceptance callback as tablet button");break;
                    case 1:
                        Check(manager.TotalCount==2&&!hud.TabletOpen,"Two small rocks spawned");
                        Check(manager.interaction.TryPickup(manager.Targets[0].GetComponent<PhysicalObject>()),"Pickup in Play Mode");break;
                    case 2:
                        Check(manager.interaction.inventory.Used==1&&manager.interaction.inventory.SelectedRock!=null,"Rock remains selected in inventory");
                        CaptureCamera("Holding-rock.png");
                        InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.G));break;
                    case 3:
                        Check(manager.interaction.inventory.Used==0&&!manager.Targets[0].GetComponent<PhysicalObject>().IsStored,"G releases rock and slot");
                        InputSystem.QueueStateEvent(keyboard,new KeyboardState());
                        InputSystem.QueueStateEvent(mouse,new MouseState());
                        GardenRockValidation.DeliverAll(manager);
                        hud.SetTablet(true);break;
                    case 4:
                        Check(manager.State==ContractState.ReadyToFinish&&manager.wallet.Money==0,"Delivery awaits confirmation");
                        InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.Enter));break;
                    case 5:
                        Check(manager.Completed&&manager.wallet.Money==20&&manager.progression.Experience==25,"Enter works with tablet OPEN");
                        InputSystem.QueueStateEvent(keyboard,new KeyboardState());
                        Check(hud.AcceptFromTablet(0),"Repeat low-level contract");
                        GardenRockValidation.DeliverAll(manager);break;
                    case 6:
                        InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.Enter));break;
                    case 7:
                        Check(manager.Completed&&manager.wallet.Money==40&&manager.progression.Level==2,"Enter works in world and level two unlocks");
                        InputSystem.QueueStateEvent(keyboard,new KeyboardState());
                        Check(hud.AcceptFromTablet(1)&&manager.TotalCount==4,"Mixed contract selection");
                        GardenRockValidation.DeliverAll(manager);Check(hud.ConfirmDelivery(),"Finish mixed job");
                        Check(hud.equipment.TryBuy(hud.equipment.shop[0]),"Buy permanent lever");
                        Check(File.Exists(GameSave.SavePath),"Profile file written");break;
                    case 8:
                        int balance=manager.wallet.Money;int xp=manager.progression.Experience;
                        GameSave.Load(manager.wallet,manager.progression,manager.interaction.inventory);
                        Check(manager.wallet.Money==balance&&manager.progression.Experience==xp&&hud.equipment.Owns(ToolKind.Lever)&&manager.interaction.inventory.Used==1,"Save/load retains purchase, money and XP");
                        hud.equipment=null;SessionInstaller.Configure(manager,false);
                        Check(hud.equipment!=null&&hud.contracts.catalog.Length==4,"Missing UI reference repaired by installer");
                        Finish(0,"PASS: real Play Mode: tablet 3D animation, catalog acceptance, hands-only start, multi-rock spawn, pickup animation/slot, G physical drop, Enter while tablet open and closed, XP unlock, mixed contract, permanent purchase and save/load, repair missing equipment reference.");break;
                }
            }
            catch(Exception e){Finish(1,e.ToString());}
        }
        static void CaptureCamera(string name)
        {
            var camera=manager.interaction.view;var old=camera.targetTexture;var previous=RenderTexture.active;
            var rt=new RenderTexture(1280,800,24);var image=new Texture2D(1280,800,TextureFormat.RGB24,false);
            try{camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;image.ReadPixels(new Rect(0,0,1280,800),0,0);image.Apply();File.WriteAllBytes("Logs/"+name,image.EncodeToPNG());}
            finally{camera.targetTexture=old;RenderTexture.active=previous;rt.Release();UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(image);}
        }
        static void Finish(int code,string message)
        {
            EditorApplication.update-=Tick;if(keyboard!=null)InputSystem.RemoveDevice(keyboard);if(mouse!=null)InputSystem.RemoveDevice(mouse);
            Directory.CreateDirectory("Logs");File.WriteAllText("Logs/ProgressionPlayValidation.txt",message);
            SessionState.SetInt(Pending+".exit",code);finishing=true;EditorApplication.ExitPlaymode();
        }
        static void Check(bool condition,string message){if(!condition)throw new Exception("FAILED: "+message);}
    }
}
