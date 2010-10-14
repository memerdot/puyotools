using System;
using System.Windows.Forms;
using System.ComponentModel;

namespace PuyoTools
{
    public abstract class ItemIterator
    {
        public abstract bool isItem();
        public abstract bool isItemArray();
        public abstract ToolStripItem getItem();
        public abstract string getText();
        public abstract ItemIterator[] getItemArray();
        public abstract object buildToolStripItemArray();
        public abstract object buildToolStripItemArray(int level);
    }
    public class Item : ItemIterator
    {
        private ToolStripItem i;
        private string t;
        public Item(string text, EventHandler handler)
        {
            i = new ToolStripMenuItem(text, null, handler);
            t = text;
        }
        public Item(ToolStripItem item)
        {
            i = item;
            //t=item.text;
        }
        public override bool isItem()
        {
            return true;
        }
        public override bool isItemArray()
        {
            return false;
        }
        public override ToolStripItem getItem()
        {
            return i;
        }
        public override string getText()
        {
            return t;
        }
        public override ItemIterator[] getItemArray()
        {
            throw new System.ApplicationException("Item asked for ItemArray");
        }
        public override object buildToolStripItemArray()
        {
            return buildToolStripItemArray(0);
        }
        public override object buildToolStripItemArray(int level)
        {
            throw new System.ApplicationException("Item asked to build ToolStripItem array");
        }
    }
    public class ItemArray : ItemIterator
    {
        private ItemIterator[] i;
        string t;
        public ItemArray(string text, ItemIterator[] items)
        {
            i = items;
            t = text;
        }
        public override bool isItem()
        {
            return false;
        }
        public override bool isItemArray()
        {
            return true;
        }
        public override ToolStripItem getItem()
        {
            throw new System.ApplicationException("ItemArray asked for Item");
        }
        public override string getText()
        {
            return t;
        }
        public override ItemIterator[] getItemArray()
        {
            return i;
        }
        public override object buildToolStripItemArray()
        {
            return buildToolStripItemArray(0);
        }
        public override object buildToolStripItemArray(int level)
        {
            switch (level)
            {
                case 0:
                    ToolStripItem[] Array = new ToolStripItem[i.GetLength(0)];
                    int x = 0;
                    foreach (ItemIterator item in i)
                    {
                        if (item.isItem())
                        {
                            Array[x] = item.getItem();
                        }
                        if (item.isItemArray())
                        {
                            Array[x] = (ToolStripItem)item.buildToolStripItemArray(level + 1);
                        }
                        x++;
                    }
                    return Array;
                    // Yeah, it's unreachable. But Mono Compiler makes a stupid:
                    // when it's not there, it assumes that case labels bleed, which isn't allowed.
                    // --
                    // Someone in the Mono Project needs to update the switch/case system to use the
                    // new method of detecting reachable code before these breaks can be removed.
                    break;
                case 1:
                    ToolStripDropDownButton Button = new ToolStripDropDownButton(t);
                    foreach (ItemIterator item in i)
                    {
                        if (item.isItem())
                        {
                            Button.DropDownItems.Add(item.getItem());
                        }
                        if (item.isItemArray())
                        {
                            Button.DropDownItems.Add((ToolStripMenuItem)item.buildToolStripItemArray(level + 1));
                        }
                    }
                    return Button;
                    break;
                default:
                    ToolStripMenuItem MenuItem = new ToolStripMenuItem(t);
                    foreach (ItemIterator item in i)
                    {
                        if (item.isItem())
                        {
                            MenuItem.DropDownItems.Add(item.getItem());
                        }
                        if (item.isItemArray())
                        {
                            MenuItem.DropDownItems.Add((ToolStripMenuItem)item.buildToolStripItemArray(level + 1));
                        }
                    }
                    return MenuItem;
                    break;
            }
        }
    }
}