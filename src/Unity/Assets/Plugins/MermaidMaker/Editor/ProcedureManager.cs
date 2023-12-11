using System;
using System.Reflection;
using Plugins.MermaidMaker.Runtime.Core;
using UnityEditor;

namespace Plugins.MermaidMaker.Editor
{
    public static class ProcedureManager
    {
        private static Assembly _targetAssembly;
        
        [MenuItem("Window/MermaidMaker")]
        public static void Generate()
        {
            AssemblyEditor.ShowWindow();
            AssemblyEditor.ProcessCompleted += EnterToNameSpace;
        }

        private static void EnterToNameSpace(object sender,EventArgs e)
        {
            var assemblyName = "Assembly-CSharp";
            if (AssemblyEditor.AssemblySpecified)
            {
                assemblyName = AssemblyEditor.SelectedAssemblyDefinitionAsset.name;
            }
            
            _targetAssembly = Assembly.Load(assemblyName);
            var root = MermaidMakerUtility.GetNameSpaces(_targetAssembly);
            
            NameSpaceEditor.ShowWindow(root, _targetAssembly);
        }
    }
}