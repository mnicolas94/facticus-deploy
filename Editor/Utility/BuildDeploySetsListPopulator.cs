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
    public static class BuildDeploySetsListPopulator
    {
        public static void LoadSets(List<BuildDeploySet> sets)
        {
            sets.Clear();
            AssetDatabase.Refresh();
            var loadedSets = AssetDatabase.FindAssets("t:BuildDeploySet")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<BuildDeploySet>)
                .ToList();
            sets.AddRange(loadedSets); 
        }
        
        public static void FillListView(ListView listView, List<BuildDeploySet> sets, Action<VisualElement> onBind)
        {
            listView.makeItem = () => new RenamableLabel("");
            listView.bindItem = (item, index) =>
            {
                var set = sets[index];
                item.userData = set;
                if (item is RenamableLabel renamableLabel)
                {
                    renamableLabel.Text = set.name;
                    renamableLabel.OnRename += (newText) => RenameItemData(newText, renamableLabel);
                }
                
                onBind.Invoke(item);
            };
            listView.itemsSource = sets;
        }

        private static void RenameItemData(string newName, RenamableLabel renamableLabel)
        {
            var set = renamableLabel.userData as BuildDeploySet;
            Undo.RecordObject(set, "Change BuildDeploySet name");
            var path = AssetDatabase.GetAssetPath(set);
            AssetDatabase.RenameAsset(path, newName);
            EditorUtility.SetDirty(set);
            AssetDatabase.SaveAssets();
        }
    }
}