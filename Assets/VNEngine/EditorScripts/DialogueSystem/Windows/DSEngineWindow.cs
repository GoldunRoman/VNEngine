#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace VNEngine.DS.Windows
{
    using System;
    using Utilities;

    public class DSEngineWindow : EditorWindow
    {
        private DSGraphView _graphView;

        private Button _saveButton;
        private Button _miniMapButton;

        private static TextField s_fileNameTextField;

        private readonly string _defaultFileName = "DialoguesFileName";

        [MenuItem("Window/VNEngine/DialogueGraph")]
        public static void Open()
        {
            GetWindow<DSEngineWindow>("Dialogue Graph");
        }

        private void OnEnable()
        {
            AddGraphView();
            AddToolbar();

            AddStyles();
        }

        #region Elements Addition
        private void AddGraphView()
        {
            _graphView = new DSGraphView(this);

            _graphView.StretchToParentSize();

            rootVisualElement.Add(_graphView);
        }

        private void AddToolbar()
        {
            Toolbar toolbar = new Toolbar();

            s_fileNameTextField = DSElementUtility.CreateTextField(_defaultFileName, "File Name:", callback =>
            {
                s_fileNameTextField.value = callback.newValue.RemoveWhitespaces().RemoveSpecialCharacters();
            });

            _saveButton = DSElementUtility.CreateButton("Save", () => Save());

            Button loadButton = DSElementUtility.CreateButton("Load", () => Load());
            Button clearButton = DSElementUtility.CreateButton("Clear", () => Clear());
            Button resetButton = DSElementUtility.CreateButton("Reset", () => ResetGraph());
            _miniMapButton = DSElementUtility.CreateButton("Minimap", () => ToggleMiniMap());

            toolbar.Add(s_fileNameTextField);
            toolbar.Add(_saveButton);
            toolbar.Add(loadButton);
            toolbar.Add(clearButton);
            toolbar.Add(resetButton);
            toolbar.Add(_miniMapButton);

            toolbar.AddStyleSheets("DialogueSystem/DSToolbarStyles.uss");

            rootVisualElement.Add(toolbar);
        }

        private void AddStyles()
        {
            rootVisualElement.AddStyleSheets("DialogueSystem/DSVariables.uss");
        }
        #endregion

        #region Toolbar Actions
        private void Save()
        {
            if (string.IsNullOrEmpty(s_fileNameTextField.value))
            {
                EditorUtility.DisplayDialog(
                    "Invalid file name.",
                    "Please ensure the file name you`ve typed in is valid",
                    "Roger!"
                    );

                return;
            }

            DSInputOutputUtility.Initialize(_graphView, s_fileNameTextField.value);
            DSInputOutputUtility.Save();
        }

        private void Load()
        {
            string filePath = EditorUtility.OpenFilePanel("Dialogue Graphs", "Assets/VNEngine/DialoguesSavedAssets/Graphs", "asset");

            if (string.IsNullOrEmpty(filePath))
                return;

            Clear();

            DSInputOutputUtility.Initialize(_graphView, Path.GetFileNameWithoutExtension(filePath));
            DSInputOutputUtility.Load();
        }

        private void Clear()
        {
            _graphView.ClearGraph();
        }

        private void ResetGraph()
        {
            Clear();

            UpdateFileName(_defaultFileName);
        }

        private void ToggleMiniMap()
        {
            _graphView.ToggleMiniMap();

            _miniMapButton.ToggleInClassList(".ds-toolbar__button__selected");
        }
        #endregion

        #region Utility Methods
        public static void UpdateFileName(string newFileName)
        {
            s_fileNameTextField.value = newFileName;
        }

        public void EnableSaving()
        {
            _saveButton.SetEnabled(true);
        }

        public void DisableSaving()
        {
            _saveButton.SetEnabled(false);
        }
        #endregion
    }
}
#endif