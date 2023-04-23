using UnityEngine;

namespace Deploy.Runtime
{
    [CreateAssetMenu(fileName = "FloatVariable", menuName = "Facticus/Deploy/FloatVariable", order = 0)]
    public class FloatVariable : ScriptableObject
    {
        [SerializeField] private float _value;

        public float Value => _value;
    }
}