using System.Collections.Generic;
using System.IO;
using Deploy.Editor.Data;
using Deploy.Editor.Utility;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Deploy.Editor.EditorWindows
{
    public class DeployEditorWindow : EditorWindow
    {
        [SerializeField] private int _listSelectedIndex = -1;
        
        private VisualElement _setsContainer;
        private VisualElement _settingsContainer;
        private List<BuildDeploySet> _sets = new List<BuildDeploySet>();
        private ListView _list;
        private Button _addButton;
        private Button _removeButton;
        private Button _refreshButton;
        private VisualElement _inspectorContainer;
        private Label _setNameLabel;
        private InspectorElement _ie;

        [MenuItem("Tools/Facticus/Deploy/Edit sets")]
        public static void ShowExample()
        {
            DeployEditorWindow wnd = GetWindow<DeployEditorWindow>();
            wnd.titleContent = new GUIContent("Deploy settings");
        }

        public void CreateGUI()
        {
            VisualElement root = rootVisualElement;

            // Import UXML
            var visualTree = Resources.Load<VisualTreeAsset>("DeployEditorWindow");
            VisualElement uxmlContent = visualTree.Instantiate();
            uxmlContent.style.flexGrow = 1;
            root.Add(uxmlContent);
            
            var setsToggle = root.Q<ToolbarToggle>("sets-toolbar-toggle");
            var settingsToggle = root.Q<ToolbarToggle>("settings-toolbar-toggle");
            _setsContainer = root.Q("sets-container");
            _settingsContainer = root.Q("settings-container");
            TabBarUtility.SetupTabBar(new List<(Toggle, VisualElement)>
            {
                (setsToggle, _setsContainer),
                (settingsToggle, _settingsContainer)
            });
            
            SetupSetsView(root);
            SetupSettingsView(root);
        }

        private void SetupSetsView(VisualElement root)
        {
            // get inspector element
            _inspectorContainer = root.Q("SetEditorContainer");

            // set name label
            _setNameLabel = root.Q<Label>("SetNameLabel");

            // Get and populate list view
            _list = root.Q<ListView>("SetsList");
            _list.onSelectionChange += OnListSelectionChange;
            RefreshList();

            // get the Add button
            _addButton = root.Q<Button>("AddButton");
            _addButton.clicked += OnAddNewClicked;

            // get the Remove button
            _removeButton = root.Q<Button>("RemoveButton");
            _removeButton.clicked += OnRemoveClicked;

            // get the Remove button
            _refreshButton = root.Q<Button>("RefreshButton");
            _refreshButton.clicked += OnRefreshClicked;

            // add a 2 pane split view
            var listContainer = root.Q("ListContainer");
            var splitView = new TwoPaneSplitView(
                0, 250, TwoPaneSplitViewOrientation.Horizontal);
            _setsContainer.Add(splitView);
            splitView.Add(listContainer);
            splitView.Add(_inspectorContainer);
        }

        private void SetupSettingsView(VisualElement root)
        {
            var settings = DeploySettings.GetOrCreate();
            var settingsElement = new InspectorElement(settings);
            _settingsContainer.Add(settingsElement);
        }

        private void PopulateList()
        {
            BuildDeploySetsListPopulator.LoadSets(_sets);
            BuildDeploySetsListPopulator.FillListView(_list, _sets);
            _list.selectedIndex = _listSelectedIndex;
        }

        private void RefreshList()
        {
            _list.Clear();
            // await Task.Delay(100);
            PopulateList();
            // await Task.Delay(100);
            _list.RefreshItems();
        }

        private void OnAddNewClicked()
        {
            // create the directory if it does not exist
            var dir = DeploySettings.GetOrCreate().DefaultAssetDirectory;
            Directory.CreateDirectory(dir);
            AssetDatabase.Refresh();

            // create the new asset
            var newSet = CreateInstance<BuildDeploySet>();
            var path = Path.Join(dir, "New Set.asset");
            path = AssetDatabase.GenerateUniqueAssetPath(path);
            AssetDatabase.CreateAsset(newSet, path);
            AssetDatabase.SaveAssets();
            
            RefreshList();
        }

        private void OnRemoveClicked()
        {
            if (_list.selectedIndex < 0)
            {
                return;
            }
            
            var set = _list.selectedItem as BuildDeploySet;
            var path = AssetDatabase.GetAssetPath(set);
            AssetDatabase.DeleteAsset(path);
            
            RefreshList();
        }

        private void OnRefreshClicked()
        {
            RefreshList();
        }

        private void OnListSelectionChange(IEnumerable<object> obj)
        {
            _listSelectedIndex = _list.selectedIndex;
            string setLabelText = "";
            
            if (_inspectorContainer.Contains(_ie))
            {
                _inspectorContainer.Remove(_ie);
            }
            
            if (_listSelectedIndex < 0)
            {
                setLabelText = "-";
            }
            else
            {
                var set = _list.selectedItem as BuildDeploySet;
                if (set != null)
                {
                    _ie = new InspectorElement(set);
                    _inspectorContainer.Add(_ie);
                    setLabelText = set.name;
                }
            }

            _setNameLabel.text = setLabelText;
        }
    }
}