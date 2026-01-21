#if DEBUG
using KeePass.Forms;
using System;
using System.Linq;
using System.Windows.Forms;

namespace PickCharsPlus
{
    internal static class DebugHelpers
    {
        public static void ListAllButtonsAndOptions(PwEntryForm form, Action<string> log)
        {
            try
            {
                log("ListAllButtonsAndOptions: start");
                var all = new System.Collections.Generic.List<Control>();
                void Collect(Control parent)
                {
                    foreach (Control c in parent.Controls)
                    {
                        all.Add(c);
                        if (c.HasChildren) Collect(c);
                    }
                }
                Collect(form);

                var buttons = all.Where(c => c is Button).Cast<Button>().ToList();
                log($"ListAllButtonsAndOptions: totalControls={all.Count} buttons={buttons.Count}");

                for (int i = 0; i < buttons.Count; i++)
                {
                    var b = buttons[i];
                    log($"Button[{i}] Name='{b.Name}' Text='{b.Text}' Type='{b.GetType().FullName}' Visible={b.Visible} Enabled={b.Enabled}");

                    try { log($"Button[{i}] ParentType={b.Parent?.GetType().FullName}"); } catch { }

                    try
                    {
                        var prop = b.GetType().GetProperty("ContextMenuStrip");
                        var cms = prop?.GetValue(b) as ContextMenuStrip;
                        if (cms != null)
                        {
                            log($"Button[{i}] ContextMenuStrip Items.Count={cms.Items.Count}");
                            for (int j = 0; j < cms.Items.Count; j++)
                                log($"Button[{i}] CMS item[{j}] Text='{cms.Items[j].Text}' Type='{cms.Items[j].GetType().FullName}'");
                        }
                        else
                        {
                            log($"Button[{i}] ContextMenuStrip=null");
                        }
                    }
                    catch (Exception ex)
                    {
                        log($"Button[{i}] reading ContextMenuStrip failed: {ex}");
                    }

                    try
                    {
                        if (b.Parent is ToolStrip tsParent)
                        {
                            log($"Button[{i}] parent is ToolStrip with Items.Count={tsParent.Items.Count}");
                            for (int j = 0; j < tsParent.Items.Count; j++)
                                log($"Button[{i}] parent item[{j}] Text='{tsParent.Items[j].Text}' Type='{tsParent.Items[j].GetType().FullName}'");
                        }
                    }
                    catch (Exception ex)
                    {
                        log($"Button[{i}] reading parent ToolStrip failed: {ex}");
                    }

                    try
                    {
                        var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public;
                        var fields = b.GetType().GetFields(flags);
                        foreach (var f in fields)
                        {
                            var ft = f.FieldType.FullName ?? string.Empty;
                            if (ft.IndexOf("ContextMenuStrip", StringComparison.OrdinalIgnoreCase) >= 0 || ft.IndexOf("ToolStrip", StringComparison.OrdinalIgnoreCase) >= 0 || ft.IndexOf("DropDown", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                object val = null;
                                try { val = f.GetValue(b); } catch { }
                                log($"Button[{i}] field '{f.Name}' Type='{ft}' ValueNull={(val==null)}");
                                if (val is ContextMenuStrip cmsf)
                                {
                                    log($"Button[{i}] field '{f.Name}' ContextMenuStrip Items.Count={cmsf.Items.Count}");
                                    for (int j = 0; j < cmsf.Items.Count; j++)
                                        log($"Button[{i}] field '{f.Name}' item[{j}] Text='{cmsf.Items[j].Text}' Type='{cmsf.Items[j].GetType().FullName}'");
                                }
                                else if (val is ToolStrip tsf)
                                {
                                    log($"Button[{i}] field '{f.Name}' ToolStrip Items.Count={tsf.Items.Count}");
                                    for (int j = 0; j < tsf.Items.Count; j++)
                                        log($"Button[{i}] field '{f.Name}' item[{j}] Text='{tsf.Items[j].Text}' Type='{tsf.Items[j].GetType().FullName}'");
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        log($"Button[{i}] reflection scan failed: {ex}");
                    }
                }

                try
                {
                    var toolStrips = all.Where(c => c is ToolStrip).Cast<ToolStrip>().ToList();
                    log($"ListAllButtonsAndOptions: toolStrips found={toolStrips.Count}");
                    for (int t = 0; t < toolStrips.Count; t++)
                    {
                        var ts = toolStrips[t];
                        log($"ToolStrip[{t}] Items.Count={ts.Items.Count} Type={ts.GetType().FullName}");
                        for (int j = 0; j < ts.Items.Count; j++)
                            log($"ToolStrip[{t}] item[{j}] Text='{ts.Items[j].Text}' Type='{ts.Items[j].GetType().FullName}'");
                    }
                }
                catch { }

                log("ListAllButtonsAndOptions: end");
            }
            catch (Exception ex)
            {
                log($"ListAllButtonsAndOptions: exception {ex}");
            }
        }
    }
}
#endif
