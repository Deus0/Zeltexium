using UnityEngine;

namespace DerelictComputer.DroneMachine
{
    public class KeyChanger : MonoBehaviour
    {
        public enum TriggerType
        {
            Awake,
            Start,
            Enable,
            TriggerEnter
        }

        [SerializeField] private TriggerType _triggerType = TriggerType.TriggerEnter;
        [SerializeField] private MusicMathUtils.Note _rootNote;
        [SerializeField] private MusicMathUtils.ScaleMode _scaleMode;
        [SerializeField] private double _frequency = 0.25;
        [SerializeField] private double _frequencyChangeTime = 0;

        private Collider _collider;

        private void Awake()
        {
            if (_triggerType == TriggerType.Awake)
            {
                DoTrigger();
            }
            else if (_triggerType == TriggerType.TriggerEnter)
            {
                _collider = GetComponent<Collider>();

                if (_collider == null)
                {
                    _collider = gameObject.AddComponent<BoxCollider>();
                    _collider.isTrigger = true;
                }
            }
        }

        private void Start()
        {
            if (_triggerType == TriggerType.Start)
            {
                DoTrigger();
            }
        }

        private void OnEnable()
        {
            if (_triggerType == TriggerType.Enable)
            {
                DoTrigger();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_triggerType == TriggerType.TriggerEnter)
            {
                if (other.CompareTag("Player"))
                {
                    DoTrigger();
                }
            }
        }

        private void OnDrawGizmos()
        {
            if (_triggerType == TriggerType.TriggerEnter)
            {
                var collider = GetComponent<BoxCollider>();

                if (collider != null)
                {
                    Gizmos.matrix = transform.localToWorldMatrix;
                    Gizmos.color = Color.green;
                    Gizmos.DrawWireCube(collider.center, collider.size);
                }
                else
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawWireCube(transform.position, Vector3.one);
                }
            }
        }

        public void DoTrigger()
        {
            DerelictComputer.DroneMachine.DroneMachine.Instance.SetKey(_rootNote, _scaleMode);
            DerelictComputer.DroneMachine.DroneMachine.Instance.SetFrequency(_frequency*Random.Range(0.8f, 1.2f), _frequencyChangeTime);
        }

        private float lastTriggered;
        public float Cooldown = 0f;
        public void Update()
        {
            if (Cooldown != 0f && Time.time - lastTriggered >= Cooldown)
            {
                lastTriggered = Time.time;
                DoTrigger();
            }
        }
    }
}
