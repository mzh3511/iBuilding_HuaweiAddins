using System.AddIn;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using ASM.Entity;
using RanOpt.iBuilding.BLL;
using RanOpt.iBuilding.Common.Units;
using RanOpt.iBuilding.DBM.Device;
using RanOpt.iBuilding.Extensions.AddInView;
using RanOpt.iBuilding.LayoutModel;

namespace HuaweiAddins.TestRegion
{
    [AddIn("DeviceInfo", Version = "1.0.0.0", Publisher = "zhenhua.mao", Description = "移动设备到圆点")]
    public class HuaweiAddIn_MoveDev2Zero : IRibbonMenuAV
    {
        private MainHostAV _host;

        #region Implementation of IRibbonMenuAV

        public void Init(MainHostAV host)
        {
            _host = host;
            host.AppendRibbonMenu("Huawei", "移动选中设备到圆点", null, null);
        }

        public void OnMenuInvoked(object tag)
        {
            if (Project.Current == null)
                return;

            var fldView = _host.GetActiveView() as BaseLayoutView;
            var layoutLayer = fldView?.LayoutLayer;
            var selection = layoutLayer?.GetSelectedObject(fldView);
            if (selection == null || selection.Count == 0)
                return;
            var dev = selection[0] as LayoutDevice;
            if (dev == null)
                return;
            //修改设备位置到圆点
            dev.Rectangles[(int)LayoutMode.LayoutLD] = new RectangleF(new PointF(), dev.Rectangles[(int)LayoutMode.LayoutLD].Size);
            //通知与该设备相关联的对象
            dev.NotifyMovement();
            //重新计算当前View中线缆显示路径
            layoutLayer.CalculateDecorateLine(fldView);
            fldView.Invalidate();
        }
        #endregion
    }
}
