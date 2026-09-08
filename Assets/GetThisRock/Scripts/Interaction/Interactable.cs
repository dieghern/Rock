using UnityEngine;

namespace GetThisRock
{
    public abstract class Interactable : MonoBehaviour
    {
        public abstract string GetPrompt(PlayerInteraction actor);
        public abstract void Interact(PlayerInteraction actor);
    }
}
