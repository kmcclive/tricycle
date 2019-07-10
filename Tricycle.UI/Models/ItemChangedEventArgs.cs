using System;

namespace Tricycle.UI.Models
{
    public class ItemChangedEventArgs : EventArgs
    {
        public ListItem OldItem { get; }
        public ListItem NewItem { get; }

        public ItemChangedEventArgs(ListItem oldItem, ListItem newItem)
        {
            OldItem = oldItem;
            NewItem = newItem;
        }
    }
}
