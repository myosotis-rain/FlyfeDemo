using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Flyfe.Gameplay;
using Flyfe.Camera;
using Flyfe.Player;

namespace Flyfe.Dialogue
{
    public enum CutsceneTriggerMode
    {
        Manual,     
        Proximity   
    }

    public class CutsceneTrigger2D : MonoBehaviour, IInteractable
    {
        [SerializeField] private CutsceneController targetCutscene;
        [SerializeField] private MemorySwitch linkedSwitch;
        [SerializeField] private CutsceneTriggerMode triggerMode = CutsceneTriggerMode.Proximity;
        [SerializeField] private float autoTriggerRadius = 3f;
        [SerializeField] private bool persistAfterDeath = true;
        [SerializeField] private bool destroyAfterUse = true;
        [SerializeField] private string playerTag = "Player";
        [SerializeField] private string interactPrompt = "Examine";

        [Header("Positioning")]
        [SerializeField] private bool snapToCenter = true;
        [SerializeField] private float snapSpeed = 5f;

        private static HashSet<string> _triggeredCutscenes = new HashSet<string>();
        private bool _hasTriggered = false;
        private Transform _playerTransform;

        private void Awake()
        {
            string id = name + transform.position.ToString();
            if (persistAfterDeath && _triggeredCutscenes.Contains(id))
            {
                _hasTriggered = true;
            }
        }

        void Start()
        {
            GameObject player = GameObject.FindGameObjectWithTag(playerTag);
            if (player != null) _playerTransform = player.transform;
        }

        void Update()
        {
            if (triggerMode == CutsceneTriggerMode.Proximity && !_hasTriggered)
            {
                if (_playerTransform == null)
                {
                    GameObject player = GameObject.FindGameObjectWithTag(playerTag);
                    if (player != null) _playerTransform = player.transform;
                    return;
                }

                if (Vector2.Distance(transform.position, _playerTransform.position) <= autoTriggerRadius)
                {
                    Trigger();
                }
            }
        }

        public void Interact(GameObject user)
        {
            if (triggerMode == CutsceneTriggerMode.Manual && !_hasTriggered)
            {
                Trigger();
            }
        }

        public string GetInteractPrompt()
        {
            return triggerMode == CutsceneTriggerMode.Manual ? interactPrompt : "";
        }

        private void Trigger()
        {
            if (targetCutscene != null && !_hasTriggered)
            {
                _hasTriggered = true;

                if (persistAfterDeath)
                {
                    string id = name + transform.position.ToString();
                    _triggeredCutscenes.Add(id);
                }

                StartCoroutine(PreparePlayerAndStart());
            }
        }

        private IEnumerator PreparePlayerAndStart()
        {
            GameObject player = GameObject.FindGameObjectWithTag(playerTag);
            if (player != null && player.TryGetComponent<PlayerInputController>(out var input))
            {
                input.SetInputLocked(true);
            }

            if (linkedSwitch != null)
            {
                linkedSwitch.TurnOn();
                yield return new WaitUntil(() => linkedSwitch.GetCurrentState() == MemorySwitch.SwitchState.On);
            }

            if (player != null && snapToCenter)
            {
                Vector3 targetPos = new Vector3(transform.position.x, player.transform.position.y, player.transform.position.z);
                float timeout = 0.5f;
                float elapsed = 0;

                while (Vector2.Distance(new Vector2(player.transform.position.x, 0), new Vector2(targetPos.x, 0)) > 0.1f && elapsed < timeout)
                {
                    player.transform.position = Vector3.MoveTowards(player.transform.position, targetPos, snapSpeed * Time.deltaTime);
                    elapsed += Time.deltaTime;
                    yield return null;
                }
                player.transform.position = targetPos;
            }

            if (targetCutscene != null)
            {
                targetCutscene.StartCutscene();
                yield return new WaitUntil(() => !targetCutscene.IsActive);

                if (linkedSwitch != null)
                {
                    linkedSwitch.TurnOff();
                }
            }

            if (destroyAfterUse)
            {
                if (TryGetComponent<Collider2D>(out var col)) col.enabled = false;
            }
        }
    }
}
