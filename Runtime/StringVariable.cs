using UnityEngine;

namespace Deploy.Runtime
{
    [CreateAssetMenu(fileName = "StringVariable", menuName = "Facticus/Deploy/StringVariable", order = 0)]
    public class StringVariable : ScriptableObject
    {
        [SerializeField] private string _value;

        public string Value => _value;
    }
}