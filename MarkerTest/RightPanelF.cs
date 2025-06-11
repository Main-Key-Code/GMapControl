using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MarkerTest
{
    public partial class RightPanelF : UserControl
    {
        public RightPanelF()
        {
            InitializeComponent();

        }

        Panel pnlMain;
        private void RightPanelF_Load(object sender, EventArgs e)
        {
           pnlMain = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true
            };
            this.Controls.Add(pnlMain);
            this.Dock = DockStyle.Right;
            this.Width = 200; // Set the width of the right panel
        }
    }
}
