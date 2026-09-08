using UnityEngine;
namespace GetThisRock
{
    public sealed class PlayerProgression : MonoBehaviour
    {
        [SerializeField] int experience;
        public int Experience => experience;
        public int Level => 1 + Mathf.FloorToInt(Mathf.Sqrt(experience / 50f));
        public int NextLevelXP => Level * Level * 50;
        public int CurrentLevelXP => (Level-1)*(Level-1)*50;
        public float LevelProgress => Mathf.InverseLerp(CurrentLevelXP,NextLevelXP,experience);
        public void Restore(int value) => experience = Mathf.Max(0, value);
        public void AddExperience(int value) => experience += Mathf.Max(0, value);
    }
}
