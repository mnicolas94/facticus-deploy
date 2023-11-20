#if UNITY_EDITOR

using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace Deploy.Samples
{
    public class ScenesConfigPreBuild : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            // configurar escenas
        }
    }
}

#endif