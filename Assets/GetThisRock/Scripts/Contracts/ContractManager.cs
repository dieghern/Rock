using System;
using System.Collections.Generic;
using UnityEngine;
namespace GetThisRock
{
    public enum ContractState { Idle, Active, ReadyToFinish, Completed }
    public sealed class ContractManager : MonoBehaviour
    {
        public Contract[] catalog;
        public Contract activeContract;
        public PlayerWallet wallet;
        public PlayerProgression progression;
        public ScenarioDirector scenarios;
        public Material rockMaterial;
        int acceptanceCount;
        public Rock targetRock;
        public DeliveryZone deliveryZone;
        public PlayerInteraction interaction;
        readonly List<Rock> targets = new List<Rock>();
        public IReadOnlyList<Rock> Targets => targets;
        public ContractState State { get; private set; }
        public bool Completed => State == ContractState.Completed;
        public bool HasActiveJob => State == ContractState.Active || State == ContractState.ReadyToFinish;
        public int DeliveredCount { get; private set; }
        public int TotalCount => targets.Count;
        public string LastMessage { get; private set; }
        public int CompletedCount { get; private set; }
        public event Action<Contract> ContractCompleted;
        public bool IsUnlocked(Contract c) => c != null && progression != null && progression.Level >= c.requiredLevel;
        public bool WasCompleted(Contract c) => false;
        void Start()
        {
            activeContract = null; State = ContractState.Idle;
            if (targetRock != null) targetRock.gameObject.SetActive(false);
        }
        void FixedUpdate() => RefreshDelivery();
        public bool TryAccept(Contract c)
        {
            if (HasActiveJob) { LastMessage = "Termina el contrato actual antes de aceptar otro."; return false; }
            if (!IsUnlocked(c) || catalog == null || Array.IndexOf(catalog, c) < 0 || targetRock == null || deliveryZone == null || c.rocks == null || c.rocks.Length == 0)
            { LastMessage = "Contrato no disponible o configuración incompleta."; return false; }
            interaction.ReleaseObjects();
            interaction.inventory.ClearContractRocks();
            foreach (var item in targets) if (item != null && item != targetRock) { item.gameObject.SetActive(false); if (Application.isPlaying) Destroy(item.gameObject); else DestroyImmediate(item.gameObject); }
            targets.Clear();
            activeContract = c;
            acceptanceCount++;
            scenarios?.Build(c, acceptanceCount);
            targetRock.gameObject.SetActive(false);
            for (int i = 0; i < c.rocks.Length; i++)
            {
                Rock rock = i == 0 ? targetRock : Instantiate(targetRock);
                var spec = c.rocks[i];
                if (scenarios != null) spec.position = scenarios.Resolve(spec.position, i);
                rock.transform.SetParent(null);
                rock.transform.localScale = Vector3.one * spec.diameter;
                rock.transform.SetPositionAndRotation(spec.position, Quaternion.identity);
                rock.mass = spec.mass; rock.integrity = 100; rock.targetId = c.targetId + "-" + i;
                var physical = rock.GetComponent<PhysicalObject>();
                physical.IsStored = false;
                physical.allowPickup = spec.mass <= interaction.EffectiveCarryMass;
                physical.displayName = spec.mass <= interaction.EffectiveCarryMass ? "Roca pequeña" : "Roca pesada";
                var breaker = rock.GetComponent<RockBreaker>();
                if (breaker == null) breaker = rock.gameObject.AddComponent<RockBreaker>();
                var visual = rock.GetComponent<RockVisual>();
                if (visual == null) visual = rock.gameObject.AddComponent<RockVisual>();
                visual.material = rockMaterial; visual.Refresh(c.scenarioSeed + i * 37 + acceptanceCount * 101);
                foreach (var col in rock.GetComponentsInChildren<Collider>(true)) col.enabled = true;
                var body = rock.GetComponent<Rigidbody>();
                body.isKinematic = false;
                body.mass = spec.mass;
                body.position = spec.position;
                body.rotation = Quaternion.identity;
                body.linearVelocity = body.angularVelocity = Vector3.zero;
                rock.gameObject.SetActive(true);
                targets.Add(rock);
            }
            State = ContractState.Active; DeliveredCount = 0;
            LastMessage = "Lleva todas las rocas a ENTREGA; suéltalas y pulsa Enter para cobrar.";
            Physics.SyncTransforms();
            return true;
        }
        public void ReplaceRock(Rock original, System.Collections.Generic.IReadOnlyList<Rock> fragments)
        {
            int index=targets.IndexOf(original); if(index<0)return; targets.RemoveAt(index);
            for(int i=0;i<fragments.Count;i++)targets.Insert(index+i,fragments[i]);
            RefreshDelivery();
        }
        public void RefreshDelivery()
        {
            if (!HasActiveJob) return;
            DeliveredCount = 0;
            foreach (var rock in targets)
                if (rock != null && rock.gameObject.activeInHierarchy && !rock.GetComponent<PhysicalObject>().IsStored &&
                    rock.integrity >= activeContract.minimumIntegrity && deliveryZone != null && deliveryZone.Contains(rock)) DeliveredCount++;
            State = TotalCount > 0 && DeliveredCount == TotalCount ? ContractState.ReadyToFinish : ContractState.Active;
        }
        public bool TryCompleteDelivery(Rock rock, DeliveryZone zone)
        {
            if (!HasActiveJob || zone != deliveryZone || !targets.Contains(rock)) return false;
            RefreshDelivery(); return State == ContractState.ReadyToFinish;
        }
        public bool TryFinish()
        {
            Physics.SyncTransforms(); RefreshDelivery();
            if (State != ContractState.ReadyToFinish || wallet == null)
            { LastMessage = HasActiveJob ? $"Faltan {TotalCount - DeliveredCount} rocas: suéltalas completamente dentro de ENTREGA." : "Acepta un contrato desde la tablet."; return false; }
            State = ContractState.Completed; CompletedCount++;
            wallet.Credit(activeContract.reward);
            progression.AddExperience(activeContract.experienceReward);
            LastMessage = $"Contrato terminado: +${activeContract.reward} y +{activeContract.experienceReward} XP.";
            GameSave.SaveCurrent();
            ContractCompleted?.Invoke(activeContract);
            return true;
        }
    }
}


