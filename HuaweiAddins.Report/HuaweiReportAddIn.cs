/*******************************************************************************
 * Copyright：All rights reserved by Ranplan Co.
 * CLR  版本：4.0.30319.18052
 * 功能 说明：
 * 处理 记录：
 * 1、2014/6/4 15:45:57 MAOZHENHUA-PC created.
 *******************************************************************************/
using System;
using System.AddIn;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RanOpt.iBuilding.BLL;
using RanOpt.iBuilding.Extensions.AddInView;

namespace HuaweiAddins.Report
{
    [AddIn("HuaweiReportAddIn", Version = "1.0.0.0", Publisher = "zhenhua.mao", Description = "Huawei custom report")]
    public class HuaweiReportAddIn:IRibbonMenuAV
    {
        private MainHostAV _MainHost;
        #region Implementation of IRibbonMenuAV

        public void Init(MainHostAV host)
        {
            _MainHost = host;
            host.AppendRibbonMenu("Huawei","Report",null,null);
        }

        public void OnMenuInvoked(object tag)
        {
            if(Project.Current == null)
                return;
            Form1 form = new Form1();
            form.ShowDialog();
        }

        #endregion
    }
}
