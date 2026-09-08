using UnityEngine;
namespace GetThisRock
{
    // A compound dynamic body: the wheel and rear feet support a real, open cargo tray.
    public sealed class WheelbarrowController : MonoBehaviour
    {
        public PlayerInteraction interaction;
        GameObject cart; Rigidbody body; Transform wheel; PhysicsMaterial grip, tire;
        public bool Active { get; private set; }
        public bool Deployed => cart != null && cart.activeSelf;
        public Vector3 HandlePoint(float side) => cart.transform.TransformPoint(new Vector3(side * .39f, .72f, -.97f));
        public string Prompt => Active ? "CARRETILLA | E: apoyar en el piso" : Deployed ? "CARRETILLA | Acércate a los mangos y pulsa E para tomarla · F: manos · G: cargar roca" : "CARRETILLA | E: desplegar";
        public void Initialize(Material template)
        {
            if (cart != null) return;
            grip = new PhysicsMaterial("Tray grip") { staticFriction = .9f, dynamicFriction = .75f, bounciness = 0 };
            tire = new PhysicsMaterial("Wheel contact") { staticFriction = .12f, dynamicFriction = .08f, bounciness = 0 };
            cart = new GameObject("Carretilla"); body = cart.AddComponent<Rigidbody>();
            body.mass = 22; body.linearDamping = .25f; body.angularDamping = 3;
            body.interpolation = RigidbodyInterpolation.Interpolate; body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            body.solverIterations = 12; body.solverVelocityIterations = 6;
            Part("Bandeja", new Vector3(0,.43f,0), new Vector3(1.05f,.1f,1.25f), template);
            Part("Lado izquierdo", new Vector3(-.54f,.65f,0), new Vector3(.07f,.45f,1.25f), template);
            Part("Lado derecho", new Vector3(.54f,.65f,0), new Vector3(.07f,.45f,1.25f), template);
            Part("Frente", new Vector3(0,.65f,.63f), new Vector3(1.1f,.45f,.07f), template);
            Part("Borde trasero", new Vector3(0,.52f,-.63f), new Vector3(1.1f,.12f,.07f), template);
            for (int side = -1; side <= 1; side += 2)
            {
                Part("Mango", new Vector3(side*.39f,.69f,-.85f), new Vector3(.065f,.065f,.8f), template);
                Part("Pata", new Vector3(side*.42f,.22f,-.46f), new Vector3(.07f,.43f,.08f), template);
            }
            var wheelObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder); wheel = wheelObject.transform;
            wheel.name = "Rueda"; wheel.SetParent(cart.transform,false); wheel.localPosition = new Vector3(0,.25f,.68f);
            wheel.localScale = new Vector3(.5f,.1f,.5f); wheel.localRotation = Quaternion.Euler(0,0,90);
            wheelObject.GetComponent<Collider>().enabled = false; Dispose(wheelObject.GetComponent<Collider>());
            wheelObject.GetComponent<Renderer>().sharedMaterial = template;
            var wheelCollider = new GameObject("Contacto rueda"); wheelCollider.transform.SetParent(cart.transform,false); wheelCollider.transform.localPosition = wheel.localPosition;
            var sphere = wheelCollider.AddComponent<SphereCollider>(); sphere.radius = .25f; sphere.sharedMaterial = tire;
            body.centerOfMass = new Vector3(0,.3f,.05f);
            foreach (var c in cart.GetComponentsInChildren<Collider>()) if (interaction.playerCollider != null) Physics.IgnoreCollision(c, interaction.playerCollider);
            cart.SetActive(false);
        }
        void Part(string label, Vector3 p, Vector3 scale, Material mat)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube); go.name = label; go.transform.SetParent(cart.transform,false);
            go.transform.localPosition = p; go.transform.localScale = scale; go.GetComponent<Renderer>().sharedMaterial = mat; go.GetComponent<Collider>().sharedMaterial = grip;
        }
        public void Toggle()
        {
            if (cart == null) return;
            if (Active) { Park(); return; }
            if (!Deployed)
            {
                var p = interaction.transform.position + interaction.transform.forward * 1.65f;
                if (!Physics.Raycast(p + Vector3.up * 3, Vector3.down, out var hit, 7, interaction.interactionMask, QueryTriggerInteraction.Ignore)) return;
                p.y = hit.point.y + .08f;
                var q = Quaternion.Euler(0, interaction.transform.eulerAngles.y, 0);
                if (Physics.CheckBox(p + Vector3.up*.65f, new Vector3(.59f,.48f,.75f), q, interaction.interactionMask, QueryTriggerInteraction.Ignore)) return;
                cart.transform.SetPositionAndRotation(p, q); cart.SetActive(true);
            }
            if (Vector3.Distance(interaction.transform.position, HandlePoint(0)) > 2.3f) return;
            Active = true; body.WakeUp();
        }
        public void Park() { Active = false; }
        public void Hide() => Park();
        public void PlaceOnTerrain(ScenarioDirector terrain)
        {
            if (!Deployed) return; Park(); var p=body.position; p.y=terrain.HeightAt(p.x,p.z)+.08f;
            body.position=p; body.linearVelocity=body.angularVelocity=Vector3.zero;
        }
        void FixedUpdate()
        {
            if (!Deployed) return;
            if (Active && (!interaction.input.Captured || interaction.equipment.Selected != ToolKind.Wheelbarrow)) Park();
            if (Active)
            {
                var target = interaction.transform.position + interaction.transform.forward * 1.65f;
                var error = Vector3.ProjectOnPlane(target - body.position, Vector3.up);
                if (error.magnitude > 2.8f) { Park(); return; }
                var velocity = Vector3.ProjectOnPlane(body.linearVelocity, Vector3.up);
                body.AddForce(Vector3.ClampMagnitude(error * 310 - velocity * 100, 360));
                // Hands stabilize pitch and roll; gravity and contacts still determine height.
                var up = Vector3.up;
                float nearest = float.PositiveInfinity;
                foreach (var ground in Physics.RaycastAll(body.position + Vector3.up, Vector3.down, 3, 1 << 0, QueryTriggerInteraction.Ignore))
                    if (!ground.collider.transform.IsChildOf(cart.transform) && ground.collider.GetComponentInParent<Rigidbody>() == null && ground.distance < nearest) { nearest=ground.distance; up=ground.normal; }
                var forward = Vector3.ProjectOnPlane(interaction.transform.forward, up).normalized;
                var desired = Quaternion.LookRotation(forward, up) * Quaternion.Euler(-5,0,0);
                var delta = desired * Quaternion.Inverse(body.rotation); delta.ToAngleAxis(out float angle, out var axis);
                if (angle > 180) angle -= 360;
                if (axis.sqrMagnitude > .001f) body.AddTorque(Vector3.ClampMagnitude(axis * (angle * Mathf.Deg2Rad * 100) - body.angularVelocity * 28, 100));
            }
            wheel.Rotate(Vector3.up, Vector3.Dot(body.linearVelocity, cart.transform.forward) / .25f * Mathf.Rad2Deg * Time.fixedDeltaTime, Space.Self);
        }
        static void Dispose(Object item) { if(item==null)return; if(Application.isPlaying)Destroy(item);else DestroyImmediate(item); }
        void OnDestroy() { Dispose(cart); Dispose(grip); Dispose(tire); }
    }
}


