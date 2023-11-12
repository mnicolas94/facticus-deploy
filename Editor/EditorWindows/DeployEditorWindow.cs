using System.Collections.Generic;
using System.IO;
using System.Linq;
using Deploy.Editor.Data;
using Deploy.Editor.Settings;
using Deploy.Editor.Utility;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Utils.Editor.Extensions;

namespace Deploy.Editor.EditorWindows
{
    public class DeployEditorWindow : EditorWindow
    {
        [SerializeField] private int _listSelectedIndex = -1;
        
        private VisualElement _contextsContainer;
        private VisualElement _settingsContainer;
        private List<DeployContext> _contexts = new List<DeployContext>();
        private ListView _list;
        private Button _addButton;
        private Button _removeButton;
        private Button _refreshButton;
        private VisualElement _inspectorContainer;
        private Label _contextNameLabel;
        private InspectorElement _inspectorElement;

        [MenuItem("Tools/Facticus/Deploy/Open Deploy editor window")]
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
            
            var contextsToggle = root.Q<ToolbarToggle>("contexts-toolbar-toggle");
            var settingsToggle = root.Q<ToolbarToggle>("settings-toolbar-toggle");
            _contextsContainer = root.Q("contexts-container");
            _settingsContainer = root.Q("settings-container");
            TabBarUtility.SetupTabBar(new List<(Toggle, VisualElement)>
            {
                (contextsToggle, _contextsContainer),
                (settingsToggle, _settingsContainer)
            });
            
            SetupContextsView(root);
            SetupSettingsView(root);
        }

        private void SetupContextsView(VisualElement root)
        {
            // get inspector element
            _inspectorContainer = root.Q("ContextEditorContainer");

            // context name label
            _contextNameLabel = root.Q<Label>("ContextNameLabel");

            // Get and populate list view
            _list = root.Q<ListView>("ContextsList");
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
            _contextsContainer.Add(splitView);
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
            DeployContextsListPopulator.LoadContexts(_contexts);
            DeployContextsListPopulator.FillListView(_list, _contexts, OnContextViewCreated);
            
            // set the previous selection index and ensure to draw the selected context
            _list.SetSelectionWithoutNotify(new []{ _listSelectedIndex });
            DrawSelectedContext();
        }

        private void OnContextViewCreated(VisualElement item)
        {
            ContextualMenuManipulator contextManipulator = new ContextualMenuManipulator(SetupContextContextMenu);
            contextManipulator.target = item;
        }
        
        private void SetupContextContextMenu(ContextualMenuPopulateEvent ctx)
        {
            var label = ctx.target;
            ctx.menu.AppendAction(
                "Duplicate",
                action => DuplicateContext(action, (VisualElement) label),
                DropdownMenuAction.Status.Normal);
        }

        private void DuplicateContext(DropdownMenuAction action, VisualElement label)
        {
            var context = label.userData as DeployContext;
            var copy = context.DuplicateScriptableObjectWithSubAssets();
            RefreshList();
        }

        private void RefreshList()
        {
            _list.Clear();
            PopulateList();
#if UNITY_2021_2_OR_NEWER
            _list.RefreshItems();
#else
            _list.Refresh();
#endif
        }

        private void OnAddNewClicked()
        {
            var newContext = CreateInstance<DeployContext>();
            var assetName = "New Context";
            CreateContextAsset(assetName, newContext);
            RefreshList();
        }

        private void OnRemoveClicked()
        {
            if (_list.selectedIndex < 0)
            {
                return;
            }
            
            var context = _list.selectedItem as DeployContext;
            var path = AssetDatabase.GetAssetPath(context);
            AssetDatabase.DeleteAsset(path);
            
            RefreshList();
        }

        private void OnRefreshClicked()
        {
            RefreshList();
        }

        private void OnListSelectionChange(IEnumerable<object> obj)
        {
            DrawSelectedContext();
        }

        private void DrawSelectedContext()
        {
            _listSelectedIndex = _list.selectedIndex;
            string contextLabelText = "";

            if (_inspectorContainer.Contains(_inspectorElement))
            {
                _inspectorContainer.Remove(_inspectorElement);
            }

            if (_listSelectedIndex < 0)
            {
                contextLabelText = "-";
            }
            else
            {
                var context = _list.selectedItem as DeployContext;
                if (context != null)
                {
                    _inspectorElement = new InspectorElement(context);
                    _inspectorContainer.Add(_inspectorElement);
                    contextLabelText = context.name;
                }
            }

            _contextNameLabel.text = contextLabelText;
        }

        private static void CreateContextAsset(string assetName, DeployContext newContext)
        {
            // create the directory if it does not exist
            var dir = DeploySettings.GetOrCreate().DefaultAssetDirectory;
            Directory.CreateDirectory(dir);
            AssetDatabase.Refresh();

            // create the new asset
            var path = Path.Combine(dir, $"{assetName}.asset");
            path = AssetDatabase.GenerateUniqueAssetPath(path);
            AssetDatabase.CreateAsset(newContext, path);
            AssetDatabase.SaveAssets();
        }
    }
}
