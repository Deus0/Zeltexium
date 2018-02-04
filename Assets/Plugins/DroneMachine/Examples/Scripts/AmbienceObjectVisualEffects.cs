using DerelictComputer.DroneMachine;
using UnityEngine;

namespace DerelictComputer
{
    public class AmbienceObjectVisualEffects : MonoBehaviour
    {
        [SerializeField] private Renderer _renderer;
        [SerializeField] private DroneSynth _droneSynth;
        [SerializeField] private Color _color = Color.cyan;

        private Material _material;

        private void Awake()
        {
            _material = _renderer.material;
        }

        private void Update()
        {
            float brightness = Mathf.Abs(Mathf.Abs((float) _droneSynth.LfoPhase - 0.5f) - 0.5f) * 2;
            _material.color = _color*brightness;
        }
    }
}
