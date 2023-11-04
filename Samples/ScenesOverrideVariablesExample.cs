using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Deploy.Samples
{
    public class ScenesOverrideVariablesExample : ScriptableObject
    {
#if UNITY_EDITOR
        [SerializeField] private List<SceneAsset> _scenes;
#endif
    }
}