using System;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Plugins.MermaidMaker.Editor
{
    public sealed class AssemblyEditor : EditorWindow
    {
        public static bool AssemblySpecified;
        public static AssemblyDefinitionAsset SelectedAssemblyDefinitionAsset;
        private static AssemblyEditor _window;
        public static event EventHandler ProcessCompleted;
        
        public static void ShowWindow()
        {
            _window = GetWindow<AssemblyEditor>();
            _window.titleContent = new GUIContent("MermaidMaker - Assembly Selection");
            NameSpaceEditor.CloseWindow();
        }

        private static void CloseWindow()
        {
            if (_window != null) _window.Close();
            _window = null;
        }

        private void OnGUI()
        {
            AssemblySpecified = EditorGUILayout.Toggle("Select Assembly", AssemblySpecified);
            if (AssemblySpecified)
            {
                GUILayout.Label("Assembly", EditorStyles.boldLabel);
                SelectedAssemblyDefinitionAsset =
                    (AssemblyDefinitionAsset)EditorGUILayout.ObjectField("Assembly",
                        SelectedAssemblyDefinitionAsset,
                        typeof(AssemblyDefinitionAsset), false);
            }
            else
            {
                GUILayout.Label("If not selected, the scope will be limited to 'Assembly-CSharp'.",
                    EditorStyles.miniLabel);
            }

            if (!GUILayout.Button("Apply", GUILayout.Width(100), GUILayout.Height(20))) return;

            CloseWindow();
            OnProcessCompleted();
        }

        private void OnProcessCompleted()
        {
            ProcessCompleted?.Invoke(this, EventArgs.Empty);
        }
    }
}