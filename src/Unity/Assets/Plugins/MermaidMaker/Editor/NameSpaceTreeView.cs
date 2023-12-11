using System.Collections.Generic;
using Plugins.Core;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Plugins.MermaidMaker.Editor
{
    public class NameSpaceTreeView : TreeView
    {
        public NameSpaceNode RootNode;

        public NameSpaceTreeView(TreeViewState treeViewState, MultiColumnHeader multiColumnHeader) : base(
            treeViewState, multiColumnHeader)
        {
            showBorder = true;
        }

        public void Setup(NameSpaceNode rootNode)
        {
            RootNode = rootNode;
            Reload();
        }

        protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
        {
            var rows = GetRows() ?? new List<TreeViewItem>();
            rows.Clear();
            
            if (RootNode != null)
            {
                NodeToTreeView(RootNode.Children, root, rows);
            }

            SetupDepthsFromParentsAndChildren(root);
            return rows;
        }

        private void NodeToTreeView(List<NameSpaceNode> childNodes, TreeViewItem parent, IList<TreeViewItem> rows)
        {
            foreach (var childNode in childNodes)
            {
                var childItem = new NameSpaceNodeTreeViewItem
                {
                    id = childNode.Id,
                    displayName = childNode.NameSpaceName,
                    Data = childNode
                };
                parent.AddChild(childItem);
                rows.Add(childItem);
                if (childNode.Children.Count >= 1)
                {
                    if (IsExpanded(childNode.Id))
                    {
                        NodeToTreeView(childNode.Children, childItem, rows);
                    }
                    else
                    {
                        childItem.children = CreateChildListForCollapsedParent();
                    }
                }
            }
        }

        protected override TreeViewItem BuildRoot()
        {
            var root = new TreeViewItem { id = 0, depth = -1 };
            return root;
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            var item = (NameSpaceNodeTreeViewItem)args.item;

            for (var i = 0; i < args.GetNumVisibleColumns(); ++i)
            {
                var cellRect = args.GetCellRect(i);
                var columnIndex = args.GetColumn(i);

                if (columnIndex == 0)
                {
                    base.RowGUI(args);
                }
                else if (columnIndex == 1)
                {
                    var beforeValue = item.Data.WasSelecting;
                    item.Data.WasSelecting = GUI.Toggle(cellRect, item.Data.WasSelecting , GUIContent.none);
                    var afterValue = item.Data.WasSelecting;
                    if (beforeValue != afterValue)
                    {
                        foreach (var child in item.Data.Children)
                        {
                            child.WasSelecting = item.Data.WasSelecting;
                        }
                    }
                }
            }
        }

        private class NameSpaceNodeTreeViewItem : TreeViewItem
        {
            public NameSpaceNode Data;
        }
    }
}