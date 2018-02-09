using System.Windows.Forms;
using theawesomecoder61.Helpers;

namespace XCI_Tool
{
    public class TreeViewFileSystem
    {
        public TreeView treeView;

        public TreeViewFileSystem(TreeView tv)
        {
        }

        public BetterTreeNode AddDir(string name, BetterTreeNode parent = null, string unknown = "")
        {
            BetterTreeNode btn = new BetterTreeNode(name);
            btn.Offset = -1;
            btn.Size = -1;
            parent.Nodes.Add(btn);
            return btn;
        }

        public BetterTreeNode AddFile(string name, BetterTreeNode parent, long offset, long size)
        {
            BetterTreeNode btn = new BetterTreeNode(name);
            btn.Offset = offset;
            btn.Size = size;
            parent.Nodes.Add(btn);
            return btn;
        }
    }
}