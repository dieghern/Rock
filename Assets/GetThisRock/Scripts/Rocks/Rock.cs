using UnityEngine;

namespace GetThisRock
{
    [RequireComponent(typeof(Rigidbody), typeof(PhysicalObject))]
    public sealed class Rock : MonoBehaviour
    {
        public string targetId = "garden-rock";
        public string rockType = "Granite";
        [Min(.1f)] public float mass = 180f;
        [Min(0)] public float resistance = 100f;
        [Range(0, 100)] public float integrity = 100f;
        [Min(0)] public int value = 0;
        // Size comes from geometry; future contents/fracture belong in separate components.
        public Vector3 Size => GetComponent<Collider>().bounds.size;
        void Awake() => ApplyMass();
        void OnValidate() => ApplyMass();
        public void ApplyMass() => GetComponent<Rigidbody>().mass = Mathf.Max(.1f, mass);
    }
}
