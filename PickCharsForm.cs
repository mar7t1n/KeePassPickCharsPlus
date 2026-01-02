using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using KeePassLib;
using KeePassLib.Security;

namespace PickCharsPlus
{
    public class PickCharsForm : Form
    {
        private string _text;
        private Label _output;
        private ComboBox _fieldCombo;
        private FlowLayoutPanel _buttonsPanel;
        private PwEntry _entry;
        private int _maxFieldLength = 0;

        const int PanelMaxWidth = 1600;
        const int PanelMinWidth = 600;
        const int ButtonSize = 50;


        const int ButtonPadding = 4;

        public PickCharsForm(PwEntry entry = null)
        {
            _entry = entry;

            // create dropdown at top-left
            createDropDown();

            // initial text: prefer supplied text, otherwise read from selected dropdown field
            _text = GetFieldPlainText(_fieldCombo?.SelectedItem?.ToString() ?? "Password");

            _output = new Label { Location = new Point(10, 50), AutoSize = true };
            Controls.Add(_output);

            _buttonsPanel = new FlowLayoutPanel {  
                Location = new Point(10, 80),
                Width = ClientSize.Width - 20,
                Height = ClientSize.Height - 80,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                AutoSize = true,
                AutoScroll = false,
                WrapContents = true
            };

            Controls.Add(_buttonsPanel);

            BuildButtonsForText();

            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "Pick characters from " + (_entry?.Strings.Get(PwDefs.TitleField)?.ReadString() ?? "entry");
            setClientSize(_maxFieldLength);

            // Allow form to receive key events before child controls
            this.KeyPreview = true;
            this.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Escape)
                    this.Close();
            };
        }

        private void createDropDown()
        {
            PwEntry entry = this._entry;

            _fieldCombo = new ComboBox { Location = new Point(10, 10), DropDownStyle = ComboBoxStyle.DropDownList, Width = 260 };

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

            if (_fieldCombo.Items.Count == 1) _fieldCombo.Enabled = false;
            Controls.Add(_fieldCombo);
        }

        private void FieldCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            string field = _fieldCombo.SelectedItem?.ToString() ?? "Password";
            _text = GetFieldPlainText(field);
            _output.Text = string.Empty;
            BuildButtonsForText();
            setClientSize(_text.Length);
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
        }

        private void setClientSize(int fieldLength)
        {
            int desiredWidth = Math.Min(PanelMaxWidth, Math.Max(PanelMinWidth, 80 + fieldLength * (ButtonPadding + ButtonSize)));
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
