using UnityEngine;
namespace GetThisRock
{
    public sealed class PlayerEquipment:MonoBehaviour
    {
        public PlayerWallet wallet;public PlayerInventory inventory;public PlayerStats stats;public ToolDefinition[] shop;
        public ToolKind Selected=>inventory!=null?inventory.SelectedTool:ToolKind.Hands;
        public bool Owns(ToolKind kind)=>kind==ToolKind.Hands||(inventory!=null&&inventory.FindTool(kind)>=0);
        public bool TryEquip(ToolKind kind)=>inventory!=null&&Owns(kind)&&inventory.Select(kind==ToolKind.Hands?-1:inventory.FindTool(kind));
        public bool TryBuy(ToolDefinition tool)
        {
            if(tool==null||inventory==null||inventory.Used>=inventory.Capacity||shop==null||System.Array.IndexOf(shop,tool)<0||Owns(tool.kind)||wallet==null||!wallet.TrySpend(tool.price))return false;
            inventory.AddTool(tool.kind);GameSave.SaveCurrent();return true;
        }
        public bool TryUpgradeInventory(){if(inventory.Capacity>=PlayerInventory.MaximumCapacity)return false;int cost=inventory.NextUpgradeCost;if(!wallet.TrySpend(cost))return false;inventory.Upgrade();GameSave.SaveCurrent();return true;}
        public bool TryUpgradeSpeed(){if(stats==null||!stats.CanImproveSpeed)return false;int cost=stats.SpeedCost;if(!wallet.TrySpend(cost))return false;stats.speedLevel++;GameSave.SaveCurrent();return true;}
        public bool TryUpgradeCarry(){if(stats==null||!stats.CanImproveCarry)return false;int cost=stats.CarryCost;if(!wallet.TrySpend(cost))return false;stats.carryLevel++;GameSave.SaveCurrent();return true;}
    }
}
