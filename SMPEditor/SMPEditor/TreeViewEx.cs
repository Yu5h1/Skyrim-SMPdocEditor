using System.Windows.Controls;

namespace SMPEditor
{
    public static class TreeViewEx
    {
        public static TreeViewItem GetTreeItem(this ItemsControl itemsControl,int index)
        {
            if (itemsControl.Items.Count == 0) return null;
            return itemsControl.Items[index] as TreeViewItem;
        }
    }
}
