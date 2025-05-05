using UnityEngine;

namespace Deploy.Tests.Editor
{
    public class TestScriptableObject : ScriptableObject
    {
        public int Int;
        public float Float;
        public string String;

        public static TestScriptableObject Get(int i = 0, float f = 0f, string s = "")
        {
            var tso = CreateInstance<TestScriptableObject>();
            tso.Int = i;
            tso.Float = f;
            tso.String = s;
            return tso;
        }

        public bool IsEqualTo(TestScriptableObject other)
        {
            if (Int != other.Int) return false;
            if (!Mathf.Approximately(Float, other.Float)) return false;
            if (String != other.String) return false;

            return true;
        }
    }
}