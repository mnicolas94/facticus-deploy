using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.UIElements;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace Deploy.Editor.Settings
{
    [InitializeOnLoad]
    public class WorkflowVersionUpdater : IPackageManagerExtension
    {
        static WorkflowVersionUpdater()
        {
            PackageManagerExtensions.RegisterExtension(new WorkflowVersionUpdater());
        }

        public VisualElement CreateExtensionUI()
        {
            return null;
        }

        public void OnPackageSelectionChange(PackageInfo packageInfo)
        {
        }

        public void OnPackageAddedOrUpdated(PackageInfo packageInfo)
        {
            Debug.Log("A package was installed or updated");
            if (UpdatedSelf(packageInfo))
            {
                Debug.Log("It was Deploy");
            }
        }

        public void OnPackageRemoved(PackageInfo packageInfo)
        {
            
        }

        private bool UpdatedSelf(PackageInfo packageInfo)
        {
            return packageInfo.name == "com.facticus.deploy";
        }
    }
}