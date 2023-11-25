using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Cysharp.Text;
using static System.String;

namespace Plugins.Runtime
{
    public static class MermaidMakerUtility
    {
        private static readonly string BR = Environment.NewLine;

        private static List<string> _nameSpaces;

        public static NameSpaceNode GetNameSpaces(Assembly assembly)
        {
            var id = 0;

            var nameSpaces = assembly.GetTypes()
                .Select(type => type.Namespace);

            var nameSpaceDic = nameSpaces
                .Where(n => n != null)
                .ToDictionary(name => name.Split("."), name => name);

            var nameSpaceNames = nameSpaceDic.Keys.ToList();

            var root = new NameSpaceNode(0, "Root", "");
            var allNameSpaceNodes = new List<NameSpaceNode>();

            if (nameSpaceNames.Count != 0)
            {
                foreach (var nameSpaceName in nameSpaceNames)
                {
                    for (var i = 0; i < nameSpaceName.Length; i++)
                    {
                        var allNameSpaceNames = allNameSpaceNodes.Select(node => node.NameSpaceName).ToList();
                        if (!allNameSpaceNames.Contains(nameSpaceName[i]))
                        {
                            NameSpaceNode parentNode;
                            if (i == 0)
                            {
                                parentNode = root;
                            }
                            else
                            {
                                var parentName = nameSpaceName[i - 1];
                                parentNode =
                                    allNameSpaceNodes.First(node => node.NameSpaceName == parentName);
                            }

                            var newNode = new NameSpaceNode(id, nameSpaceName[i], nameSpaceDic[nameSpaceName]);
                            parentNode.AddChild(newNode);
                            allNameSpaceNodes.Add(newNode);
                            id++;
                        }
                    }
                }
            }

            foreach (var node in allNameSpaceNodes)
            {
                node.Children.Sort((a, b) => CompareOrdinal(a.NameSpaceName, b.NameSpaceName));
            }

            return root;
        }

        public static void CreateStringText(List<string> nameSpaces, int selectedIndex, string fileName,
            Assembly assembly)
        {
            _nameSpaces = nameSpaces;
            var fileText = "";
            var arrowText = "";

            var types = assembly
                .GetTypes()
                .Where(type => _nameSpaces.Contains(type.Namespace));

            var memberInfos = types.ToList();
            foreach (var type in memberInfos)
            {
                var interfaces = type.GetInterfaces().Select(item => item.Namespace)
                    .Select(name => string.Join(".", name!.Split(".").Take(2))).ToList();
                if (interfaces.Contains("System.Runtime")) continue;
                if (type.Name.Contains("<>")) continue;

                Debug.Log(type.Name);

                if (type.IsEnum)
                {
                    fileText += $"{BR}    class {type.Name}{{{BR}    <<enum>>{BR}   }}{BR}";
                    continue;
                }

                var words = type.BaseType?.Name.Split("`");
                if (words != null && type.BaseType != null)
                    arrowText += $"{type.Name} --|> {words[0]}{BR}";


                arrowText = type.GetInterfaces().Where(interfaceType => memberInfos.Contains(interfaceType))
                    .Aggregate(arrowText, (current, item) => current + $"{type.Name} ..|> {item.Name}{BR}");

                fileText += $"{BR}    class {type.Name.Split("`")[0]}";
                fileText += "{";
                if (type.IsInterface) fileText += $"{BR}    <<interface>>";
                fileText += $"{BR}";

                var fields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                    .Where(field => field.DeclaringType == type);

                foreach (var field in fields)
                {
                    fileText += "       ";
                    var attributeText = GetFieldAttributeText(field);
                    if (attributeText == "") continue;
                    fileText += attributeText;

                    fileText += GetSpecialAttributeText(field);
                    var typeText = GetTypeText(field.FieldType);
                    fileText += $"{typeText} ";

                    fileText += $"{field.Name}{BR}";
                }


                var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
                    .Where(method => method.DeclaringType == type);
                foreach (var method in methods)
                {
                    fileText += "       ";
                    var attributeText = GetMethodAttributeText(method);
                    if (attributeText == "") continue;
                    fileText += attributeText;

                    var typeText = GetTypeText(method.ReturnType);
                    fileText += $"{typeText} ";

                    var parameters = method.GetParameters().Select(parameter => $"{GetTypeText(parameter.ParameterType)} {parameter.Name}").ToArray();
                    
                    fileText += $"{method.Name}({ZString.Join(",",parameters)}){BR}";
                }

                fileText += @"   }";
                fileText += $"{BR}";
            }

            fileText += arrowText;

            CreateDiagramFile($"```mermaid{BR}    classDiagram{BR}{fileText}```", selectedIndex, fileName);
        }

        private static string GetSpecialAttributeText(FieldInfo field)
        {
            if (field.IsLiteral) return "const ";
            var text = "";
            if (field.IsStatic) text += "static ";
            if (field.IsInitOnly) text += "readonly ";
            return text;
        }

        private static void CreateDiagramFile(string text, int selectedIndex, string fileName)
        {
            if (selectedIndex == 0)
            {
                Debug.Log(text);
            }
            else
            {
                FileManager.WriteFile(fileName, text);
            }
        }

        private static string GetFieldAttributeText(FieldInfo fieldInfo)
        {
            if (fieldInfo.IsPrivate) return "-";
            if (fieldInfo.IsFamily) return "#";
            if (fieldInfo.IsPublic) return "+";
            Debug.Log($"{fieldInfo.Name}の属性が見つかりません。");
            throw new Exception("Attribute not found");
        }

        private static string GetMethodAttributeText(MethodBase methodInfo)
        {
            if (methodInfo.IsPrivate) return "-";
            if (methodInfo.IsFamily) return "#";
            if (methodInfo.IsPublic) return "+";
            Debug.Log($"{methodInfo.Name}の属性が見つかりません。");
            throw new Exception("Attribute not found");
        }

        private static string GetTypeText(Type fieldType)
        {
            var words = fieldType.Name.Split("`");
            if (words.Length == 1)
            {
                var words2 = words[0].Split("[");

                var word = words2[0];
                var text = word switch
                {
                    "Boolean" => "bool",
                    "Byte" => "byte",
                    "Char" => "char",
                    "Decimal" => "decimal",
                    "Double" => "double",
                    "Int16" => "short",
                    "Int32" => "int",
                    "Int64" => "long",
                    "SByte" => "sbyte",
                    "Single" => "float",
                    "String" => "string",
                    "Void" => "void",
                    "UInt16" => "ushort",
                    "UInt32" => "uint",
                    "UInt64" => "ulong",
                    _ => fieldType.Name
                };
                if (words2.Length == 2)
                {
                    return $"{text}[{words2[1]}";
                }

                return text;
            }

            if (words.Length >= 2)
            {   
                var arguments = fieldType.GenericTypeArguments;

                var genericTypeList = arguments.Select(GetTypeText).ToList();
                string genericText = "";

                if(words[0] == "ValueTuple")
                {
                    genericText += $"({genericTypeList[0]}";
                }
                else
                {
                    genericText += $"{words[0]}<{genericTypeList[0]}";
                }
                genericTypeList.RemoveAt(0);
                genericTypeList.ForEach(text => { genericText += $",{text}"; });

                if(words[0] == "ValueTuple")
                {
                    genericText += ")";
                }
                else
                {
                    genericText += ">";
                }
                return genericText;
            }

            return "";
        }
    }
}