using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Plugins.Core;
using static System.String;

namespace Plugins.MermaidMaker.Runtime.Core
{
    public static class MermaidMakerUtility
    {
        private static readonly string BR = Environment.NewLine;

        private static List<string> _nameSpaces;

        private static readonly List<char> NormalChars = new()
        {
            'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U',
            'V', 'W', 'X', 'Y', 'Z',
            'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u',
            'v', 'w', 'x', 'y', 'z',
            '^', '`', '[', ']', '_',
            '1', '2', '3', '4', '5', '6', '7', '8', '9', '0'
        };

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

        public static string CreateStringText(List<string> nameSpaces, int selectedIndex, string fileName,
            Assembly assembly, string path)
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
                    .Select(name => Join(".", name!.Split(".").Take(2))).ToList();
                if (interfaces.Contains("System.Runtime")) continue;

                if (IsSpecial(type.Name)) continue;

                if (type.IsEnum)
                {
                    fileText += $"{BR}    class {type.Name}{{{BR}    <<enum>>{BR}   }}{BR}";
                    continue;
                }

                var typeWords = type.Name.Split("`");
                
                var baseTypeWords = type.BaseType?.Name.Split("`");
                if (baseTypeWords != null && type.BaseType != null && memberInfos.Contains(type.BaseType))
                    arrowText += $"{typeWords[0]} --|> {baseTypeWords[0]}{BR}";
                
                arrowText += type.GetInterfaces()
                    .Where(interfaceType => memberInfos.Contains(interfaceType))
                    .Aggregate(arrowText, (current, item) => current + $"{typeWords[0]} ..|> {item.Name}{BR}");

                fileText += $"{BR}    class {type.Name.Split("`")[0]}";
                fileText += "{";
                if (type.IsInterface) fileText += $"{BR}    <<interface>>";
                fileText += $"{BR}";

                var fields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance |
                                            BindingFlags.Static)
                    .Where(field => field.DeclaringType == type);

                foreach (var field in fields)
                {
                    if (IsSpecial(field.Name)) continue;
                    var typeText = GetTypeText(field.FieldType);
                    if (typeText == "") continue;

                    fileText += "       ";
                    var attributeText = GetFieldAttributeText(field);
                    if (attributeText == "") continue;
                    fileText += attributeText;

                    fileText += GetSpecialAttributeText(field);
                    fileText += $"{typeText} ";

                    fileText += $"{field.Name}{BR}";

                    var arrowResult = GetIntensiveRelationShip(type, field.FieldType, true);
                    if (arrowResult != "")
                    {
                        arrowText += $"{arrowResult}{BR}";
                    };
                }


                var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance |
                                              BindingFlags.Static)
                    .Where(method => method.DeclaringType == type);
                foreach (var method in methods)
                {
                    if (IsSpecial(method.Name)) continue;
                    var parameters = method.GetParameters()
                        .Select(parameter =>
                        {
                            var parameterTypeText = GetTypeText(parameter.ParameterType);
                            return parameterTypeText == "" ? "" : $"{parameterTypeText} {parameter.Name}";
                        }).ToArray();
                    if (parameters.Contains("")) continue;

                    fileText += "       ";
                    var attributeText = GetMethodAttributeText(method);
                    if (attributeText == "") continue;
                    fileText += attributeText;

                    var typeText = GetTypeText(method.ReturnType);
                    if (typeText == "") continue;
                    fileText += $"{typeText} ";

                    fileText += $"{method.Name}({Join(",", parameters)}){BR}";
                }

                fileText += @"   }";
                fileText += $"{BR}";
            }

            fileText += arrowText;

            var markdown = $"```mermaid{BR}    classDiagram{BR}{fileText}```";
            if (selectedIndex != 0) CreateDiagramFile(markdown, fileName, path);
            return markdown;
        }

        private static bool IsSpecial(string text)
        {
            var isContinue = false;
            foreach (var c in text)
            {
                if (!NormalChars.Contains(c)) isContinue = true;
            }

            return isContinue;
        }

        private static string GetSpecialAttributeText(FieldInfo field)
        {
            if (field.IsLiteral) return "const ";
            var text = "";
            if (field.IsStatic) text += "static ";
            if (field.IsInitOnly) text += "readonly ";
            return text;
        }

        private static void CreateDiagramFile(string text, string fileName, string path)
        {
            FileManager.WriteFile(fileName, text, path);
        }

        private static string GetFieldAttributeText(FieldInfo fieldInfo)
        {
            if (fieldInfo.IsPrivate) return "-";
            if (fieldInfo.IsFamily) return "#";
            if (fieldInfo.IsPublic) return "+";
            throw new Exception("Attribute not found");
        }

        private static string GetMethodAttributeText(MethodBase methodInfo)
        {
            if (methodInfo.IsPrivate) return "-";
            if (methodInfo.IsFamily) return "#";
            if (methodInfo.IsPublic) return "+";
            throw new Exception("Attribute not found");
        }

        private static string GetIntensiveRelationShip(Type classType,Type fieldType,bool isFirst)
        {
            var baseInterfaces = fieldType.GetInterfaces();
            var baseType = fieldType.BaseType;
            if (baseInterfaces.Contains(typeof(System.Collections.IEnumerable)))
            {
                return $"{fieldType.GetElementType()} --o {classType}";
            }
            else
            {
                if (baseType == null) return "";
                
                var result = GetIntensiveRelationShip(classType, baseType,false);
                if (result == "")
                {
                    return "";
                }
                else
                {
                    if (isFirst) return "";
                    return $"{fieldType.GetElementType()!.Name.Split("`")[0]} --o {fieldType.Name.Split("`")[0]}";
                }
            }
        }

        private static string GetTypeText(Type fieldType)
        {
            if (fieldType.HasElementType)
            {
                if (IsSpecial(fieldType.Name)) return "";

                var elementType = fieldType.GetElementType();
                var elementWords = fieldType.Name.Split("[");
                var typeText = GetTypeText(elementType);
                return typeText == "" ? "" : $"{typeText}[{elementWords[1]}";
            }

            if (fieldType.IsGenericType)
            {
                if (IsSpecial(fieldType.Name)) return "";

                var genericTypes = fieldType.GenericTypeArguments;
                var genericTypeTexts = genericTypes.Select(GetTypeText).ToArray();
                if (genericTypeTexts.Contains("")) return "";

                var typeOriginalName = fieldType.Name.Split("`")[0];
                var parenthesis = typeOriginalName == "ValueTuple"
                    ? new[] { "(", ")" }
                    : new[] { $"{typeOriginalName}<", ">" };

                var text = $"{parenthesis[0]}{genericTypeTexts[0]}";
                for (var i = 1; i < genericTypeTexts.Length; i++)
                {
                    text += $",{genericTypeTexts[i]}";
                }

                text += $"{parenthesis[1]}";
                return text;
            }

            var fieldTypeName = fieldType.Name;
            return fieldTypeName switch
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
        }
    }
}