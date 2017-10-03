using UnityEngine;

namespace DerelictComputer
{
    [RequireComponent(typeof(BoxCollider))]
    public class ActivateObjectsOnTrigger : MonoBehaviour
    {
        public enum TriggerType
        {
            Enable,
            Trigger
        }

        [SerializeField] private TriggerType _triggerType = TriggerType.Trigger;
        [SerializeField] private GameObject[] _objectsToActivate;
        [SerializeField] private GameObject[] _objectsToDeactivate;

        private void OnEnable()
        {
            var c = GetComponent<BoxCollider>();
            c.isTrigger = true;

            if (_triggerType != TriggerType.Enable)
            {
                return;
            }

            DoActivate();
        }

        private void OnTriggerEnter()
        {
            if (_triggerType != TriggerType.Trigger)
            {
                return;
            }

            DoActivate();
        }

        private void DoActivate()
        {
            foreach (var o in _objectsToActivate)
            {
                o.SetActive(true);
            }

            foreach (var o in _objectsToDeactivate)
            {
                o.SetActive(false);
            }
        }
    }
}
