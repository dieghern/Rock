using UnityEngine;

namespace GetThisRock
{
    [RequireComponent(typeof(BoxCollider))]
    public sealed class DeliveryZone : MonoBehaviour
    {
        public string zoneId = "roadside";
        public ContractManager contracts;
        void OnTriggerStay(Collider other)
        {
            var rock = other.GetComponentInParent<Rock>();
            if (rock != null && contracts != null) contracts.TryCompleteDelivery(rock, this);
        }
        public bool Contains(Rock rock)
        {
            if (rock == null) return false;
            var box = GetComponent<BoxCollider>();
            var half = box.size * .5f;
            bool found = false;
            foreach (var collider in rock.GetComponentsInChildren<Collider>())
            {
                if (!collider.enabled || collider.isTrigger || !collider.gameObject.activeInHierarchy) continue;
                found = true;
                // Conservative containment of every collider, rather than pivot/trigger overlap.
                var b = collider.bounds;
                for (int i = 0; i < 8; i++)
                {
                    var corner = b.center + Vector3.Scale(b.extents, new Vector3((i & 1) == 0 ? -1 : 1, (i & 2) == 0 ? -1 : 1, (i & 4) == 0 ? -1 : 1));
                    var p = transform.InverseTransformPoint(corner) - box.center;
                    if (Mathf.Abs(p.x) > half.x || Mathf.Abs(p.y) > half.y || Mathf.Abs(p.z) > half.z) return false;
                }
            }
            return found;
        }
    }
}
