using UnityEngine;
namespace GetThisRock
{
    [System.Serializable]
    public struct RockSpawn
    {
        [Min(.1f)] public float mass;
        [Min(.1f)] public float diameter;
        public Vector3 position;
    }
    public enum SuccessCondition { DeliverWholeRock }
    public enum ScenarioPreset { Garden, Orchard, Construction, Hills }
    [CreateAssetMenu(menuName = "Get This Rock/Contract")]
    public sealed class Contract : ScriptableObject
    {
        public string contractId = "001", displayName, mapScene = "GardenRock", targetId = "garden-rock", deliveryZoneId = "roadside";
        [TextArea] public string description, objective;
        public SuccessCondition successCondition;
        public int reward = 20, experienceReward = 25, requiredLevel = 1;
        public float minimumIntegrity;
        public RockSpawn[] rocks;
        public ScenarioPreset scenario;
        public int scenarioSeed=1001;
        // Legacy fields retained only so existing custom assets can be migrated.
        [HideInInspector] public float rockMass = 35, rockDiameter = .85f;
        [HideInInspector] public Vector3 spawnPosition;
        [HideInInspector] public int requiredCompletedContracts;
    }
}
