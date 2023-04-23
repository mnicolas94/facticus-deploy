using UnityEngine;

namespace Deploy.Runtime
{
    [CreateAssetMenu(fileName = "BoolVariable", menuName = "Facticus/Deploy/BoolVariable", order = 0)]
    public class BoolVariable : ScriptableObject
    {
        [SerializeField] private bool _value;

        public bool Value => _value;
    }
}