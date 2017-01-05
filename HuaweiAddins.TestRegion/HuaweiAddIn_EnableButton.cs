using System;
using System.AddIn;
using System.Windows.Forms;
using DevComponents.DotNetBar;
using RanOpt.iBuilding.BLL;
using RanOpt.iBuilding.BSMModule;
using RanOpt.iBuilding.Extensions.AddInView;
using RanOpt.iBuilding.SD_LD;
using RanOpt.iBuilding.SD_LD.View;
using RanOpt.iBuilding.UI;

namespace HuaweiAddins.TestRegion
{
    [AddIn("HuaweiAddIn_EnableButton", Version = "1.0.0.0", Publisher = "zhenhua.mao", Description = "Huawei 按钮可用性控制")]
    public class HuaweiAddIn_EnableButton : IRibbonMenuAV
    {
        private MainHostAV _host;
        private ButtonItem _bsmBtn;
        private ButtonItem _fldBtn;
        private ButtonItem _switch2BsmBtn;
        private ButtonItem _switch2FldBtn;

        #region Implementation of IRibbonMenuAV

        public void Init(MainHostAV host)
        {
            _host = host;

            _bsmBtn = host.AppendRibbonMenuEx("Huawei", "BSM按钮", null, null) as ButtonItem;
            _bsmBtn.Enabled = false;

            _fldBtn = host.AppendRibbonMenuEx("Huawei", "FLD按钮", null, null) as ButtonItem;
            _fldBtn.Enabled = false;

            _switch2BsmBtn = host.AppendRibbonMenuEx("Huawei", "切换为BSM", null, null) as ButtonItem;
            _switch2BsmBtn.Click += _switch2BsmBtn_Click;
            _switch2BsmBtn.Enabled = false;

            _switch2FldBtn = host.AppendRibbonMenuEx("Huawei", "切换为FLD", null, null) as ButtonItem;
            _switch2FldBtn.Click += _switch2FldBtn_Click;
            _switch2FldBtn.Enabled = false;

            var context = _host.Context as IAppContext;
            if (context != null)
            {
                context.MainUIActiveDocChanged += Context_MainUIActiveDocChanged;
            }
        }

        private void _switch2FldBtn_Click(object sender, EventArgs e)
        {
            var editForm = _host.GetActiveDocument() as IFloorEditForm;
            if (editForm == null)
                return;
            var srcEditMode = editForm.EditMode;
            if(srcEditMode == FloorEditMode.FLD)
                return;
            MessageBox.Show($"{srcEditMode}----->{FloorEditMode.FLD}");
            editForm.EditMode = FloorEditMode.FLD;
        }

        private void _switch2BsmBtn_Click(object sender, EventArgs e)
        {
            var editForm = _host.GetActiveDocument() as IFloorEditForm;
            if (editForm == null)
                return;
            var srcEditMode = editForm.EditMode;
            if (srcEditMode == FloorEditMode.BSM)
                return;
            MessageBox.Show($"{srcEditMode}----->{FloorEditMode.BSM}");
            editForm.EditMode = FloorEditMode.BSM;
        }

        private void Context_MainUIActiveDocChanged(object sender, EventArgs e)
        {
            _bsmBtn.Enabled = _host.GetActiveView() is DiagrammingControl;
            _fldBtn.Enabled = _host.GetActiveView() is LayoutView;
            _switch2BsmBtn.Enabled = _bsmBtn.Enabled || _fldBtn.Enabled;
            _switch2FldBtn.Enabled = _bsmBtn.Enabled || _fldBtn.Enabled;
        }

        public void OnMenuInvoked(object tag)
        {
        }
        #endregion
    }
}
