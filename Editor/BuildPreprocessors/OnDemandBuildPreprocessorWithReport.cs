using Deploy.Editor.Settings;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace Deploy.Editor.BuildPreprocessors
{
    public interface IOnDemandBuildPreprocessorWithReport
    {
        void OnValidate();
    }
    
    public abstract class OnDemandBuildPreprocessorWithReport : IPreprocessBuildWithReport, IOnDemandBuildPreprocessorWithReport
    {
        public bool IsEnabled
        {
            get
            {
                var enabledPreprocessors = DeploySettings.GetOrCreate().OnDemandBuildPreprocessors;
                foreach (var enabledPreprocessor in enabledPreprocessors)
                {
                    if (enabledPreprocessor != null && enabledPreprocessor.GetType() == GetType())
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public int callbackOrder { get; }
        public void OnPreprocessBuild(BuildReport report)
        {
            if (IsEnabled)
            {
                OnPreprocessBuildInternal(report);
            }
        }
        
        protected abstract void OnPreprocessBuildInternal(BuildReport report);
        public abstract void OnValidate();
    }
}