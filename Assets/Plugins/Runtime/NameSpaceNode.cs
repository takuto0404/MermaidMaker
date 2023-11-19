using System.Collections.Generic;

namespace Plugins.Runtime
{
    public class NameSpaceNode
    {
        public NameSpaceNode(int id,string nameSpaceName,string fullName)
        {
            Id = id;
            NameSpaceName = nameSpaceName;
            Children = new List<NameSpaceNode>();
            FullName = fullName;
            WasSelecting = true;
        }

        public readonly int Id;
        public readonly string NameSpaceName;
        public readonly List<NameSpaceNode> Children;
        public readonly string FullName;

        public bool WasSelecting;

        public void AddChild(NameSpaceNode child)
        {
            Children.Add(child);
        }
    }
}