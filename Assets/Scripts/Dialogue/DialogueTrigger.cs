using UnityEngine;
using Flyfe.Gameplay;
using Flyfe.UI;
using Flyfe.Core;

namespace Flyfe.Dialogue
{
    public class DialogueTrigger : MonoBehaviour, IInteractable
    {
        [SerializeField] private DialogueConversation conversation;
        [SerializeField] private bool triggerOnStart = false;
        [SerializeField] private bool oneTimeUse = false;
        [SerializeField] private float autoTriggerRadius = 0f; 

        private bool _hasTriggered = false;

        void Start()
        {
            if (triggerOnStart) Trigger();
        }

        void Update()
        {
            if (autoTriggerRadius > 0 && !_hasTriggered)
            {
                GameObject player = GameObject.FindGameObjectWithTag(Tags.Player);
                if (player != null && Vector2.Distance(transform.position, player.transform.position) <= autoTriggerRadius)
                {
                    Trigger();
                }
            }
        }

        public void Interact(GameObject user) => Trigger();

        public string GetInteractPrompt() => _hasTriggered && oneTimeUse ? "" : "Examine";

        public void Trigger()
        {
            if (oneTimeUse && _hasTriggered) return;
            if (conversation == null) return;

            _hasTriggered = true;
            DialogueUI.Instance.StartConversation(conversation);
        }
    }
}
