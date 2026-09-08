using UnityEngine;
namespace GetThisRock
{
    public sealed class PlayerStats:MonoBehaviour
    {
        public int speedLevel,carryLevel;
        public float BaseSpeed=4.5f,BaseCarryMass=12;
        public float MoveSpeed=>BaseSpeed+speedLevel*.45f;
        public float CarryMass=>BaseCarryMass+carryLevel*3;
        public int SpeedCost=>80+speedLevel*70;
        public int CarryCost=>80+carryLevel*70;
        public bool CanImproveSpeed=>speedLevel<5;
        public bool CanImproveCarry=>carryLevel<5;
        public void Restore(int speed,int carry){speedLevel=Mathf.Clamp(speed,0,5);carryLevel=Mathf.Clamp(carry,0,5);}
    }
}
