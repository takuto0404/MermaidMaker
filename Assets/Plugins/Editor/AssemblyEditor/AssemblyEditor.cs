using System.Reflection;
using Plugins.Runtime;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Plugins.Editor.AssemblyEditor
{
    public class AssemblyEditor : EditorWindow
    {
        private bool _selectAssembly;
        private AssemblyDefinitionAsset _asmdef;
        private Assembly _assembly;
        private static AssemblyEditor _window;

        [MenuItem("Window/MermaidMaker")]
        public static void ShowWindow()
        {
            _window = GetWindow<AssemblyEditor>();
            _window.titleContent = new GUIContent("MermaidMaker - Assembly Selection");
            NameSpaceEditor.NameSpaceEditor.CloseWindow();
        }

        private static void CloseWindow()
        {
            if(_window != null)_window.Close();
            _window = null;
        }

        private void OnGUI()
        {
            _selectAssembly = EditorGUILayout.Toggle("Select Assembly", _selectAssembly);
            if (_selectAssembly)
            {
                GUILayout.Label("Assembly", EditorStyles.boldLabel);
                _asmdef = (AssemblyDefinitionAsset)EditorGUILayout.ObjectField("Assembly", _asmdef,
                    typeof(AssemblyDefinitionAsset), false);
            }
            else
            {
                GUILayout.Label("If not selected, the scope will be limited to 'Assembly-CSharp'.",EditorStyles.miniLabel);
            }

            if (GUILayout.Button("Apply",GUILayout.Width(100),GUILayout.Height(20)))
            {
                var text = "Assembly-CSharp";
                if (_selectAssembly)
                {
                    text = _asmdef.name;
                }

                _assembly = Assembly.Load(text);
                var root = MermaidMakerUtility.GetNameSpaces(_assembly);
                NameSpaceEditor.NameSpaceEditor.ShowWindow(root,_assembly);
                CloseWindow();
            }
        }
    }
}