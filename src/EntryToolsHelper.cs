/// This routine helps add our menu item to the Entry Tools menu button in the Entry Edit dialog

using KeePass.Forms;
using KeePass.Plugins;
using KeePass.UI;
using System;
using System.Reflection;
using System.Windows.Forms;

namespace PickCharsPlus
{
    internal class EntryToolsHelper
    {
        private readonly IPluginHost _host;
        private readonly Action<string> _log;
        private readonly EventHandler _onClick;

        public EntryToolsHelper(IPluginHost host, Action<string> log, EventHandler onClick)
        {
            _host = host;
            _log = log ?? (_ => { });
            _onClick = onClick;
        }

        public void OnWindowAdded(object sender, GwmWindowEventArgs e)
        {
            PwEntryForm entryForm = e.Form as PwEntryForm;
            if (entryForm == null)
            {
                _log("EntryToolsHelper.OnWindowAdded: not a PwEntryForm");
                return;
            }

            _log("EntryToolsHelper.OnWindowAdded: PwEntryForm detected");

#if DEBUG
            // Debug: list all buttons and options on the form to help explain how it works
            DebugHelpers.ListAllButtonsAndOptions(entryForm, msg => _log(msg));
#endif

            // Modify immediately and also after Load
            try
            {
                ModifyEntryToolsMenu(entryForm);
            }
            catch (Exception ex)
            {
                _log($"EntryToolsHelper: ModifyEntryToolsMenu immediate call failed: {ex}");
            }

            entryForm.Load += (s, args) =>
            {
                _log("EntryToolsHelper.OnWindowAdded: Load event fired");
                try { ModifyEntryToolsMenu(entryForm); } catch (Exception ex) { _log($"EntryToolsHelper: ModifyEntryToolsMenu on Load failed: {ex}"); }
            };
        }

        private void ModifyEntryToolsMenu(PwEntryForm form)
        {
            _log("EntryToolsHelper.ModifyEntryToolsMenu: start");

            Control[] controls = form.Controls.Find("m_btnTools", true);
            _log($"EntryToolsHelper.ModifyEntryToolsMenu: controls found={controls?.Length}");

            if (controls.Length == 0)
            {
                _log("EntryToolsHelper.ModifyEntryToolsMenu: no m_btnTools");
                return;
            }

            if (!(controls[0] is Button btnTools))
            {
                _log($"EntryToolsHelper.ModifyEntryToolsMenu: controls[0] is not Button (actual={controls[0].GetType().FullName})");
                return;
            }

            _log("EntryToolsHelper.ModifyEntryToolsMenu: m_btnTools is Button");

            try
            {
                var flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
                var f = form.GetType().GetField("m_ctxTools", flags);
                if (f != null)
                {
                    object val = null;
                    try { val = f.GetValue(form); } catch { }
                    _log($"EntryToolsHelper: found field 'm_ctxTools' Type='{f.FieldType.FullName}' ValueNull={(val == null)}");
                    if (val is ContextMenuStrip cms)
                    {
                        _log($"EntryToolsHelper: inserting into m_ctxTools ContextMenuStrip with Items.Count={cms.Items.Count}");
                        TryInsertIntoToolStripItemCollection(cms.Items);
                    }
                    else if (val is ToolStrip ts)
                    {
                        _log($"EntryToolsHelper: inserting into m_ctxTools ToolStrip with Items.Count={ts.Items.Count}");
                        TryInsertIntoToolStrip(ts);
                    }
                    else
                    {
                        _log($"EntryToolsHelper: m_ctxTools exists but has unexpected type {f.FieldType.FullName}");
                    }
                }
                else
                {
                    _log("EntryToolsHelper: field 'm_ctxTools' not found on form");
                }
            }
            catch (Exception ex)
            {
                _log($"EntryToolsHelper: error accessing m_ctxTools: {ex}");
            }

            _log("EntryToolsHelper.ModifyEntryToolsMenu: end");
        }

