using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Учет_личных_финансов_c_
{
    public class FinanceControls
    {
        // Основные элементы управления транзакциями
        public TextBox AmountTextBox { get; private set; }
        public TextBox DescriptionTextBox { get; private set; }
        public ComboBox TypeComboBox { get; private set; }
        public ComboBox CategoryComboBox { get; private set; }
        public ComboBox PaymentMethodComboBox { get; private set; }
        public DateTimePicker DatePicker { get; private set; }

        // Кнопки операций
        public Button AddButton { get; private set; }
        public Button EditButton { get; private set; }
        public Button DeleteButton { get; private set; }
        public Button ReportButton { get; private set; }

        // Таблицы
        public DataGridView OperationsDataGridView { get; private set; }
        public DataGridView SummaryDataGridView { get; private set; }
        public DataGridView BudgetsDataGridView { get; private set; }
        public DataGridView RecurringPaymentsDataGridView { get; private set; }
        public DataGridView DebtsDataGridView { get; private set; }

        // Метки
        public Label BalanceLabel { get; private set; }
        public Label IncomeLabel { get; private set; }
        public Label ExpenseLabel { get; private set; }
        public Label BudgetStatusLabel { get; private set; }

        // Элементы для работы с бюджетами
        public ComboBox BudgetCategoryComboBox { get; private set; }
        public TextBox BudgetLimitTextBox { get; private set; }
        public Button SetBudgetButton { get; private set; }

        // Элементы для регулярных платежей
        public ComboBox RecurringFrequencyComboBox { get; private set; }
        public DateTimePicker RecurringStartDatePicker { get; private set; }
        public Button AddRecurringPaymentButton { get; private set; }

        // Элементы для работы с долгами
        public TextBox DebtPersonTextBox { get; private set; }
        public TextBox DebtAmountTextBox { get; private set; }
        public CheckBox IsOwedToMeCheckBox { get; private set; }
        public DateTimePicker DebtDueDatePicker { get; private set; }
        public Button AddDebtButton { get; private set; }
        public Button SettleDebtButton { get; private set; }

        // Элементы для валют
        public ComboBox CurrencyComboBox { get; private set; }
        public Button UpdateRatesButton { get; private set; }

        // Элементы для резервного копирования
        public Button BackupButton { get; private set; }
        public Button RestoreButton { get; private set; }

        // Вкладки для организации интерфейса
        public TabControl MainTabControl { get; private set; }

        public void InitializeComponents(Control parent)
        {
            MainTabControl = new TabControl
            {
                Location = new Point(10, 10),
                Size = new Size(780, 580),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            // Вкладка операций
            var operationsTab = new TabPage("Операции");
            InitializeOperationsTab(operationsTab);
            MainTabControl.TabPages.Add(operationsTab);

            // Вкладка бюджетов
            var budgetsTab = new TabPage("Бюджеты");
            InitializeBudgetsTab(budgetsTab);
            MainTabControl.TabPages.Add(budgetsTab);

            // Вкладка регулярных платежей
            var recurringTab = new TabPage("Регулярные платежи");
            InitializeRecurringTab(recurringTab);
            MainTabControl.TabPages.Add(recurringTab);

            // Вкладка долгов
            var debtsTab = new TabPage("Долги");
            InitializeDebtsTab(debtsTab);
            MainTabControl.TabPages.Add(debtsTab);

            // Вкладка настроек
            var settingsTab = new TabPage("Настройки");
            InitializeSettingsTab(settingsTab);
            MainTabControl.TabPages.Add(settingsTab);

            parent.Controls.Add(MainTabControl);
        }

        private void InitializeOperationsTab(TabPage tab)
        {
            // Основные элементы управления транзакциями
            AmountTextBox = new TextBox { Location = new Point(20, 20), Width = 100 };
            DescriptionTextBox = new TextBox { Location = new Point(130, 20), Width = 200 };

            TypeComboBox = new ComboBox
            {
                Location = new Point(20, 50),
                Width = 100,
                Items = { "Доход", "Расход" }
            };

            CategoryComboBox = new ComboBox
            {
                Location = new Point(130, 50),
                Width = 200,
                Items = { "Продукты", "Жилье", "Транспорт", "ЖКХ", "Здоровье",
                         "Техника", "Покупки в интернете", "Подписки",
                         "Одежда", "Подарки", "Кафе и рестораны", "Другое" }
            };

            PaymentMethodComboBox = new ComboBox
            {
                Location = new Point(340, 50),
                Width = 120,
                Items = { "Карта", "Наличные" }
            };

            DatePicker = new DateTimePicker
            {
                Location = new Point(20, 80),
                Width = 150
            };

            // Кнопки операций
            AddButton = new Button { Text = "Добавить", Location = new Point(20, 110) };
            EditButton = new Button { Text = "Редактировать", Location = new Point(120, 110) };
            DeleteButton = new Button { Text = "Удалить", Location = new Point(220, 110) };
            ReportButton = new Button { Text = "Отчет", Location = new Point(320, 110) };

            // Таблицы
            OperationsDataGridView = new DataGridView
            {
                Location = new Point(20, 150),
                Size = new Size(740, 200),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            SummaryDataGridView = new DataGridView
            {
                Location = new Point(20, 360),
                Size = new Size(740, 150),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            // Метки
            BalanceLabel = new Label
            {
                Text = "Баланс: 0",
                Location = new Point(500, 20),
                Font = new Font("Arial", 12, FontStyle.Bold),
                AutoSize = true
            };

            IncomeLabel = new Label
            {
                Text = "Доходы: 0",
                Location = new Point(500, 50),
                Font = new Font("Arial", 10),
                AutoSize = true
            };

            ExpenseLabel = new Label
            {
                Text = "Расходы: 0",
                Location = new Point(500, 80),
                Font = new Font("Arial", 10),
                AutoSize = true
            };

            // Добавление элементов на вкладку
            tab.Controls.AddRange(new Control[] {
                AmountTextBox, DescriptionTextBox, TypeComboBox, CategoryComboBox,
                PaymentMethodComboBox, DatePicker, AddButton, EditButton,
                DeleteButton, ReportButton, OperationsDataGridView,
                SummaryDataGridView, BalanceLabel, IncomeLabel, ExpenseLabel
            });
        }

        private void InitializeBudgetsTab(TabPage tab)
        {
            BudgetCategoryComboBox = new ComboBox
            {
                Location = new Point(20, 20),
                Width = 200,
                Items = { "Продукты", "Жилье", "Транспорт", "ЖКХ", "Здоровье",
                         "Техника", "Покупки в интернете", "Подписки",
                         "Одежда", "Подарки", "Кафе и рестораны", "Другое" }
            };

            BudgetLimitTextBox = new TextBox
            {
                Location = new Point(230, 20),
                Width = 100
            };

            SetBudgetButton = new Button
            {
                Text = "Установить лимит",
                Location = new Point(340, 20)
            };

            BudgetStatusLabel = new Label
            {
                Location = new Point(20, 50),
                Width = 400,
                Text = "Статус бюджетов:"
            };

            BudgetsDataGridView = new DataGridView
            {
                Location = new Point(20, 80),
                Size = new Size(740, 400),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            tab.Controls.AddRange(new Control[] {
                BudgetCategoryComboBox, BudgetLimitTextBox, SetBudgetButton,
                BudgetStatusLabel, BudgetsDataGridView
            });
        }

        private void InitializeRecurringTab(TabPage tab)
        {
            RecurringFrequencyComboBox = new ComboBox
            {
                Location = new Point(20, 20),
                Width = 150,
                Items = { "Ежедневно", "Еженедельно", "Ежемесячно", "Ежегодно" }
            };

            RecurringStartDatePicker = new DateTimePicker
            {
                Location = new Point(180, 20),
                Width = 150
            };

            AddRecurringPaymentButton = new Button
            {
                Text = "Добавить регулярный платеж",
                Location = new Point(340, 20)
            };

            RecurringPaymentsDataGridView = new DataGridView
            {
                Location = new Point(20, 60),
                Size = new Size(740, 420),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            tab.Controls.AddRange(new Control[] {
                RecurringFrequencyComboBox, RecurringStartDatePicker,
                AddRecurringPaymentButton, RecurringPaymentsDataGridView
            });
        }

        private void InitializeDebtsTab(TabPage tab)
        {
            DebtPersonTextBox = new TextBox
            {
                Location = new Point(20, 20),
                Width = 150,
                Text = "Кому/от кого"
            };

            DebtAmountTextBox = new TextBox
            {
                Location = new Point(180, 20),
                Width = 100,
                Text = "Сумма"
            };

            IsOwedToMeCheckBox = new CheckBox
            {
                Location = new Point(290, 20),
                Text = "Мне должны",
                AutoSize = true
            };

            DebtDueDatePicker = new DateTimePicker
            {
                Location = new Point(400, 20),
                Width = 150
            };

            AddDebtButton = new Button
            {
                Text = "Добавить долг",
                Location = new Point(560, 20)
            };

            SettleDebtButton = new Button
            {
                Text = "Погасить",
                Location = new Point(660, 20)
            };

            DebtsDataGridView = new DataGridView
            {
                Location = new Point(20, 60),
                Size = new Size(740, 420),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            tab.Controls.AddRange(new Control[] {
                DebtPersonTextBox, DebtAmountTextBox, IsOwedToMeCheckBox,
                DebtDueDatePicker, AddDebtButton, SettleDebtButton, DebtsDataGridView
            });
        }

        private void InitializeSettingsTab(TabPage tab)
        {
            CurrencyComboBox = new ComboBox
            {
                Location = new Point(20, 20),
                Width = 150,
                Items = { "RUB", "USD", "EUR", "GBP" }
            };

            UpdateRatesButton = new Button
            {
                Text = "Обновить курсы",
                Location = new Point(180, 20)
            };

            BackupButton = new Button
            {
                Text = "Создать резервную копию",
                Location = new Point(20, 60),
                Width = 200
            };

            RestoreButton = new Button
            {
                Text = "Восстановить из копии",
                Location = new Point(230, 60),
                Width = 200
            };

            tab.Controls.AddRange(new Control[] {
                CurrencyComboBox, UpdateRatesButton,
                BackupButton, RestoreButton
            });
        }
    }
}