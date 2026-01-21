/// This class adds our menu item to the Entry Context Menu shows on the main screen

using KeePass.Plugins;
using System;
using System.Windows.Forms;

namespace PickCharsPlus
{
    internal static class ContextMenuHelper
    {
        public static ToolStripMenuItem SetupEntryContextMenu(IPluginHost host, string label, EventHandler onClick, Action<string> log)
        {
            try
            {
                log?.Invoke($"ContextMenuHelper: start");
                if (host?.MainWindow != null && host.MainWindow.EntryContextMenu != null)
                {
                    var contextMenu = host.MainWindow.EntryContextMenu;
                    var pluginMenuItem = new ToolStripMenuItem(label);
                    pluginMenuItem.Click += onClick;
                    InsertItemAboveSeparator(contextMenu.Items, pluginMenuItem, log);
                    log?.Invoke($"ContextMenuHelper: added pluginMenuItem to EntryContextMenu");
                    return pluginMenuItem;
                }
            }
            catch (Exception ex)
            {
                log?.Invoke($"ContextMenuHelper: exception {ex}");
            }

            return null;
        }

        private static bool InsertItemAboveSeparator(ToolStripItemCollection menuItems, ToolStripMenuItem item, Action<string> log)
        {
            bool inserted = false;

            for (int i = 0; i < menuItems.Count; i++)
            {
                if (menuItems[i] is ToolStripSeparator)
                {
                    menuItems.Insert(i, item);
                    inserted = true;
                    break;
                }
            }

            log?.Invoke($"ContextMenuHelper.InsertItemAboveSeparator: menuItems.Count={menuItems.Count}, inserted={inserted}");
            return inserted;
        }
    }
}
