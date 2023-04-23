using UnityEngine;

namespace Deploy.Runtime
{
    [CreateAssetMenu(fileName = "IntVariable", menuName = "Facticus/Deploy/IntVariable", order = 0)]
    public class IntVariable : ScriptableObject
    {
        [SerializeField] private int _value;

        public int Value => _value;
    }
}