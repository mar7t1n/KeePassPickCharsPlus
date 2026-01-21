namespace PickCharsPlus
{
    using KeePass.Forms;
    using KeePass.Plugins;
    using KeePass.UI;
    using KeePassLib;
    using System;
    using System.IO;
    using System.Text;
    using System.Reflection;
    using System.Linq;
    using System.Diagnostics;
    using System.Windows.Forms;


    public sealed class PickCharsPlusExt : KeePass.Plugins.Plugin
    {
        private IPluginHost _host;
        private ToolStripMenuItem pluginMenuItem;
        private EntryToolsHelper _entryToolsHelper;
        private readonly object _logLock = new object();
        private string LogFilePath => Path.Combine(Path.GetTempPath(), "PickCharsPlusExt.log");

        public const string PickCharsMenuLabel = "Pick Chars Plus+";

        public override bool Initialize(IPluginHost host)
        {
            _host = host ?? throw new ArgumentNullException(nameof(host));
            try
            {
                // Use EntryToolsHelper to manage the Tools button/menu modifications
                _entryToolsHelper = new EntryToolsHelper(_host, msg => Log(msg), Show_PickCharsPlus_Dialog);
                GlobalWindowManager.WindowAdded += _entryToolsHelper.OnWindowAdded;

                // Setup the right-click entry context menu via helper
                pluginMenuItem = ContextMenuHelper.SetupEntryContextMenu(_host, PickCharsMenuLabel, Show_PickCharsPlus_Dialog, msg => Log(msg));
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }


        /// This routine invokes our PickCharsForm dialog for the currently selected entry
        private void Show_PickCharsPlus_Dialog(object sender, EventArgs e)
        {
            try
            {
                Log("MenuItem_Click: invoked");
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
                System.Windows.Forms.MessageBox.Show(PickCharsMenuLabel, ex.Message);
                Log($"MenuItem_Click: exception {ex}");
            }
        }

        public override void Terminate()
        {
            Log("Terminate: start");
            try
            {
                if (_entryToolsHelper != null)
                {
                    try { GlobalWindowManager.WindowAdded -= _entryToolsHelper.OnWindowAdded; } catch { }
                    _entryToolsHelper = null;
                }
            }
            catch { }
            if (pluginMenuItem != null)
            {
                try
                {
                    pluginMenuItem.Click -= Show_PickCharsPlus_Dialog;

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
            Log("Terminate: end");
        }

        [Conditional("DEBUG")]
        private void Log(string message)
        {
            try
            {
                var ts = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                string line = ts + " - " + message + Environment.NewLine;
                lock (_logLock)
                {
                    File.AppendAllText(LogFilePath, line, Encoding.UTF8);
                }
            }
            catch { }
        }
    }
}
