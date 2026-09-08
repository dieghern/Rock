using UnityEngine;
namespace GetThisRock
{
    public enum ToolKind { Hands, Lever, Rope, Hammer, Wheelbarrow }
    [CreateAssetMenu(menuName="Get This Rock/Tool")]
    public sealed class ToolDefinition:ScriptableObject
    {
        public ToolKind kind;public string displayName;[TextArea]public string description;[Min(0)]public int price;
    }
}
