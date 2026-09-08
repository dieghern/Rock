using UnityEngine;
namespace GetThisRock
{
    [CreateAssetMenu(menuName = "Get This Rock/Content")]
    public sealed class GameContent : ScriptableObject
    {
        public Contract[] contracts;
        public ToolDefinition[] tools;
        public GameObject tabletPrefab, handPrefab;
        public Material toolMaterial, rockMaterial, grassMaterial, dirtMaterial, woodMaterial, leavesMaterial;
    }
    public static class SessionInstaller
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Install()
        {
            var manager = Object.FindAnyObjectByType<ContractManager>();
            if (manager != null) Configure(manager, true);
        }
        public static void Configure(ContractManager manager, bool loadSave)
        {
            var content = Resources.Load<GameContent>("RockContent");
            var player = manager.interaction != null ? manager.interaction : Object.FindAnyObjectByType<PlayerInteraction>();
            if (player == null || content == null) { Debug.LogError("Falta RockContent o el jugador. Ejecuta Upgrade Contracts and Shop."); return; }
            manager.interaction = player;
            manager.catalog = content.contracts;
            manager.wallet = manager.GetComponent<PlayerWallet>();
            manager.progression = GetOrAdd<PlayerProgression>(manager.gameObject);
            manager.scenarios = GetOrAdd<ScenarioDirector>(manager.gameObject);
            manager.scenarios.grass=content.grassMaterial; manager.scenarios.dirt=content.dirtMaterial; manager.scenarios.stone=content.rockMaterial; manager.scenarios.wood=content.woodMaterial; manager.scenarios.leaves=content.leavesMaterial;
            manager.rockMaterial=content.rockMaterial;
            if (manager.targetRock == null) manager.targetRock = Object.FindAnyObjectByType<Rock>(FindObjectsInactive.Include);
            if (manager.deliveryZone == null) manager.deliveryZone = Object.FindAnyObjectByType<DeliveryZone>();
            if (manager.deliveryZone != null) manager.deliveryZone.contracts = manager;
            player.inventory = GetOrAdd<PlayerInventory>(player.gameObject);
            player.equipment = GetOrAdd<PlayerEquipment>(player.gameObject);
            player.equipment.inventory = player.inventory;
            player.equipment.wallet = manager.wallet;
            player.equipment.shop = content.tools;
            player.equipment.stats = GetOrAdd<PlayerStats>(player.gameObject);
            player.tools = GetOrAdd<RockToolUse>(player.gameObject);
            player.tools.interaction = player;
            player.tools.equipment = player.equipment;
            player.tools.visualTemplate = content.toolMaterial;
            var motor = player.GetComponent<PlayerController>();
            motor.interaction = player;
            // Player collider is excluded from object raycasts and drop placement.
            player.gameObject.layer = 2;
            player.interactionMask = ~(1 << 2);
            var presentation = GetOrAdd<FirstPersonPresentation>(player.gameObject);
            presentation.interaction = player;
            presentation.content = content;
            var hud = GetOrAdd<GameHUD>(manager.gameObject);
            hud.contracts = manager; hud.wallet = manager.wallet; hud.equipment = player.equipment; hud.interaction = player; hud.presentation = presentation;
            if (loadSave) GameSave.Load(manager.wallet, manager.progression, player.inventory, player.equipment.stats);
        }
        static T GetOrAdd<T>(GameObject go) where T : Component => go.GetComponent<T>() ?? go.AddComponent<T>();
    }
}
