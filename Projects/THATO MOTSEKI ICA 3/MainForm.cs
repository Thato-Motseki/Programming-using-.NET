using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace EcoCashSimulation
{
    public class MainForm : Form
    {
        private readonly Dictionary<int, Account> accounts = new Dictionary<int, Account>();
        private Account currentAccount;
        private Panel contentPanel;
        private Label balanceLabel;
        private DataGridView historyGrid;
        private readonly Color ink = Color.FromArgb(15, 23, 42);
        private readonly Color teal = Color.FromArgb(37, 99, 235);
        private readonly Color pale = Color.FromArgb(245, 248, 255);

        public MainForm()
        {
            CreateSampleAccounts();
            Text = "EcoCash Lesotho";
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(920, 600);
            Size = new Size(1080, 700);
            ShowLogin();
        }

        private void CreateSampleAccounts()
        {
            accounts.Add(61234567, new Account(61234567, "Thato Motseki", "1234", 2500m));
            accounts.Add(62345678, new Account(62345678, "Mpho Mokoena", "4321", 1800m));
            accounts.Add(63456789, new Account(63456789, "Lerato Phiri", "1111", 3200m));
        }

        private void ShowLogin()
        {
            Controls.Clear();
            Panel page = new Panel { Dock = DockStyle.Fill, BackColor = pale };
            Panel card = new Panel { Size = new Size(450, 470), BackColor = Color.White };
            ApplyRoundedCorners(card, 22);
            Action centerCard = delegate { card.Location = new Point(Math.Max(0, (page.ClientSize.Width - card.Width) / 2), Math.Max(0, (page.ClientSize.Height - card.Height) / 2)); };
            page.Resize += delegate { centerCard(); };
            page.Controls.Add(card);
            card.Controls.Add(LabelFor("ECOCASH", 28, FontStyle.Bold, teal, new Point(42, 38), new Size(340, 45)));
            card.Controls.Add(LabelFor("Lesotho mobile money", 12, FontStyle.Regular, Color.Gray, new Point(45, 82), new Size(300, 25)));
            card.Controls.Add(LabelFor("Sign in to your wallet", 20, FontStyle.Bold, ink, new Point(42, 140), new Size(340, 35)));
            TextBox phone = Input("EcoCash number", card, 45, 205, false);
            TextBox pin = Input("4-digit PIN", card, 45, 285, true);
            Button login = ButtonFor("Sign in", teal, Color.White, 45, 370, 165, 46);
            Button register = ButtonFor("Create account", Color.White, teal, 230, 370, 165, 46);
            login.Click += delegate { Login(phone.Text, pin.Text); };
            register.Click += delegate { ShowRegistration(); };
            card.Controls.Add(login); card.Controls.Add(register); Controls.Add(page); centerCard();
        }

        private void Login(string phoneText, string pin)
        {
            int phoneNumber;
            if (!int.TryParse(phoneText, out phoneNumber) || !IsValidPhoneNumber(phoneNumber)) { MessageBox.Show("Enter a valid 8-digit EcoCash number starting with 6.", "Login"); return; }
            if (!accounts.ContainsKey(phoneNumber)) { MessageBox.Show("Account not found. Create an account to get started.", "Login"); return; }
            if (!accounts[phoneNumber].VerifyPin(pin)) { MessageBox.Show("Incorrect PIN.", "Login"); return; }
            currentAccount = accounts[phoneNumber]; ShowDashboard();
        }

        private void ShowRegistration()
        {
            using (Form dialog = new Form())
            {
                dialog.Text = "Create EcoCash account"; dialog.Size = new Size(390, 410); dialog.StartPosition = FormStartPosition.CenterParent; dialog.FormBorderStyle = FormBorderStyle.FixedDialog; dialog.MaximizeBox = false;
                TextBox phone = Input("EcoCash number", dialog, 35, 25, false); TextBox name = Input("Full name", dialog, 35, 90, false); TextBox pin = Input("4-digit PIN", dialog, 35, 155, true); TextBox confirm = Input("Confirm PIN", dialog, 35, 220, true);
                Button create = ButtonFor("Create account", teal, Color.White, 35, 290, 150, 42);
                create.Click += delegate
                {
                    int number;
                    if (!int.TryParse(phone.Text, out number) || !IsValidPhoneNumber(number)) { MessageBox.Show("Enter a valid 8-digit number starting with 6."); return; }
                    if (accounts.ContainsKey(number)) { MessageBox.Show("That number is already registered."); return; }
                    if (string.IsNullOrWhiteSpace(name.Text) || pin.Text.Length != 4 || !pin.Text.All(char.IsDigit)) { MessageBox.Show("Enter a name and a 4-digit numeric PIN."); return; }
                    if (pin.Text != confirm.Text) { MessageBox.Show("PINs do not match."); return; }
                    currentAccount = new Account(number, name.Text.Trim(), pin.Text, 0m); accounts.Add(number, currentAccount); dialog.DialogResult = DialogResult.OK;
                };
                dialog.Controls.Add(create);
                if (dialog.ShowDialog(this) == DialogResult.OK) ShowDashboard();
            }
        }

        private void ShowDashboard()
        {
            Controls.Clear();
            Panel header = new Panel { Dock = DockStyle.Top, Height = 86, BackColor = ink };
            header.Controls.Add(LabelFor("Hello, " + currentAccount.Name, 19, FontStyle.Bold, Color.White, new Point(30, 17), new Size(500, 30)));
            header.Controls.Add(LabelFor(currentAccount.PhoneNumber.ToString(), 11, FontStyle.Regular, Color.LightGray, new Point(32, 49), new Size(300, 22)));
            Button logout = ButtonFor("Log out", Color.Transparent, Color.White, 840, 24, 110, 36); logout.Anchor = AnchorStyles.Top | AnchorStyles.Right; logout.Click += delegate { currentAccount = null; ShowLogin(); }; header.Controls.Add(logout);
            contentPanel = new Panel { Dock = DockStyle.Fill, BackColor = pale, Padding = new Padding(30) }; Controls.Add(contentPanel); Controls.Add(header);
            Panel balance = new Panel { Location = new Point(30, 30), Size = new Size(360, 150), BackColor = teal };
            ApplyRoundedCorners(balance, 18);
            balance.Controls.Add(LabelFor("AVAILABLE BALANCE", 11, FontStyle.Bold, Color.White, new Point(22, 22), new Size(250, 25)));
            balanceLabel = LabelFor("M " + currentAccount.Balance.ToString("0.00"), 31, FontStyle.Bold, Color.White, new Point(22, 57), new Size(310, 52)); balance.Controls.Add(balanceLabel); contentPanel.Controls.Add(balance);
            AddAction("Deposit money", 430, 30, delegate { Deposit(); }); AddAction("Send money", 600, 30, delegate { SendMoney(); }); AddAction("Cash out", 770, 30, delegate { CashOut(); });
            contentPanel.Controls.Add(LabelFor("Recent transactions", 20, FontStyle.Bold, ink, new Point(30, 215), new Size(400, 35)));
            historyGrid = new DataGridView { Location = new Point(30, 260), Size = new Size(900, 300), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom, ReadOnly = true, AllowUserToAddRows = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, BackgroundColor = Color.White, BorderStyle = BorderStyle.None, RowHeadersVisible = false };
            historyGrid.ColumnHeadersDefaultCellStyle.BackColor = ink; historyGrid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White; historyGrid.EnableHeadersVisualStyles = false; contentPanel.Controls.Add(historyGrid); RefreshDashboard();
        }

        private void AddAction(string text, int x, int y, EventHandler action) { Button button = ButtonFor(text, Color.White, teal, x, y, 145, 150); button.Click += action; contentPanel.Controls.Add(button); }
        private void RefreshDashboard() { balanceLabel.Text = "M " + currentAccount.Balance.ToString("0.00"); historyGrid.DataSource = currentAccount.GetTransactions().OrderByDescending(t => t.TransactionDate).Select(t => new { Type = t.TransactionType, Amount = "M " + t.Amount.ToString("0.00"), Reference = t.Reference, Date = t.TransactionDate.ToString("dd/MM/yyyy HH:mm"), Status = t.Status }).ToList(); }

        private void Deposit() { string amountText; if (!Prompt("Deposit money", "Amount (M)", out amountText)) return; decimal amount; if (!decimal.TryParse(amountText, out amount) || amount <= 0) { MessageBox.Show("Amount must be greater than zero."); return; } DepositTransaction transaction = new DepositTransaction(currentAccount, amount); transaction.Process(); currentAccount.AddTransaction(transaction); RefreshDashboard(); MessageBox.Show("Deposit successful.\nReference: " + transaction.Reference); }
        private void CashOut() { string amountText; if (!Prompt("Cash out", "Amount (M)", out amountText)) return; decimal amount; if (!decimal.TryParse(amountText, out amount) || amount <= 0) { MessageBox.Show("Amount must be greater than zero."); return; } if (!ConfirmPin()) return; try { CashOutTransaction transaction = new CashOutTransaction(currentAccount, amount); transaction.Process(); currentAccount.AddTransaction(transaction); RefreshDashboard(); MessageBox.Show("Cash out successful."); } catch (InsufficientBalanceException ex) { MessageBox.Show(ex.Message, "Cash out"); } }
        private void SendMoney() { string recipientText; if (!Prompt("Send money", "Recipient number", out recipientText)) return; int recipientNumber; if (!int.TryParse(recipientText, out recipientNumber) || !accounts.ContainsKey(recipientNumber) || recipientNumber == currentAccount.PhoneNumber) { MessageBox.Show("Choose a valid recipient account other than yourself."); return; } string amountText; if (!Prompt("Send money", "Amount (M)", out amountText)) return; decimal amount; if (!decimal.TryParse(amountText, out amount) || amount <= 0) { MessageBox.Show("Amount must be greater than zero."); return; } if (!ConfirmPin()) return; try { Account recipient = accounts[recipientNumber]; SendMoneyTransaction transaction = new SendMoneyTransaction(currentAccount, recipient, amount); transaction.Process(); currentAccount.AddTransaction(transaction); recipient.AddTransaction(transaction); RefreshDashboard(); MessageBox.Show("Sent to " + recipient.Name + "."); } catch (InsufficientBalanceException ex) { MessageBox.Show(ex.Message, "Send money"); } }
        private bool ConfirmPin() { string pin; if (!Prompt("Confirm transaction", "PIN", out pin, true)) return false; if (!currentAccount.VerifyPin(pin)) { MessageBox.Show("Incorrect PIN. Transaction cancelled."); return false; } return true; }
        private bool Prompt(string title, string caption, out string value, bool password = false) { using (Form dialog = new Form()) { dialog.Text = title; dialog.Size = new Size(340, 170); dialog.StartPosition = FormStartPosition.CenterParent; Label label = new Label { Text = caption, Location = new Point(25, 25), AutoSize = true }; TextBox input = new TextBox { Location = new Point(25, 55), Width = 270, UseSystemPasswordChar = password }; Button ok = ButtonFor("Continue", teal, Color.White, 25, 90, 100, 32); ok.DialogResult = DialogResult.OK; dialog.Controls.AddRange(new Control[] { label, input, ok }); dialog.AcceptButton = ok; value = dialog.ShowDialog(this) == DialogResult.OK ? input.Text : null; return value != null; } }
        private TextBox Input(string caption, Control parent, int x, int y, bool password) { TextBox box = new TextBox { Location = new Point(x, y + 22), Width = 340, Height = 36, Font = new Font("Segoe UI", 13, FontStyle.Regular), BorderStyle = BorderStyle.FixedSingle, UseSystemPasswordChar = password }; ApplyRoundedCorners(box, 10); parent.Controls.Add(LabelFor(caption, 10, FontStyle.Bold, ink, new Point(x, y), new Size(340, 20))); parent.Controls.Add(box); return box; }
        private void ApplyRoundedCorners(Control control, int radius) { using (GraphicsPath path = new GraphicsPath()) { int diameter = radius * 2; path.AddArc(0, 0, diameter, diameter, 180, 90); path.AddArc(control.Width - diameter, 0, diameter, diameter, 270, 90); path.AddArc(control.Width - diameter, control.Height - diameter, diameter, diameter, 0, 90); path.AddArc(0, control.Height - diameter, diameter, diameter, 90, 90); path.CloseFigure(); control.Region = new Region(path); } }
        private Button ButtonFor(string text, Color back, Color fore, int x, int y, int width, int height) { Button button = new Button { Text = text, Location = new Point(x, y), Size = new Size(width, height), BackColor = back, ForeColor = fore, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold), Cursor = Cursors.Hand }; button.FlatAppearance.BorderColor = teal; button.FlatAppearance.BorderSize = 1; ApplyRoundedCorners(button, 12); return button; }
        private Label LabelFor(string text, float size, FontStyle style, Color color, Point location, Size bounds) { return new Label { Text = text, Font = new Font("Segoe UI", size, style), ForeColor = color, Location = location, Size = bounds }; }
        private bool IsValidPhoneNumber(int phoneNumber) { return phoneNumber >= 60000000 && phoneNumber <= 69999999; }
    }
}
