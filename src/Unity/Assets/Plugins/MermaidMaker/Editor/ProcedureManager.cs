using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Plugins.MermaidMaker.Runtime.Core;
using UnityEditor;
using UnityEngine;

namespace Plugins.MermaidMaker.Editor
{
    public static class ProcedureManager
    {
        private static Assembly _targetAssembly;

        [MenuItem("Window/MermaidMaker")]
        public static void Generate()
        {
            AssemblyEditor.ShowWindow();
            AssemblyEditor.ProcessCompleted += FinishAssemblyProcess;
            NameSpaceEditor.ProcessCompleted += FinishNameSpaceProcess;
        }

        private static void FinishAssemblyProcess(object sender, EventArgs e)
        {
            var assemblyName = "Assembly-CSharp";
            if (AssemblyEditor.AssemblySpecified)
            {
                assemblyName = AssemblyEditor.SelectedAssemblyDefinitionAsset.name;
            }

            _targetAssembly = Assembly.Load(assemblyName);
            var root = MermaidMakerUtility.GetNameSpaces(_targetAssembly);

            NameSpaceEditor.ShowWindow(root);
        }

        private static void FinishNameSpaceProcess(object sender, EventArgs e)
        {
            var selectedStyle = NameSpaceEditor.SelectedStyle;
            var result = MermaidMakerUtility.CreateStringText(
                NameSpaceEditor.NameSpaceToApply(NameSpaceEditor.TreeView.RootNode, new List<string>()),
                (int)selectedStyle, NameSpaceEditor.FileName, _targetAssembly, NameSpaceEditor.FolderPath);
            if (selectedStyle == OutputStyle.DebugLog) Debug.Log(result);
            Debug.Log(
                selectedStyle != OutputStyle.DebugLog
                    ? "Output was successful! If the file is not found, please reload editor with ctrl+R (cmd+R)."
                    : "Output was successful!");
        }
    }
}