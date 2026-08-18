using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using KeePass;
using KeePass.Plugins;
using KeePass.Util;
using KeePassLib;
using KeePassLib.Security;
using KeePassLib.Utility;

namespace PickCharsPlus
{
    public class PickCharsForm : Form
    {
        private string _text;
        private string _selectedFields;
        private Label _output;
        private ComboBox _fieldCombo;
        private FlowLayoutPanel _buttonsPanel;
        private PwEntry _entry;
        private readonly IPluginHost _host;
        private int _maxFieldLength = 0;

        const int PanelMaxWidth = 1600;
        const int PanelMinWidth = 600;
        const int ButtonSize = 50;


        const int ButtonPadding = 4;

        public PickCharsForm(IPluginHost host, PwEntry entry = null)
        {
            _host = host;
            _entry = entry;

            // Panels 
            var panelComboSelector = new Panel { Dock = DockStyle.Top, Height = 50, Padding = new Padding(20, 15, 20, 0) };
            var panelPhoneticDisaply = new Panel { Dock = DockStyle.Bottom, Height = 50, Padding = new Padding(20, 15, 20, 0) };
            _buttonsPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(20, 15, 20, 10) };
            var panelActions = new Panel { Dock = DockStyle.Bottom, Height = 60, Padding = new Padding(20, 15, 20, 10) };

            // create dropdown at top-left
            ComboBox _fieldCombo = CreateDropDown();
            panelComboSelector.Controls.Add(_fieldCombo);

            // initial text: prefer supplied text, otherwise read from selected dropdown field
            _text = GetFieldPlainText(_fieldCombo?.SelectedItem?.ToString() ?? "Password");

            _output = new Label { AutoSize = true, Dock = DockStyle.Top };
            panelPhoneticDisaply.Controls.Add(_output);


            var copyButton = new Button
            {
                Text = "Copy to Clipboard",
                Width = 200,
                Height = 36,
                Location = new Point(10, 7),
                Anchor = AnchorStyles.Left | AnchorStyles.Bottom
            };
            copyButton.Click += (s, e) => CopyToClipboard();
            panelActions.Controls.Add(copyButton);

            var autoTypeButton = new Button
            {
                Text = "AutoType",
                Width = 200,
                Height = 36,
                Location = new Point(220, 7),
                Anchor = AnchorStyles.Left | AnchorStyles.Bottom
            };
            autoTypeButton.Click += (s, e) => AutoTypeSelection();
            panelActions.Controls.Add(autoTypeButton);

            BuildButtonsForText();

            // Add panesls 
            Controls.Add(_buttonsPanel);         // Fill FIRST
            Controls.Add(panelPhoneticDisaply);  // Bottom 2
            Controls.Add(panelActions);          // Bottom 1
            Controls.Add(panelComboSelector);    // Top

            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "Pick characters from " + (_entry?.Strings.Get(PwDefs.TitleField)?.ReadString() ?? "entry");
            SetClientSize(_maxFieldLength);

