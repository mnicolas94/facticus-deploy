using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Deploy.Editor.Data;
using Deploy.Editor.VisualElements;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Deploy.Editor.Utility
{
    public static class DeployContextsListPopulator
    {
        public static void LoadContexts(List<DeployContext> contexts)
        {
            contexts.Clear();
            AssetDatabase.Refresh();
            var loadedContexts = AssetDatabase.FindAssets("t:DeployContext")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<DeployContext>)
                .ToList();
            contexts.AddRange(loadedContexts); 
        }
        
        public static void FillListView(ListView listView, List<DeployContext> contexts, Action<VisualElement> onBind)
        {
            listView.makeItem = () => new RenamableLabel("");
            listView.bindItem = (item, index) =>
            {
                var context = contexts[index];
                item.userData = context;
                if (item is RenamableLabel renamableLabel)
                {
                    renamableLabel.Text = context.name;
                    renamableLabel.OnRename += (newText) => RenameItemData(newText, renamableLabel);
                }
                
                onBind.Invoke(item);
            };
            listView.itemsSource = contexts;
        }

        private static void RenameItemData(string newName, RenamableLabel renamableLabel)
        {
            var context = renamableLabel.userData as DeployContext;
            Undo.RecordObject(context, "Change DeployContext name");
            var path = AssetDatabase.GetAssetPath(context);
            AssetDatabase.RenameAsset(path, newName);
            EditorUtility.SetDirty(context);
            AssetDatabase.SaveAssets();
        }
    }
}