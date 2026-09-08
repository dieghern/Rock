using UnityEngine;

namespace GetThisRock
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class PhysicalObject : Interactable
    {
        public string displayName = "Object";
        public bool allowPickup = true;
        public bool IsStored { get; set; }
        Rigidbody body;
        public Rigidbody Body => body != null ? body : body = GetComponent<Rigidbody>();
        public override string GetPrompt(PlayerInteraction actor) =>
            allowPickup && Body.mass <= actor.EffectiveCarryMass ? $"{displayName} | E: recoger" : $"{displayName} | {Body.mass:0} kg";
        public override void Interact(PlayerInteraction actor) => actor.TryPickup(this);
        public void Push(Vector3 direction, Vector3 point, float force, float speedLimit, float duration)
        {
            direction = Vector3.ProjectOnPlane(direction, Vector3.up).normalized;
            if (Body.isKinematic || direction.sqrMagnitude < .1f || speedLimit <= 0) return;
            float speed = Mathf.Max(0, Vector3.Dot(Body.linearVelocity, direction));
            float taper = Mathf.Clamp01(1 - speed / speedLimit);
            // Clamp torque arm: clicking the top cannot produce an explosive angular impulse.
            var offset = Vector3.ClampMagnitude(point - Body.worldCenterOfMass, .45f);
            Body.AddForceAtPosition(direction * Mathf.Max(0, force) * taper * Mathf.Max(0, duration), Body.worldCenterOfMass + offset, ForceMode.Impulse);
        }
    }
}

