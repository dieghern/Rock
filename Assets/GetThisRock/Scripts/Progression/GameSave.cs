using System;
using System.IO;
using UnityEngine;
namespace GetThisRock
{
    public static class GameSave
    {
        [Serializable]sealed class SaveData{public int version=2,money,experience,capacity=4,speedLevel,carryLevel;public bool lever,rope,hammer,wheelbarrow;}
        public static string PathOverride;
        public static string SavePath=>PathOverride??Environment.GetEnvironmentVariable("ROCK_SAVE_PATH")??Path.Combine(Application.persistentDataPath,"rock-profile-v1.json");
        public static string LastError{get;private set;}
        public static void Load(PlayerWallet wallet,PlayerProgression progression,PlayerInventory inventory,PlayerStats stats=null)
        {
            LastError=null;wallet.Restore(0);progression.Restore(0);inventory.ResetInventory();stats?.Restore(0,0);if(!File.Exists(SavePath))return;
            try
            {
                var d=JsonUtility.FromJson<SaveData>(File.ReadAllText(SavePath));if(d==null||d.version<1||d.version>2)throw new InvalidDataException("Formato desconocido");
                wallet.Restore(d.money);progression.Restore(d.experience);inventory.RestoreCapacity(d.version>=2?d.capacity:4);stats?.Restore(d.speedLevel,d.carryLevel);
                if(d.lever)inventory.AddTool(ToolKind.Lever);if(d.rope)inventory.AddTool(ToolKind.Rope);if(d.hammer)inventory.AddTool(ToolKind.Hammer);if(d.wheelbarrow)inventory.AddTool(ToolKind.Wheelbarrow);
            }
            catch(Exception e){LastError="No se pudo leer el guardado: "+e.Message;Debug.LogWarning(LastError);}
        }
        public static bool Save(PlayerWallet wallet,PlayerProgression progression,PlayerEquipment equipment)
        {
            try
            {
                var d=new SaveData{money=wallet.Money,experience=progression.Experience,capacity=equipment.inventory.Capacity,speedLevel=equipment.stats.speedLevel,carryLevel=equipment.stats.carryLevel,lever=equipment.Owns(ToolKind.Lever),rope=equipment.Owns(ToolKind.Rope),hammer=equipment.Owns(ToolKind.Hammer),wheelbarrow=equipment.Owns(ToolKind.Wheelbarrow)};
                Directory.CreateDirectory(Path.GetDirectoryName(SavePath));string temp=SavePath+".tmp";File.WriteAllText(temp,JsonUtility.ToJson(d,true));if(File.Exists(SavePath))File.Replace(temp,SavePath,SavePath+".bak");else File.Move(temp,SavePath);LastError=null;return true;
            }
            catch(Exception e){LastError="No se pudo guardar el progreso: "+e.Message;Debug.LogWarning(LastError);return false;}
        }
        public static void SaveCurrent(){if(!Application.isPlaying)return;var m=UnityEngine.Object.FindAnyObjectByType<ContractManager>();if(m!=null&&m.progression!=null&&m.interaction!=null)Save(m.wallet,m.progression,m.interaction.equipment);}
    }
}
