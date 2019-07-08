using System;
namespace Tricycle.Models
{
    public class ItemSelectedEventArgs : EventArgs
    {
        public ListItem OldItem { get; }
        public ListItem NewItem { get; }

        public ItemSelectedEventArgs(ListItem oldItem, ListItem newItem)
        {
            OldItem = oldItem;
            NewItem = newItem;
        }
    }
}
