using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using RanOpt.iBuilding.ReportEX;

namespace HuaweiAddins.Report
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ReportHandlerFactory factory = new ReportHandlerFactory();
            ReportHandlerBase handler = null;
            int errCode = factory.Create(@"Extensions\AddIns\HuaweiAddins.Report\Report.mrt", out handler);
            if (errCode == 0)
            {
                handler.LoadData();
                handler.Render(this.stiViewerControl1);
            }
        }
    }
}
