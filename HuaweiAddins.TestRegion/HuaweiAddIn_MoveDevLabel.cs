using System.AddIn;
using System.Drawing;
using System.Linq;
using RanOpt.iBuilding.BLL;
using RanOpt.iBuilding.Extensions.AddInView;
using RanOpt.iBuilding.LayoutModel;

namespace HuaweiAddins.TestRegion
{
    [AddIn("MoveDevLabel", Version = "1.0.0.0", Publisher = "zhenhua.mao", Description = "向右下方移动设备标签")]
    public class HuaweiAddIn_MoveDevLabel : IRibbonMenuAV
    {
        private MainHostAV _host;

        #region Implementation of IRibbonMenuAV

        public void Init(MainHostAV host)
        {
            _host = host;
            host.AppendRibbonMenu("Huawei", "向右下方移动设备标签", null, null);
        }
        /// <summary>
        /// 给定一个平层设备，移动该设备的名称标签
        /// 该示例代码需要的项目：在平层添加任意一个设备，选中该设备，点击按钮移动该设备的名称标签，向右下方移动(1,-2)个位移
        /// </summary>
        /// <param name="tag"></param>
        public void OnMenuInvoked(object tag)
        {
            if (Project.Current == null)
                return;
            var activeControl = _host.GetActiveView();
            var fldView = activeControl as BaseLayoutView;

            //1 获取要移动标签的设备
            var layoutDevice = fldView?.LayoutLayer.LayList.OfType<LayoutDevice>().FirstOrDefault();
            if (layoutDevice == null)
                return;
            var subTextName = layoutDevice.SubTextList.Find(cond => cond.InfoType == TextInfoType.Name);
            var location = subTextName.Rectangles[(int)fldView.LayMode].Location;
            var size = subTextName.Rectangles[(int)fldView.LayMode].Size;

            //2 向右移动1米，向下移动两米，注意Size是自动算出来的，不要修改
            subTextName.Rectangles[(int)fldView.LayMode] = new RectangleF(location + new SizeF(50, -100), size);

            fldView.Invalidate();
        }

        #endregion
    }
}