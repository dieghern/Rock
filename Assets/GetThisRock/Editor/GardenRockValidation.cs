using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace GetThisRock.Editor
{
    public static class GardenRockValidation
    {
        public static void GenerateAndValidate(){ProgressionSetup.Upgrade();Validate();}
        [MenuItem("Get This Rock/Validate Milestone")]
        public static void Validate()
        {
            EditorSceneManager.OpenScene(GardenRockSetup.ScenePath);
            var m=UnityEngine.Object.FindAnyObjectByType<ContractManager>();SessionInstaller.Configure(m,false);
            var inv=m.interaction.inventory;var eq=m.interaction.equipment;var stats=eq.stats;
            GameSave.PathOverride=Path.GetFullPath("Logs/progression-v2-profile.json");
            try
            {
                m.wallet.Restore(1000);m.progression.Restore(0);inv.ResetInventory();stats.Restore(0,0);
                Check(inv.Capacity==4&&inv.Used==0&&eq.Selected==ToolKind.Hands,"starts with hands and four empty spaces");
                Check(m.catalog.Length==4&&eq.shop.Length==4,"four seeded contracts and four tools");
                Check(m.TryAccept(m.catalog[0])&&m.TotalCount==2,"first contract spawns only small rocks");
                var firstPosition=m.Targets[0].transform.position;DeliverAll(m);Check(m.TryFinish(),"delivery plus explicit confirmation finishes contract");
                Check(m.wallet.Money==1025&&m.progression.Experience==25,"money and XP paid once");
                Check(m.TryAccept(m.catalog[0]),"contract can be repeated");Check(Vector3.Distance(firstPosition,m.Targets[0].transform.position)>.01f,"seed iteration changes rock placement");DeliverAll(m);Check(m.TryFinish()&&m.progression.Level==2,"XP unlocks level two");
                Check(eq.TryUpgradeInventory()&&inv.Capacity==5,"inventory capacity purchased");
                Check(eq.TryUpgradeSpeed()&&stats.speedLevel==1&&eq.TryUpgradeCarry()&&stats.carryLevel==1,"character upgrades purchased");
                Check(eq.TryBuy(eq.shop[2])&&eq.Owns(ToolKind.Hammer),"hammer purchase persists as equipment");
                Check(eq.TryBuy(eq.shop[3])&&eq.Owns(ToolKind.Wheelbarrow),"wheelbarrow purchase persists as equipment");
                Check(m.TryAccept(m.catalog[1]),"level two scenario accepts");
                var medium=m.Targets[m.Targets.Count-1];var breaker=medium.GetComponent<RockBreaker>();int before=m.TotalCount;
                for(int i=0;i<3;i++)breaker.Strike(40,medium.transform.position,Vector3.forward);
                Check(m.TotalCount==before+1,"hammer splits a medium rock into two physical fragments");
                var visual=m.Targets[0].GetComponent<RockVisual>();Check(visual!=null&&visual.transform.Find("Irregular rock visual")!=null,"procedural rock mesh installed");
                Check(GameSave.Save(m.wallet,m.progression,eq),"profile saves");int money=m.wallet.Money;inv.ResetInventory();stats.Restore(0,0);GameSave.Load(m.wallet,m.progression,inv,stats);
                Check(m.wallet.Money==money&&inv.Capacity==5&&stats.speedLevel==1&&stats.carryLevel==1&&eq.Owns(ToolKind.Hammer)&&eq.Owns(ToolKind.Wheelbarrow),"capacity stats tools money XP reload permanently");
                Directory.CreateDirectory("Logs");File.WriteAllText("Logs/GardenRockValidation.txt","PASS: seeded scenarios, varying positions, XP levels/progress, inventory upgrade, speed/carry upgrades, permanent hammer/wheelbarrow, procedural rocks, fragmentation, delivery confirmation and save round-trip.\n");
                Debug.Log("PROGRESSION_V2_VALIDATION_PASS");
            }
            finally{GameSave.PathOverride=null;EditorSceneManager.OpenScene(GardenRockSetup.ScenePath);}
        }
        public static void DeliverAll(ContractManager m){for(int i=0;i<m.Targets.Count;i++){var r=m.Targets[i];r.gameObject.SetActive(true);var p=r.GetComponent<PhysicalObject>();p.IsStored=false;foreach(var c in r.GetComponentsInChildren<Collider>())c.enabled=true;var rb=p.Body;rb.isKinematic=false;rb.position=new Vector3(-.1f+(i%2)*2,r.transform.localScale.x*.5f+.02f,5.1f+(i/2)*1.8f);rb.linearVelocity=rb.angularVelocity=Vector3.zero;}Physics.SyncTransforms();m.RefreshDelivery();}
        static void Check(bool value,string message){if(!value)throw new Exception("FAILED: "+message);}
    }
}
