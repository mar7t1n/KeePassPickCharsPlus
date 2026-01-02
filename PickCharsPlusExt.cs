namespace PickCharsPlus
{

    using KeePass.Plugins;
    using KeePassLib;
    using System;
    using System.Windows.Forms;


    public sealed class PickCharsPlusExt : KeePass.Plugins.Plugin
    {
        private IPluginHost _host;
        private ToolStripMenuItem pluginMenuItem;

        public override bool Initialize(IPluginHost host)
        {
            _host = host ?? throw new ArgumentNullException(nameof(host));
            try
            {
                return setUpMenu();
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool setUpMenu()
        {
            try
            {
                // Add "MARTIN MENU" to the entry context menu (right-click on an entry)
                if (_host.MainWindow != null && _host.MainWindow.EntryContextMenu != null)
                {
                    var contextMenu = _host.MainWindow.EntryContextMenu;
                    pluginMenuItem = new ToolStripMenuItem("Pick Chars Plus");
                    pluginMenuItem.Click += MenuItem_Click;
                    InsertItemAboveSeparator(contextMenu.Items, pluginMenuItem);
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool InsertItemAboveSeparator(ToolStripItemCollection menuItems, ToolStripMenuItem item)
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

            return inserted;
        }

        private void MenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                PwEntry[] entries = _host.MainWindow.GetSelectedEntries();
                if (entries == null || entries.Length == 0)
                {
                    System.Windows.Forms.MessageBox.Show("No entry selected", "Please select a single entry first.");
                    return;
                }

                using (var dlg = new PickCharsForm(entries[0]))
                {
                    dlg.ShowDialog(_host.MainWindow);
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("PickCharsPlus", ex.Message);
            }
        }        


        public override void Terminate()
        {
            if (pluginMenuItem != null)
            {
                try
                {
                    pluginMenuItem.Click -= MenuItem_Click;

                    if (_host?.MainWindow != null)
                    {
                        var ctx = _host.MainWindow.EntryContextMenu;
                        if (ctx != null && ctx.Items.Contains(pluginMenuItem))
                            ctx.Items.Remove(pluginMenuItem);

                        var toolsMenu = _host.MainWindow.MainMenuStrip?.Items["m_menuTools"] as ToolStripMenuItem;
                        if (toolsMenu != null && toolsMenu.DropDownItems.Contains(pluginMenuItem))
                            toolsMenu.DropDownItems.Remove(pluginMenuItem);
                    }
                }
                catch { }

                try { pluginMenuItem.Dispose(); } catch { }
                pluginMenuItem = null;
            }
        }
    }
}