using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Учет_личных_финансов_c_
{
    public class FinanceControls
    {
        public TextBox AmountTextBox { get; private set; }
        public TextBox DescriptionTextBox { get; private set; }

        public ComboBox TypeComboBox { get; private set; }
        public ComboBox CategoryComboBox { get; private set; }
        public ComboBox PaymentMethodComboBox { get; private set; }

        public Button AddButton { get; private set; }
        public Button EditButton { get; private set; }
        public Button DeleteButton { get; private set; }
        public Button ReportButton { get; private set; }

        public DataGridView OperationsDataGridView { get; private set; }
        public DataGridView SummaryDataGridView { get; private set; }

        public DateTimePicker DatePicker { get; private set; }

        public Label BalanceLabel { get; private set; }
        public Label IncomeLabel { get; private set; }
        public Label ExpenseLabel { get; private set; }

        public void InitializeComponents(Control parent)
        {
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
                Items = { "Продукты", "Жилье","Транспорт","ЖКХ","Здоровье","Техника","Покупки в интернете","Подписки","Одежда","Подарки","Кафе и рестораны","Другое" }
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
                Width = 150,
            };

            AddButton = new Button
            {
                Text = "Добавить",
                Location = new Point(20, 110)
            };

            OperationsDataGridView = new DataGridView
            {
                Location = new Point(20, 150),
                Width = 600,
                Height = 200
            };

            BalanceLabel = new Label
            {
                Text = "Баланс: 0",
                Location = new Point(350, 20),
                Font = new Font("Arial", 12, FontStyle.Bold)
            };

            parent.Controls.Add(AmountTextBox);
            parent.Controls.Add(DescriptionTextBox);
            parent.Controls.Add(TypeComboBox);
            parent.Controls.Add(CategoryComboBox);
            parent.Controls.Add(PaymentMethodComboBox);
            parent.Controls.Add(DatePicker);
            parent.Controls.Add(AddButton);
            parent.Controls.Add(OperationsDataGridView);
            parent.Controls.Add(BalanceLabel);
        }
    }
}