            // Allow form to receive key events before child controls
            this.KeyPreview = true;
            this.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Escape)
                    this.Close();
            };
        }

        private ComboBox CreateDropDown()
        {
            PwEntry entry = this._entry;

            _fieldCombo = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 260, Location = new Point(10, 7), Anchor = AnchorStyles.Left | AnchorStyles.Top };

            // Common fields first
            var advancedFields = new List<string> { "Password" };

            if (entry != null)
            {
                foreach (KeyValuePair<string, ProtectedString> kv in entry.Strings)
                {
                    if (!PwDefs.IsStandardField(kv.Key) && !advancedFields.Contains(kv.Key))
                    {
                        advancedFields.Add(kv.Key);
                    }
                }
            }

            foreach (var f in advancedFields)
                _fieldCombo.Items.Add(f);

            // compute max field length among options
            _maxFieldLength = 0;
            foreach (var item in _fieldCombo.Items)
            {
                string fn = item as string ?? string.Empty;
                int len = GetFieldPlainText(fn).Length;
                if (len > _maxFieldLength) _maxFieldLength = len;
            }

            if (_fieldCombo.Items.Count > 0) _fieldCombo.SelectedIndex = 0;
            _fieldCombo.SelectedIndexChanged += FieldCombo_SelectedIndexChanged;

            // if (_fieldCombo.Items.Count == 1) _fieldCombo.Enabled = false;
            return _fieldCombo;
        }

        private void FieldCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            string field = _fieldCombo.SelectedItem?.ToString() ?? "Password";
            _text = GetFieldPlainText(field);
            _output.Text = string.Empty;
            _selectedFields = string.Empty;
            BuildButtonsForText();
            SetClientSize(_text.Length);
        }

        private string GetFieldPlainText(string fieldName)
        {
            if (_entry == null) return string.Empty;

            ProtectedString ps = null;
            if (fieldName.Equals("Password", StringComparison.OrdinalIgnoreCase))
                ps = _entry.Strings.Get(PwDefs.PasswordField);
            else
                ps = _entry.Strings.Get(fieldName);

            return ps != null ? ps.ReadString() : string.Empty;
        }

        private void BuildButtonsForText()
        {
            _buttonsPanel.Controls.Clear();

            if (string.IsNullOrEmpty(_text))
            {
                _buttonsPanel.Controls.Add(new Label { Text = "(Field is empty)", AutoSize = true });
                return;
            }

            int n = _text.Length;
            int maxButtons = Math.Min(n, 1000);
            for (int i = 0; i < maxButtons; ++i)
            {
                int index = i;
                var btn = new Button
                {
                    Tag = index,
                    Text = (i + 1).ToString(),
                    Width = ButtonSize,
                    Height = ButtonSize,
                    Margin = new Padding(ButtonPadding)
                };
                btn.Click += (s, e) => RevealChar(s, index);
                _buttonsPanel.Controls.Add(btn);
            }

            if (n > maxButtons)
                _buttonsPanel.Controls.Add(new Label { Text = $"... (field length {n} > {maxButtons})" });
        }

        private void RevealChar(object sender, int index)
        {
            if (index < 0 || index >= _text.Length) return;

            if (sender is Button btn)
            {
                btn.Text = _text[index].ToString();
                btn.BackColor = Color.LimeGreen;
                btn.Font = new Font(btn.Font, FontStyle.Bold);
            }

            _output.Text += Phonetics.GetPhonetic(_text[index]) + "  ";
            _selectedFields += _text[index];
        }

        private void CopyToClipboard()
        {
            if (!string.IsNullOrEmpty(_selectedFields))
            {

                // Copy your string
                ClipboardUtil.CopyAndMinimize(_selectedFields, false, _host.MainWindow, null, _host.Database);

                // Start a timer to clear clipboard
                long timeoutSeconds = Program.Config.Security.ClipboardClearAfterSeconds;
                var timer = new System.Threading.Timer(state =>
                {
                    // Runs on a ThreadPool thread, need to marshal to UI thread
                    if (Application.OpenForms.Count > 0)
                    {
                        Application.OpenForms[0].Invoke(new MethodInvoker(() =>
                        {
                            ClipboardUtil.ClearIfOwner();
                        }));
                    }
                }, null, timeoutSeconds * 1000, Timeout.Infinite);
            }
        }

        private string EscapeSendKeys(string input)
        {
            return input
                .Replace("{", "{{}")
                .Replace("}", "{}}")
                .Replace("+", "{+}")
                .Replace("^", "{^}")
                .Replace("%", "{%}")
                .Replace("~", "{~}");
        }

        private void AutoTypeSelection()
        {
            if (!string.IsNullOrEmpty(_selectedFields))
            {
                try {
                    string sequence = EscapeSendKeys(_selectedFields);
                    bool result = KeePass.Util.AutoType.PerformIntoPreviousWindow(_host.MainWindow, _entry, _host.Database, sequence);
                    if (!result)
                    {
                        MessageBox.Show("AutoType failed.", "AutoType Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                } catch (Exception ex) { MessageService.ShowWarning(ex); }
            }
        }

        private void SetClientSize(int fieldLength)
        {
            int desiredWidth = Math.Min(PanelMaxWidth, Math.Max(PanelMinWidth, 80 + (fieldLength+1) * (ButtonPadding + ButtonSize)));
            this.ClientSize = new Size(desiredWidth, this.ClientSize.Height);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);

            try
            {
                // Clear sensitive text from memory (best-effort)
                if (!string.IsNullOrEmpty(_text))
                {
                    var zeros = new char[_text.Length];
                    for (int i = 0; i < zeros.Length; ++i) zeros[i] = '\0';
                    _text = new string(zeros);
                    _text = string.Empty;
                }

                // Clear UI and detach references
                if (_output != null) _output.Text = string.Empty;

                if (_buttonsPanel != null)
                {
                    foreach (Control c in _buttonsPanel.Controls)
                    {
                        if (c is Button b)
                        {
                            try { b.Enabled = false; } catch { }
                            try { b.Text = string.Empty; } catch { }
                        }
                    }
                    _buttonsPanel.Controls.Clear();
                }

                // Null the entry reference
                _entry = null;
            }
            catch { }
        }
    }
}
