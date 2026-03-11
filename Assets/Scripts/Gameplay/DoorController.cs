using UnityEngine;

namespace Flyfe.Gameplay
{
    public class DoorController : MonoBehaviour, IResettable
    {
        [SerializeField] private Vector3 positionOffsetWhenOpen = new Vector3(0, 3, 0); 
        [SerializeField] private float slideSpeed = 5f;
        
        private Vector3 closedPos;
        private Vector3 targetPos;

        void Awake()
        {
            closedPos = transform.position;
            targetPos = closedPos;
        }

        public void ResetState()
        {
            transform.position = closedPos;
            targetPos = closedPos;
        }

        void Update()
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, slideSpeed * Time.deltaTime);
        }

        public void SetOpen(bool isOpen)
        {
            targetPos = isOpen ? closedPos + positionOffsetWhenOpen : closedPos;
        }

        public void Open() => SetOpen(true);
        public void Close() => SetOpen(false);
    }
}
