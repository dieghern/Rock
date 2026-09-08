using UnityEngine;
namespace GetThisRock
{
    public sealed class PlayerInteraction : MonoBehaviour
    {
        public PlayerInput input;
        public Camera view;
        public Collider playerCollider;
        public PlayerEquipment equipment;
        public PlayerInventory inventory;
        public RockToolUse tools;
        public LayerMask interactionMask = ~0;
        public float reach = 2.2f, maximumCarryMass = 12, carryDistance = .85f;
        public float EffectiveCarryMass=>equipment!=null&&equipment.stats!=null?equipment.stats.CarryMass:maximumCarryMass;
        public float pushForce = 145, maximumPushSpeed = .85f, effortBuildTime = .9f;
        public float carrySpring = 100, carryDamping = 18, maximumCarryForce = 250;
        PhysicalObject pushing;
        public string Prompt { get; private set; }
        public float Effort { get; private set; }
        public Vector3 PushPoint { get; private set; }
        public Vector3 PushNormal { get; private set; }
        public float PickupMotion { get; private set; }
        public float MovementMultiplier => tools != null && tools.WheelbarrowActive ? .42f : inventory != null && inventory.SelectedRock != null ? .8f : tools != null && tools.IsTaut ? .4f : Effort > .1f ? .3f : 1;
        public bool Ray(out RaycastHit hit) => Physics.Raycast(view.transform.position, view.transform.forward, out hit, reach, interactionMask, QueryTriggerInteraction.Ignore);
        void Update()
        {
            if (inventory == null || input == null) return;
            Prompt = "";
            if (input.ToolSlot >= -1 && inventory.Select(input.ToolSlot)) ReleaseObjects();
            if (!input.Captured) { ReleaseObjects(); return; }
            if (input.ThrowPressed && inventory.SelectedRock != null) { TryThrowSelected(); return; }
            if (input.DropPressed && inventory.SelectedRock != null) { TryDropSelected(); return; }
            if (equipment.Selected != ToolKind.Hands) { Prompt = tools.HandleInput(); return; }
            if (inventory.SelectedRock != null) Prompt = "G: soltar roca seleccionada | E: recoger otra | teclas numéricas: inventario";
            if (!Ray(out var hit)) return;
            var target = hit.collider.GetComponentInParent<Interactable>();
            if (target == null) return;
            Prompt = target.GetPrompt(this);
            if (input.InteractPressed) target.Interact(this);
        }
        void LateUpdate()
        {
            if (inventory == null || view == null) return;
            foreach (var slot in inventory.Slots)
            {
                if (slot.rock == null) continue;
                bool visible = slot.rock == inventory.SelectedRock && !input.UIBlocked;
                slot.rock.gameObject.SetActive(visible);
                if (!visible) continue;
                var destination = view.transform.TransformPoint(new Vector3(0, -.24f, .8f));
                slot.rock.transform.position = Vector3.Lerp(slot.rock.transform.position, destination, 1 - Mathf.Exp(-12 * Time.deltaTime));
                slot.rock.transform.rotation = Quaternion.Slerp(slot.rock.transform.rotation, view.transform.rotation, 1 - Mathf.Exp(-10 * Time.deltaTime));
            }
            PickupMotion = Mathf.MoveTowards(PickupMotion, 0, Time.deltaTime * 2.5f);
        }
        void FixedUpdate()
        {
            if (inventory == null || !input.Captured || inventory.SelectedRock != null || equipment.Selected != ToolKind.Hands)
            { Effort = 0; pushing = null; return; }
            if (input.PushHeld && Ray(out var hit))
            {
                var target = hit.collider.GetComponentInParent<PhysicalObject>();
                if (target != null && !target.IsStored)
                {
                    if (target != pushing) Effort = 0;
                    pushing = target; PushPoint = hit.point; PushNormal = hit.normal;
                    Effort = Mathf.MoveTowards(Effort, 1, Time.fixedDeltaTime / Mathf.Max(.1f, effortBuildTime));
                    target.Push(target.Body.worldCenterOfMass - transform.position, hit.point, pushForce * Mathf.SmoothStep(0, 1, Effort), maximumPushSpeed, Time.fixedDeltaTime);
                    return;
                }
            }
            Effort = 0; pushing = null;
        }
        public bool TryPickup(PhysicalObject target)
        {
            if (target == null || !target.allowPickup || target.Body.mass > EffectiveCarryMass || target.IsStored || inventory == null) return false;
            if (!inventory.AddRock(target)) { Prompt = "Inventario lleno. Suelta una roca con G o compra espacio."; return false; }
            tools.Release(); PickupMotion = 1; Effort = 0;
            return true;
        }
        public bool TryThrowSelected(){if(inventory.SelectedRock==null)return false;var target=inventory.SelectedRock;if(!TryDropSelected())return false;target.Body.AddForce(view.transform.forward*12f,ForceMode.Impulse);return true;}
        public bool TryDropSelected()
        {
            var target = inventory.SelectedRock;
            if (target == null) return false;
            float radius = target.transform.lossyScale.x * .5f;
            var direction = view.transform.forward;
            Vector3 position;
            if (Physics.SphereCast(view.transform.position, radius, direction, out var hit, 1.35f, interactionMask, QueryTriggerInteraction.Ignore))
                position = hit.point + hit.normal * (radius + .04f);
            else position = view.transform.position + direction * 1.35f;
            if (Physics.CheckSphere(position, radius * .95f, interactionMask, QueryTriggerInteraction.Ignore))
            { Prompt = "No hay espacio para soltar aquí."; return false; }
            inventory.RemoveSelectedRock();
            target.gameObject.SetActive(true);
            target.transform.SetParent(null, true);
            target.transform.position = position;
            target.IsStored = false;
            foreach (var col in target.GetComponentsInChildren<Collider>()) col.enabled = true;
            target.Body.isKinematic = false;
            target.Body.interpolation = RigidbodyInterpolation.Interpolate;
            target.Body.position = position;
            target.Body.linearVelocity = target.Body.angularVelocity = Vector3.zero;
            target.Body.WakeUp();
            PickupMotion = .5f;
            Physics.SyncTransforms();
            return true;
        }
        public void ReleaseObjects() { Effort = 0; pushing = null; if (tools != null) tools.Release(); }
        void OnDisable() => ReleaseObjects();
    }
}


