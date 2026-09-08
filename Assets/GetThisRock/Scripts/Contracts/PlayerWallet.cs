using System;
using UnityEngine;

namespace GetThisRock
{
    public sealed class PlayerWallet : MonoBehaviour
    {
        [SerializeField, Min(0)] int money;
        public int Money => money;
        public void Restore(int value) { money = Mathf.Max(0, value); Changed?.Invoke(money); }
        public event Action<int> Changed;
        public bool TrySpend(int amount)
        {
            if (amount < 0 || amount > money) return false;
            money -= amount;
            Changed?.Invoke(money);
            return true;
        }
        public void Credit(int amount)
        {
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
            money = checked(money + amount);
            Changed?.Invoke(money);
        }
    }
}