        private void TryInsertIntoToolStrip(ToolStrip toolStrip)
        {
            if (toolStrip == null) { _log("TryInsertIntoToolStrip: toolStrip is null"); return; }
            ToolStripMenuItem myItem = new ToolStripMenuItem(PickCharsPlusExt.PickCharsMenuLabel);
            myItem.Click += (s, e) => {
                _log("Dynamic menu clicked -> forwarding to Show_PickCharsPlus_Dialog");
                try
                {
                    _host?.MainWindow?.BeginInvoke(new Action(() => { try { _onClick?.Invoke(s, e); } catch (Exception ex) { _log($"EntryToolsHelper: onClick invoke failed: {ex}"); } }));
                }
                catch (Exception ex)
                {
                    _log($"EntryToolsHelper: BeginInvoke failed: {ex}");
                }
            };

            int insertIndex = 0;
            for (int i = 0; i < toolStrip.Items.Count; i++)
            {
                string text = toolStrip.Items[i].Text ?? string.Empty;
                _log($"TryInsertIntoToolStrip: item[{i}] Text='{text}' Type='{toolStrip.Items[i].GetType().FullName}'");
                if (text.IndexOf("initial password", StringComparison.OrdinalIgnoreCase) >= 0
                    || (text.IndexOf("copy", StringComparison.OrdinalIgnoreCase) >= 0 && text.IndexOf("password", StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    insertIndex = i + 1;
                    break;
                }
            }

            if (!MenuLooksLikeTools(toolStrip.Items))
            {
                _log("TryInsertIntoToolStrip: target toolStrip does not look like tools menu; skipping insert");
                return;
            }

            try
            {
                toolStrip.Items.Insert(insertIndex, myItem);
                _log($"TryInsertIntoToolStrip: inserted myItem at index={insertIndex}");
            }
            catch (Exception ex)
            {
                _log($"TryInsertIntoToolStrip: insert failed: {ex}");
            }
        }

        private void TryInsertIntoToolStripItemCollection(ToolStripItemCollection items)
        {
            if (items == null) { _log("TryInsertIntoToolStripItemCollection: items is null"); return; }
            ToolStripMenuItem myItem = new ToolStripMenuItem(PickCharsPlusExt.PickCharsMenuLabel);
            myItem.Click += (s, e) => {
                _log("Dynamic menu clicked -> forwarding to Show_PickCharsPlus_Dialog");
                try { _host?.MainWindow?.BeginInvoke(new Action(() => { try { _onClick?.Invoke(s, e); } catch (Exception ex) { _log($"EntryToolsHelper: onClick invoke failed: {ex}"); } })); } catch (Exception ex) { _log($"EntryToolsHelper: BeginInvoke failed: {ex}"); }
            };

            int insertIndex = 0;
            for (int i = 0; i < items.Count; i++)
            {
                string text = items[i].Text ?? string.Empty;
                _log($"TryInsertIntoToolStripItemCollection: item[{i}] Text='{text}' Type='{items[i].GetType().FullName}'");
                if (text.IndexOf("initial password", StringComparison.OrdinalIgnoreCase) >= 0
                    || (text.IndexOf("copy", StringComparison.OrdinalIgnoreCase) >= 0 && text.IndexOf("password", StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    insertIndex = i + 1;
                    break;
                }
            }

            if (!MenuLooksLikeTools(items))
            {
                _log("TryInsertIntoToolStripItemCollection: target items do not look like tools menu; skipping insert");
                return;
            }

            try
            {
                items.Insert(insertIndex, myItem);
                _log($"TryInsertIntoToolStripItemCollection: inserted myItem at index={insertIndex}");
            }
            catch (Exception ex)
            {
                _log($"TryInsertIntoToolStripItemCollection: insert failed: {ex}");
            }
        }

        private bool MenuLooksLikeTools(ToolStripItemCollection items)
        {
            if (items == null) return false;
            for (int i = 0; i < items.Count; i++)
            {
                var text = items[i].Text ?? string.Empty;
                if (text.IndexOf("initial password", StringComparison.OrdinalIgnoreCase) >= 0
                    || text.IndexOf("copy", StringComparison.OrdinalIgnoreCase) >= 0 && text.IndexOf("password", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    _log($"MenuLooksLikeTools: matched marker in item[{i}] Text='{text}'");
                    return true;
                }
            }
            _log("MenuLooksLikeTools: no marker found in items");
            return false;
        }
    }
}
