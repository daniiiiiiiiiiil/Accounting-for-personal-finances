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
        FinanceControls financeControls;
        public Form1()
        {
            InitializeComponent();
            financeControls = new FinanceControls();
            financeControls.InitializeComponents(this);
        }
    }
}
