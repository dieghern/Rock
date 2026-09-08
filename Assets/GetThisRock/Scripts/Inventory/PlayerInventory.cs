using System;
using UnityEngine;
namespace GetThisRock
{
    [Serializable] public sealed class InventorySlot
    {
        public ToolKind tool;public PhysicalObject rock;
        public bool IsEmpty=>tool==ToolKind.Hands&&rock==null;
        public string Label=>rock!=null?$"Roca {rock.Body.mass:0} kg":tool switch{ToolKind.Lever=>"Palanca",ToolKind.Rope=>"Cuerda",ToolKind.Hammer=>"Martillo",ToolKind.Wheelbarrow=>"Carretilla",_=>"Vacío"};
    }
    public sealed class PlayerInventory:MonoBehaviour
    {
        public const int MinimumCapacity=4,MaximumCapacity=8;
        public InventorySlot[] Slots{get;private set;}=Create(MinimumCapacity);
        public int Capacity=>Slots.Length;
        public int SelectedIndex{get;private set;}=-1;
        public int Used{get{int n=0;foreach(var s in Slots)if(!s.IsEmpty)n++;return n;}}
        public int NextUpgradeCost=>100+(Capacity-MinimumCapacity)*80;
        public PhysicalObject SelectedRock=>SelectedIndex<0?null:Slots[SelectedIndex].rock;
        public ToolKind SelectedTool=>SelectedIndex<0?ToolKind.Hands:Slots[SelectedIndex].tool;
        static InventorySlot[] Create(int count){var result=new InventorySlot[count];for(int i=0;i<count;i++)result[i]=new InventorySlot();return result;}
        public bool Select(int index){if(index < -1||index>=Capacity)return false;SelectedIndex=index;return true;}
        public int FindTool(ToolKind kind){for(int i=0;i<Capacity;i++)if(Slots[i].tool==kind&&!Slots[i].IsEmpty)return i;return-1;}
        int Free(){for(int i=0;i<Capacity;i++)if(Slots[i].IsEmpty)return i;return-1;}
        public bool AddTool(ToolKind kind){int slot=Free();if(kind==ToolKind.Hands||slot<0||FindTool(kind)>=0)return false;Slots[slot].tool=kind;return true;}
        public bool AddRock(PhysicalObject rock)
        {
            int slot=Free();if(slot<0||rock==null||rock.IsStored)return false;Slots[slot].rock=rock;rock.IsStored=true;
            rock.Body.linearVelocity=rock.Body.angularVelocity=Vector3.zero;rock.Body.isKinematic=true;rock.Body.interpolation=RigidbodyInterpolation.None;
            foreach(var c in rock.GetComponentsInChildren<Collider>())c.enabled=false;Select(slot);return true;
        }
        public bool Upgrade(){if(Capacity>=MaximumCapacity)return false;var expanded=Create(Capacity+1);Array.Copy(Slots,expanded,Slots.Length);Slots=expanded;return true;}
        public void RestoreCapacity(int value){int target=Mathf.Clamp(value,MinimumCapacity,MaximumCapacity);var old=Slots;Slots=Create(target);for(int i=0;i<Mathf.Min(old.Length,target);i++)Slots[i]=old[i];SelectedIndex=-1;}
        public void RemoveSelectedRock(){if(SelectedRock!=null)Slots[SelectedIndex].rock=null;}
        public void ClearContractRocks(){foreach(var slot in Slots)slot.rock=null;}
        public void ResetInventory(int capacity=MinimumCapacity){Slots=Create(Mathf.Clamp(capacity,MinimumCapacity,MaximumCapacity));SelectedIndex=-1;}
    }
}


