using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Plugins.Core;
using Plugins.MermaidMaker.Runtime.Core;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Plugins.MermaidMaker.Editor
{
    public class NameSpaceEditor : EditorWindow
    {
        private int _beforeSelectedFileIndex;
        private TreeViewState _treeViewState;
        private static NameSpaceNode _rootNode;
        private int _selectedFileIndex;
        private static NameSpaceEditor _window;
        public static string FileName { get; private set; }
        public static string FolderPath => Path.Combine(Application.dataPath, "MermaidMaker");
        public static event EventHandler ProcessCompleted;
        public static OutputStyle SelectedStyle { get; private set; }
        public static NameSpaceTreeView TreeView { get; private set; }

        public static void ShowWindow(NameSpaceNode rootNode)
        {
            _rootNode = rootNode;
            _window = GetWindow<NameSpaceEditor>();
            _window.titleContent = new GUIContent("MermaidMaker - NameSpace Selection");
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
            TreeView = new NameSpaceTreeView(_treeViewState, multiColumnHeader);
            TreeView.Setup(_rootNode);
        }

        private void OnGUI()
        {
            if (_selectedFileIndex != _beforeSelectedFileIndex)
            {
                FileName = "";
                _beforeSelectedFileIndex = _selectedFileIndex;
            }

            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUILayout.Space(100);
                GUILayout.FlexibleSpace();
            }

            var rect = EditorGUILayout.GetControlRect(false, 200);
            TreeView.OnGUI(rect);

            SelectedStyle = (OutputStyle)EditorGUILayout.Popup(
                label: new GUIContent("Output Type"),
                selectedIndex: (int)SelectedStyle,
                displayedOptions: new[]
                {
                    new GUIContent("Debug Log"),
                    new GUIContent("Overwrite an existing file"),
                    new GUIContent("Generate new file")
                }
            );
            
            switch (SelectedStyle)
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
                    FileName = allFiles[_selectedFileIndex];
                    break;
                case OutputStyle.GenerateNewFile:
                GUILayout.Label("If no file name is specified, it will be assigned in numerical order.",
                        EditorStyles.miniLabel);
                    FileName = GUILayout.TextField(FileName);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var isNoNameSpaceSelected = NameSpaceToApply(_rootNode, new List<string>()).Count == 0;
            if (isNoNameSpaceSelected)
            {
                EditorGUI.BeginDisabledGroup(true);
                GUILayout.Label(
                    "Please select NameSpaces",
                    EditorStyles.miniLabel);
            }
            
            if (GUILayout.Button("Apply", GUILayout.Width(100), GUILayout.Height(20)))
            {
                if (_selectedFileIndex == 0 && FileName == "")
                {
                    FileName = $"ClassDiagram{FileManager.GetNumberOfDefaultFiles(FolderPath)}";
                }
                
                CloseWindow();
                OnProcessCompleted();
            }

            if (isNoNameSpaceSelected)
            {
                EditorGUI.EndDisabledGroup();
            }
        }

        public static List<string> NameSpaceToApply(NameSpaceNode parentNode, List<string> result)
        {
            if (parentNode.Children.Count == 0) return result;
            foreach (var child in parentNode.Children)
            {
                if (child.WasSelecting) result.Add(child.FullName);
                result = NameSpaceToApply(child, result);
            }

            return result;
        }

        private static void OnProcessCompleted()
        {
            ProcessCompleted?.Invoke(null, EventArgs.Empty);
        }
    }
}