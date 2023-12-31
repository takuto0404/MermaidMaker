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
        private static readonly string NewLine = Environment.NewLine;

        private static List<string> _nameSpaces;

        private static readonly List<char> ValidCharacters = new()
        {
            'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U',
            'V', 'W', 'X', 'Y', 'Z',
            'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u',
            'v', 'w', 'x', 'y', 'z',
            '^', '`', '[', ']', '_',
            '1', '2', '3', '4', '5', '6', '7', '8', '9', '0'
        };

        private static List<string> _typeNames;

        private static List<Type> GetValidTypes(Assembly assembly)
        {
            return assembly.GetTypes()
                .Where(type =>
                    !type.GetInterfaces()
                        .Select(item => item.Namespace)
                        .Select(nameSpaceFullName => Join(".", nameSpaceFullName!.Split(".").Take(2)))
                        .ToList().Contains("System.Runtime"))
                .Where(type => !IsSpecial(type.Name))
                .ToList();
        }

        public static NameSpaceNode MakeNameSpaceNodes(Assembly assembly)
        {
            var id = 0;

            var nameSpaceFullNames = GetValidTypes(assembly)
                .Select(type => type.Namespace!);

            var nameSpaceDictionary = nameSpaceFullNames
                .ToDictionary(name => name.Split("."), name => name);

            var splitNames = nameSpaceDictionary.Keys.ToList();

            var root = new NameSpaceNode(0, "Root", "");

            var nameSpaceNodes = new List<NameSpaceNode>();

            if (splitNames.Count == 0) return root;
            foreach (var nameSpaceName in splitNames)
            {
                for (var i = 0; i < nameSpaceName.Length; i++)
                {
                    var addedNameSpaceNames = nameSpaceNodes.Select(node => node.NameSpaceName).ToList();
                    if (addedNameSpaceNames.Contains(nameSpaceName[i])) continue;
                    
                    NameSpaceNode parentNode;
                    if (i == 0)
                    {
                        parentNode = root;
                    }
                    else
                    {
                        var parentName = nameSpaceName[i - 1];
                        parentNode =
                            nameSpaceNodes.First(node => node.NameSpaceName == parentName);
                    }

                    var newNode = new NameSpaceNode(id, nameSpaceName[i], nameSpaceDictionary[nameSpaceName]);
                    parentNode.AddChild(newNode);
                    nameSpaceNodes.Add(newNode);
                    id++;
                }
            }

            foreach (var node in nameSpaceNodes)
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
            var arrowTexts = new List<string>();

            var memberInfos = GetValidTypes(assembly);
            _typeNames = memberInfos.Select(type => RemoveGenericPartFromTypeName(type.Name)).ToList();
            foreach (var type in memberInfos)
            {
                if (type.IsEnum)
                {
                    fileText += $"{NewLine}    class {type.Name}{{{NewLine}    <<enum>>{NewLine}   }}{NewLine}";
                    continue;
                }

                var typeWords = type.Name.Split("`");

                var baseTypeWords = type.BaseType?.Name.Split("`");
                if (baseTypeWords != null && type.BaseType != null && memberInfos.Contains(type.BaseType))
                    arrowTexts.Add($"{typeWords[0]} --|> {baseTypeWords[0]}{NewLine}");

                foreach (var arrow in type.GetInterfaces()
                             .Where(interfaceType => memberInfos.Contains(interfaceType))
                             .Select(item => $"{typeWords[0]} ..|> {item.Name}{NewLine}"))
                {
                    arrowTexts.Add(arrow);
                }


                fileText += $"{NewLine}    class {RemoveGenericPartFromTypeName(type.Name)}";
                fileText += "{";
                if (type.IsInterface) fileText += $"{NewLine}    <<interface>>";
                fileText += $"{NewLine}";

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

                    fileText += $"{field.Name}{NewLine}";

                    var arrowResult = GetIntensiveRelationShip(type, field.FieldType, true);
                    if (IsNullOrWhiteSpace(arrowResult)) continue;
                    arrowTexts.Add($"{arrowResult}");
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

                    fileText += $"{method.Name}({Join(",", parameters)}){NewLine}";
                }

                fileText += @"   }";
                fileText += $"{NewLine}";
            }

            arrowTexts = arrowTexts.Distinct().ToList();

            fileText += Concat(arrowTexts);

            var markdown = $"```mermaid{NewLine}    classDiagram{NewLine}{fileText}```";
            if (selectedIndex != 0) CreateDiagramFile(markdown, fileName, path);
            return markdown;
        }

        private static bool IsSpecial(string text)
        {
            var isContinue = false;
            foreach (var c in text)
            {
                if (!ValidCharacters.Contains(c)) isContinue = true;
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

        private static string RemoveGenericPartFromTypeName(string name)
        {
            return name.Split("`")[0];
        }

        private static IEnumerable<Type> GetGenerics(List<Type> types)
        {
            var list = new List<Type>();
            //二次元配列
            foreach (var type in types)
            {
                if (_typeNames.Contains(RemoveGenericPartFromTypeName(type.Name)))
                {
                    list.Add(type);
                }

                var genericTypes = type.GetGenericArguments();
                var result = GetGenerics(genericTypes.ToList());
                list.AddRange(result);
            }

            return list;
        }

        private static string GetIntensiveRelationShip(Type classType, Type type, bool isFirst)
        {
            var baseInterfaces = type.GetInterfaces();
            if (baseInterfaces.Contains(typeof(System.Collections.IEnumerable)))
            {
                var text = "";
                if (type.HasElementType)
                {
                    var elementType = type.GetElementType()!;
                    if (!_typeNames.Contains(RemoveGenericPartFromTypeName(elementType.Name))) return "";
                    text =
                        $"{RemoveGenericPartFromTypeName(elementType.Name)} --o {RemoveGenericPartFromTypeName(classType.Name)}{NewLine}";
                }

                var arguments = type.GetGenericArguments().ToList();
                var generics = GetGenerics(arguments);
                var text2 = Concat(generics.Select(genericType =>
                    $"{RemoveGenericPartFromTypeName(genericType.Name)} --o {RemoveGenericPartFromTypeName(classType.Name)}{NewLine}"));
                return Concat(text, text2);
            }

            var baseType = type.BaseType;
            if (baseType == null) return "";

            var result = GetIntensiveRelationShip(classType, baseType, false);
            if (result == "") return "";

            if (!isFirst) return "";
            return
                $"{RemoveGenericPartFromTypeName(type.GetElementType()!.Name)} --o {RemoveGenericPartFromTypeName(classType.Name)}";
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

                var typeOriginalName = RemoveGenericPartFromTypeName(fieldType.Name);
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