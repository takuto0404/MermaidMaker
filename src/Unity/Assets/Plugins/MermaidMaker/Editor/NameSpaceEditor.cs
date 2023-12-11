using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Plugins.Core;
using Plugins.MermaidMaker.Runtime.Core;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Plugins.MermaidMaker.Editor
{
    public class NameSpaceEditor : EditorWindow
    {
        private NameSpaceTreeView _treeView;
        private TreeViewState _treeViewState;
        private static NameSpaceNode _rootNode;
        private static string FolderPath => Path.Combine(Application.dataPath, "MermaidMaker");
        private OutputStyle _selectedStyle;
        private int _selectedFileIndex;
        private string _fileName;
        private int _beforeSelectedFileIndex;
        private static NameSpaceEditor _window;
        private static Assembly _assembly;

        public static void ShowWindow(NameSpaceNode rootNode,Assembly assembly)
        {
            _rootNode = rootNode;
            _window = GetWindow<NameSpaceEditor>();
            _window.titleContent = new GUIContent("MermaidMaker - NameSpace Selection");
            _assembly = assembly;
        }

        public static void CloseWindow()
        {
            if (_window != null) _window.Close();
            _window = null;
        }

        private void OnEnable()
        {
            _beforeSelectedFileIndex = -1;
            _treeViewState ??= new TreeViewState();

            var nameColumn = new MultiColumnHeaderState.Column
            {
                headerContent = new GUIContent("NameSpace"),
                headerTextAlignment = TextAlignment.Center,
                canSort = false,
                width = 300,
                minWidth = 300,
                autoResize = true,
                allowToggleVisibility = true
            };
            var selectBoxColumn = new MultiColumnHeaderState.Column
            {
                headerContent = new GUIContent("Apply or not"),
                headerTextAlignment = TextAlignment.Center,
                canSort = false,
                width = 100,
                minWidth = 100,
                autoResize = true,
                allowToggleVisibility = true
            };
            var headerState = new MultiColumnHeaderState(new[] { nameColumn, selectBoxColumn });
            var multiColumnHeader = new MultiColumnHeader(headerState);
            _treeView = new NameSpaceTreeView(_treeViewState, multiColumnHeader);
            _treeView.Setup(_rootNode);
        }

        private void OnGUI()
        {
            if (_selectedFileIndex != _beforeSelectedFileIndex)
            {
                _fileName = "";
                _beforeSelectedFileIndex = _selectedFileIndex;
            }

            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUILayout.Space(100);
                GUILayout.FlexibleSpace();
            }

            var rect = EditorGUILayout.GetControlRect(false, 200);
            _treeView.OnGUI(rect);

            _selectedStyle = (OutputStyle)EditorGUILayout.Popup(
                label: new GUIContent("Output Type"),
                selectedIndex: (int)_selectedStyle,
                displayedOptions: new[]
                {
                    new GUIContent("Debug Log"),
                    new GUIContent("Overwrite an existing file"),
                    new GUIContent("Generate new file")
                }
            );
            
            switch (_selectedStyle)
            {
                case OutputStyle.DebugLog: 
                    break;
                case OutputStyle.Overwrite:
                    var allFiles = FileManager.GetAllMarkdownFiles(FolderPath);
                    if (allFiles.Count == 0)
                    {
                        GUILayout.Label(
                            "Markdown file does not exist.\nThe file must be in Assets/MermaidMaker.\nIf you change the setting to 'Generate new file', folders and files will be automatically generated.",
                            EditorStyles.miniBoldLabel);
                        break;
                    }

                    var contents = allFiles.Select(fileName => new GUIContent(fileName.Split(".")[0])).ToArray();
                    _selectedFileIndex = EditorGUILayout.Popup(
                        label: new GUIContent("Markdown File"),
                        selectedIndex: _selectedFileIndex,
                        displayedOptions: contents
                    );
                    _fileName = allFiles[_selectedFileIndex];
                    break;
                case OutputStyle.GenerateNewFile:
                GUILayout.Label("If no file name is specified, it will be assigned in numerical order.",
                        EditorStyles.miniLabel);
                    _fileName = GUILayout.TextField(_fileName);
                    break;
            }

            var selectedNameSpaces = NameSpaceToApply(_rootNode, new List<string>()).Count != 0;
            if (!selectedNameSpaces)
            {
                EditorGUI.BeginDisabledGroup(true);
                GUILayout.Label(
                    "Please select NameSpaces",
                    EditorStyles.miniLabel);
            }
            
            if (GUILayout.Button("Apply", GUILayout.Width(100), GUILayout.Height(20)))
            {
                if (_selectedFileIndex == 0 && _fileName == "")
                {
                    _fileName = $"ClassDiagram{FileManager.GetNumberOfDefaultFiles(FolderPath)}";
                }

                var result = MermaidMakerUtility.CreateStringText(NameSpaceToApply(_treeView.RootNode, new List<string>()),
                    (int)_selectedStyle, _fileName,_assembly,FolderPath);
                if(_selectedStyle == 0)Debug.Log(result);
                CloseWindow();
                Debug.Log(
                    _selectedStyle == OutputStyle.DebugLog
                        ? "Output was successful! If the file is not found, please reload editor with ctrl+R (cmd+R)."
                        : "Output was successful!");
            }

            if (!selectedNameSpaces)
            {
                EditorGUI.EndDisabledGroup();
            }
        }

        private List<string> NameSpaceToApply(NameSpaceNode parentNode, List<string> result)
        {
            if (parentNode.Children.Count == 0) return result;
            foreach (var child in parentNode.Children)
            {
                if (child.WasSelecting) result.Add(child.FullName);
                result = NameSpaceToApply(child, result);
            }

            return result;
        }
    }
}