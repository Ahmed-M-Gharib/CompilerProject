using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TinyLanguageScanner;

namespace TinyScanner
{
    public class Form1 : Form
    {
        private TextBox txtCode;
        private Button btnScan;
        private Button btnClear;
        private DataGridView dgvTokens;
        private Label lblStatus;
        private Label lblCodeHeader;
        private Label lblTokensHeader;

        public Form1()
        {
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            this.Text = "Tiny Language - Lexical Analyzer";
            this.Size = new Size(950, 600);
            this.MinimumSize = new Size(800, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Segoe UI", 9.5f);

            lblCodeHeader = new Label
            {
                Text = "Source Code:",
                Location = new Point(12, 12),
                Size = new Size(120, 20),
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
            };

            txtCode = new TextBox
            {
                Location = new Point(12, 35),
                Size = new Size(380, 460),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Consolas", 10f),
                AcceptsReturn = true,
                AcceptsTab = true,
                Text = "/* computes factorial */\r\nint main()\r\n{\r\n    int x;\r\n    read x;\r\n    if x > 0 then\r\n        int fact := 1;\r\n        repeat\r\n            fact := fact * x;\r\n            x := x - 1;\r\n        until x = 0\r\n        write fact;\r\n    end\r\n    return 0;\r\n}"
            };

            btnScan = new Button
            {
                Text = "Scan",
                Location = new Point(400, 35),
                Size = new Size(80, 32),
                BackColor = Color.FromArgb(0, 100, 180),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnScan.FlatAppearance.BorderSize = 0;
            btnScan.Click += BtnScan_Click;

            btnClear = new Button
            {
                Text = "Clear",
                Location = new Point(400, 75),
                Size = new Size(80, 32),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnClear.Click += (s, e) => {
                txtCode.Clear();
                dgvTokens.Rows.Clear();
                lblStatus.Text = "";
            };

            lblTokensHeader = new Label
            {
                Text = "Tokens:",
                Location = new Point(490, 12),
                Size = new Size(80, 20),
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
            };

            dgvTokens = new DataGridView
            {
                Location = new Point(490, 35),
                Size = new Size(440, 460),
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToResizeRows = false,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                Font = new Font("Consolas", 9.5f)
            };

            dgvTokens.Columns.Add("colLine", "Line");
            dgvTokens.Columns.Add("colType", "Token Type");
            dgvTokens.Columns.Add("colValue", "Value");

            dgvTokens.Columns["colLine"].FillWeight = 10;
            dgvTokens.Columns["colType"].FillWeight = 35;
            dgvTokens.Columns["colValue"].FillWeight = 55;

            dgvTokens.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(0, 100, 180);
            dgvTokens.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvTokens.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            dgvTokens.EnableHeadersVisualStyles = false;
            dgvTokens.RowsDefaultCellStyle.SelectionBackColor = Color.FromArgb(200, 225, 245);
            dgvTokens.RowsDefaultCellStyle.SelectionForeColor = Color.Black;
            dgvTokens.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 248, 252);

            lblStatus = new Label
            {
                Location = new Point(12, 503),
                Size = new Size(910, 22),
                Font = new Font("Segoe UI", 9f)
            };

            this.Controls.AddRange(new Control[] {
                lblCodeHeader, txtCode,
                btnScan, btnClear,
                lblTokensHeader, dgvTokens,
                lblStatus
            });

            this.Resize += (s, e) => ResizeControls();
        }

        private void BtnScan_Click(object sender, EventArgs e)
        {
            dgvTokens.Rows.Clear();

            string source = txtCode.Text;
            if (string.IsNullOrWhiteSpace(source))
            {
                lblStatus.Text = "Nothing to scan.";
                return;
            }

            var scanner = new Scanner();
            var tokens = scanner.Tokenize(source);

            foreach (var tok in tokens)
                dgvTokens.Rows.Add(tok.Line, tok.Type.ToString(), tok.Value);

            var errors = tokens.Where(t => t.Type == TokenType.UNKNOWN).ToList();
            if (errors.Any())
            {
                lblStatus.ForeColor = Color.Red;
                lblStatus.Text = $"Errors found: {errors.Count} unknown token(s) — " +
                    string.Join(", ", errors.Select(t => $"line {t.Line}: '{t.Value}'"));
            }
            else
            {
                lblStatus.ForeColor = Color.FromArgb(0, 130, 0);
                lblStatus.Text = $"Done. {tokens.Count} tokens found, no errors.";
            }
        }

        private void ResizeControls()
        {
            int w = this.ClientSize.Width;
            int h = this.ClientSize.Height;
            int leftW = (int)(w * 0.42);

            txtCode.Size = new Size(leftW - 120, h - 100);
            btnScan.Location = new Point(leftW - 100, 35);
            btnClear.Location = new Point(leftW - 100, 75);

            int rightX = leftW + 20;
            lblTokensHeader.Location = new Point(rightX, 12);
            dgvTokens.Location = new Point(rightX, 35);
            dgvTokens.Size = new Size(w - rightX - 12, h - 100);

            lblStatus.Location = new Point(12, h - 40);
            lblStatus.Size = new Size(w - 24, 22);
        }
    }
}
