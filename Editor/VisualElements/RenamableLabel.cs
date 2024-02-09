using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Deploy.Editor.VisualElements
{
    public class RenamableLabel : VisualElement
    {
        public Action<string> OnRename;
        
        protected Label _label;
        protected TextField _renameField;

        public string Text {
            get => _label.text;
            set {
                if (string.IsNullOrEmpty(value) || value.Equals("(Unnamed)"))
                {
                    _label.text = "(Unnamed)";
                    _label.style.unityFontStyleAndWeight = FontStyle.Italic;
                }
                else
                {
                    _label.text = value;
                    _label.style.unityFontStyleAndWeight = FontStyle.Normal;
                }
            }
        }
 
        public RenamableLabel(string text)
        {
            RegisterCallback<MouseDownEvent>(MouseRename, TrickleDown.TrickleDown);
            
            focusable = true;
            pickingMode = PickingMode.Position;
            RegisterCallback<KeyDownEvent>(KeyboardShortcuts, TrickleDown.TrickleDown);

            _label = new Label(text);
            Insert(0, _label);
 
            _renameField = new TextField { name = "textField", isDelayed = true };
            _renameField.style.display = DisplayStyle.None;
            _renameField.ElementAt(0).style.fontSize = _label.style.fontSize;
            _renameField.ElementAt(0).style.height = 18f;
            // _renameField.style.paddingTop = 8.5f;
            // _renameField.style.paddingLeft = 4f;
            // _renameField.style.paddingRight = 4f;
            // _renameField.style.paddingBottom = 7.5f;
            Insert(1, _renameField);
 
            VisualElement textInput = _renameField.Q(TextField.textInputUssName);
            textInput.RegisterCallback<FocusOutEvent>(EndRename);
        }
 
        private void MouseRename(MouseDownEvent evt)
        {
            if (evt.clickCount == 2 && evt.button == (int)MouseButton.LeftMouse)
            {
                StartRename();
            }
        }
 
        private void KeyboardShortcuts(KeyDownEvent evt)
        {
            if (evt.keyCode == KeyCode.F2)
            {
                StartRename();
            }
        }
 
        public void StartRename()
        {
            _label.style.display = DisplayStyle.None;
            _renameField.SetValueWithoutNotify(Text);
            _renameField.style.display = DisplayStyle.Flex;
            _renameField.Q(TextField.textInputUssName).Focus();
            _renameField.SelectAll();
        }
 
        private void EndRename(FocusOutEvent evt)
        {
            _label.style.display = DisplayStyle.Flex;
            _renameField.style.display = DisplayStyle.None;
 
            if (Text != _renameField.text)
            {
                Text = _renameField.text;
                OnRename?.Invoke(Text);
            }
        }
    }
}