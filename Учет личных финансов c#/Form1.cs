using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Учет_личных_финансов_c_
{
    public partial class Form1 : Form
    {
        private readonly DatabaseManager _dbManager;
        private readonly FinanceControls _controls;

        public Form1()
        {
            InitializeComponent();

            _dbManager = new DatabaseManager();

            Text = "Учет личных финансов";
            Size = new Size(820, 650);
            MinimumSize = new Size(820, 650);
            StartPosition = FormStartPosition.CenterScreen;

            _controls = new FinanceControls();
            _controls.InitializeComponents(this);

            ConfigureDataGridViews();
            SubscribeToEvents();
            LoadData();
        }

        private void ConfigureDataGridViews()
        {
            _controls.OperationsDataGridView.AutoGenerateColumns = false;
            _controls.OperationsDataGridView.Columns.AddRange(
                new DataGridViewTextBoxColumn { HeaderText = "Дата", DataPropertyName = "Date", Width = 100 },
                new DataGridViewTextBoxColumn { HeaderText = "Тип", DataPropertyName = "Type", Width = 80 },
                new DataGridViewTextBoxColumn { HeaderText = "Категория", DataPropertyName = "Category", Width = 120 },
                new DataGridViewTextBoxColumn { HeaderText = "Сумма", DataPropertyName = "Amount", Width = 80 },
                new DataGridViewTextBoxColumn { HeaderText = "Описание", DataPropertyName = "Description", Width = 200 },
                new DataGridViewTextBoxColumn { HeaderText = "Способ оплаты", DataPropertyName = "PaymentMethod", Width = 100 }
            );

            _controls.BudgetsDataGridView.AutoGenerateColumns = false;
            _controls.BudgetsDataGridView.Columns.AddRange(
                new DataGridViewTextBoxColumn { HeaderText = "Категория", DataPropertyName = "Category", Width = 200 },
                new DataGridViewTextBoxColumn { HeaderText = "Лимит", DataPropertyName = "Limit", Width = 100 },
                new DataGridViewTextBoxColumn { HeaderText = "Потрачено", DataPropertyName = "Spent", Width = 100 },
                new DataGridViewTextBoxColumn { HeaderText = "Остаток", DataPropertyName = "Remaining", Width = 100 }
            );

            _controls.RecurringPaymentsDataGridView.AutoGenerateColumns = false;
            _controls.RecurringPaymentsDataGridView.Columns.AddRange(
                new DataGridViewTextBoxColumn { HeaderText = "Описание", DataPropertyName = "Description", Width = 200 },
                new DataGridViewTextBoxColumn { HeaderText = "Сумма", DataPropertyName = "Amount", Width = 100 },
                new DataGridViewTextBoxColumn { HeaderText = "Категория", DataPropertyName = "Category", Width = 120 },
                new DataGridViewTextBoxColumn { HeaderText = "Периодичность", DataPropertyName = "Frequency", Width = 100 },
                new DataGridViewTextBoxColumn { HeaderText = "Следующий платеж", DataPropertyName = "NextPaymentDate", Width = 120 }
            );

            _controls.DebtsDataGridView.AutoGenerateColumns = false;
            _controls.DebtsDataGridView.Columns.AddRange(
                new DataGridViewTextBoxColumn { HeaderText = "Кому/от кого", DataPropertyName = "Person", Width = 150 },
                new DataGridViewTextBoxColumn { HeaderText = "Сумма", DataPropertyName = "Amount", Width = 100 },
                new DataGridViewCheckBoxColumn { HeaderText = "Мне должны", DataPropertyName = "IsOwedToMe", Width = 80 },
                new DataGridViewTextBoxColumn { HeaderText = "Срок", DataPropertyName = "DueDate", Width = 100 },
                new DataGridViewTextBoxColumn { HeaderText = "Описание", DataPropertyName = "Description", Width = 200 },
                new DataGridViewCheckBoxColumn { HeaderText = "Погашен", DataPropertyName = "IsSettled", Width = 70 }
            );
        }

        private void SubscribeToEvents()
        {
            _controls.AddButton.Click += AddTransaction_Click;
            _controls.EditButton.Click += EditTransaction_Click;
            _controls.DeleteButton.Click += DeleteTransaction_Click;

            _controls.SetBudgetButton.Click += SetBudget_Click;

            _controls.AddRecurringPaymentButton.Click += AddRecurringPayment_Click;

            _controls.AddDebtButton.Click += AddDebt_Click;
            _controls.SettleDebtButton.Click += SettleDebt_Click;

            _controls.UpdateRatesButton.Click += UpdateRates_Click;
            _controls.BackupButton.Click += Backup_Click;
            _controls.RestoreButton.Click += Restore_Click;
        }

        private void LoadData()
        {
            var transactions = _dbManager.GetAllTransactions();
            _controls.OperationsDataGridView.DataSource = transactions;

            UpdateBalanceLabels();

            LoadBudgets();

            _controls.RecurringPaymentsDataGridView.DataSource = _dbManager.GetAllRecurringPayments();

            _controls.DebtsDataGridView.DataSource = _dbManager.GetAllDebts();
        }

        private void UpdateBalanceLabels()
        {
            decimal balance = _dbManager.GetCurrentBalance();
            decimal income = _dbManager.GetTotalIncome();
            decimal expenses = _dbManager.GetTotalExpenses();

            _controls.BalanceLabel.Text = $"Баланс: {balance:C}";
            _controls.IncomeLabel.Text = $"Доходы: {income:C}";
            _controls.ExpenseLabel.Text = $"Расходы: {expenses:C}";
        }

        private void LoadBudgets()
        {
            var budgets = new List<BudgetDisplay>();
            var allBudgets = _dbManager.GetAllBudgets();

            foreach (var kvp in allBudgets)
            {
                decimal spent = _dbManager.GetCategorySpending(kvp.Key, DateTime.Now.AddMonths(-1), DateTime.Now);
                budgets.Add(new BudgetDisplay
                {
                    Category = kvp.Key,
                    Limit = kvp.Value,
                    Spent = spent,
                    Remaining = kvp.Value - spent
                });
            }
            _controls.BudgetsDataGridView.DataSource = budgets;
        }

        #region Обработчики событий

        private void AddTransaction_Click(object sender, EventArgs e)
        {
            try
            {
                decimal amount;
                if (!decimal.TryParse(_controls.AmountTextBox.Text, out amount))
                {
                    MessageBox.Show("Некорректная сумма");
                    return;
                }

                var transaction = new Transaction
                {
                    Amount = amount,
                    Description = _controls.DescriptionTextBox.Text,
                    Type = _controls.TypeComboBox.SelectedItem?.ToString(),
                    Category = _controls.CategoryComboBox.SelectedItem?.ToString(),
                    PaymentMethod = _controls.PaymentMethodComboBox.SelectedItem?.ToString(),
                    Date = _controls.DatePicker.Value
                };

                _dbManager.AddTransaction(transaction);
                LoadData();

                _controls.AmountTextBox.Clear();
                _controls.DescriptionTextBox.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении транзакции: {ex.Message}");
            }
        }

        private void EditTransaction_Click(object sender, EventArgs e)
        {
            if (_controls.OperationsDataGridView.SelectedRows.Count > 0)
            {
                var selected = (Transaction)_controls.OperationsDataGridView.SelectedRows[0].DataBoundItem;

                _controls.AmountTextBox.Text = selected.Amount.ToString();
                _controls.DescriptionTextBox.Text = selected.Description;
                _controls.TypeComboBox.SelectedItem = selected.Type;
                _controls.CategoryComboBox.SelectedItem = selected.Category;
                _controls.PaymentMethodComboBox.SelectedItem = selected.PaymentMethod;
                _controls.DatePicker.Value = selected.Date;

                _dbManager.DeleteTransaction(selected.Id);
            }
        }

        private void DeleteTransaction_Click(object sender, EventArgs e)
        {
            if (_controls.OperationsDataGridView.SelectedRows.Count > 0)
            {
                var selected = (Transaction)_controls.OperationsDataGridView.SelectedRows[0].DataBoundItem;
                _dbManager.DeleteTransaction(selected.Id);
                LoadData();
            }
        }

        private void SetBudget_Click(object sender, EventArgs e)
        {
            try
            {
                var categoryItem = _controls.BudgetCategoryComboBox.SelectedItem;
                if (categoryItem == null)
                {
                    MessageBox.Show("Пожалуйста, выберите категорию");
                    return;
                }
                string category = categoryItem.ToString();

                decimal limit;
                if (!decimal.TryParse(_controls.BudgetLimitTextBox.Text, out limit))
                {
                    MessageBox.Show("Некорректный лимит");
                    return;
                }

                _dbManager.SetBudget(category, limit);
                LoadBudgets();

                _controls.BudgetLimitTextBox.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при установке бюджета: {ex.Message}");
            }
        }

        private void AddRecurringPayment_Click(object sender, EventArgs e)
        {
            try
            {
                var inputForm = new Form
                {
                    Text = "Введите параметры регулярного платежа",
                    Size = new Size(350, 200),
                    StartPosition = FormStartPosition.CenterScreen
                };

                var amountLabel = new Label { Text = "Сумма:", Location = new Point(20, 20), AutoSize = true };
                var amountTextBox = new TextBox { Location = new Point(120, 20), Width = 200 };

                var categoryLabel = new Label { Text = "Категория:", Location = new Point(20, 50), AutoSize = true };
                var categoryTextBox = new TextBox { Location = new Point(120, 50), Width = 200 };

                var frequencyLabel = new Label { Text = "Периодичность:", Location = new Point(20, 80), AutoSize = true };
                var frequencyComboBox = new ComboBox { Location = new Point(120, 80), Width = 200 };
                // Заполните frequencyComboBox нужными значениями, например:
                frequencyComboBox.Items.AddRange(new string[] { "Ежедневно", "Ежемесячно", "Ежегодно" });
                frequencyComboBox.SelectedIndex = 0;

                var dateLabel = new Label { Text = "Следующий платеж:", Location = new Point(20, 110), AutoSize = true };
                var datePicker = new DateTimePicker { Location = new Point(150, 110), Width = 170 };

                var okButton = new Button { Text = "OK", Location = new Point(120, 140), DialogResult = DialogResult.OK };

                inputForm.Controls.AddRange(new Control[] {
                    amountLabel, amountTextBox,
                    categoryLabel, categoryTextBox,
                    frequencyLabel, frequencyComboBox,
                    dateLabel, datePicker,
                    okButton
                });

                if (inputForm.ShowDialog() == DialogResult.OK)
                {
                    if (!decimal.TryParse(amountTextBox.Text, out decimal amount))
                    {
                        MessageBox.Show("Некорректная сумма");
                        return;
                    }

                    var payment = new RecurringPayment
                    {
                        Description = "Регулярный платеж",
                        Amount = amount,
                        Category = categoryTextBox.Text,
                        Frequency = frequencyComboBox.SelectedItem?.ToString(),
                        NextPaymentDate = datePicker.Value
                    };

                    _dbManager.AddRecurringPayment(payment);
                    _controls.RecurringPaymentsDataGridView.DataSource = _dbManager.GetAllRecurringPayments();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении регулярного платежа: {ex.Message}");
            }
        }

        private void AddDebt_Click(object sender, EventArgs e)
        {
            try
            {
                if (!decimal.TryParse(_controls.DebtAmountTextBox.Text, out decimal amount))
                {
                    MessageBox.Show("Некорректная сумма");
                    return;
                }

                var debt = new DebtRecord
                {
                    Person = _controls.DebtPersonTextBox.Text,
                    Amount = amount,
                    IsOwedToMe = _controls.IsOwedToMeCheckBox.Checked,
                    DueDate = _controls.DebtDueDatePicker.Value,
                    Description = "Долговая запись"
                };

                _dbManager.AddDebt(debt);
                _controls.DebtsDataGridView.DataSource = _dbManager.GetAllDebts();

                _controls.DebtPersonTextBox.Clear();
                _controls.DebtAmountTextBox.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении долга: {ex.Message}");
            }
        }

        private void SettleDebt_Click(object sender, EventArgs e)
        {
            if (_controls.DebtsDataGridView.SelectedRows.Count > 0)
            {
                var selected = (DebtRecord)_controls.DebtsDataGridView.SelectedRows[0].DataBoundItem;
                _dbManager.MarkDebtAsSettled(selected.Id);
                _controls.DebtsDataGridView.DataSource = _dbManager.GetAllDebts();
            }
        }

        private void UpdateRates_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Курсы валют обновлены");
        }

        private void Backup_Click(object sender, EventArgs e)
        {
            var saveDialog = new SaveFileDialog { Filter = "Файлы базы данных|*.db" };
            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                MessageBox.Show("Резервная копия создана успешно");
            }
        }

        private void Restore_Click(object sender, EventArgs e)
        {
            var openDialog = new OpenFileDialog { Filter = "Файлы базы данных|*.db" };
            if (openDialog.ShowDialog() == DialogResult.OK)
            {
                MessageBox.Show("Данные восстановлены успешно");
                LoadData();
            }
        }
        #endregion
    }
}
