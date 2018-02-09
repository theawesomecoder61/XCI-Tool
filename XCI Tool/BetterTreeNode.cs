using System.Windows.Forms;

namespace theawesomecoder61.Helpers
{
    public class BetterTreeNode : TreeNode
    {
        public long Offset;
        public long Size;

        public BetterTreeNode(string t)
        {
            Text = t;
        }
    }
}