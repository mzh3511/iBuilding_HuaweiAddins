using System.AddIn;
using System.Drawing;
using System.Linq;
using RanOpt.iBuilding.BLL;
using RanOpt.iBuilding.Extensions.AddInView;
using RanOpt.iBuilding.LayoutModel;

namespace HuaweiAddins.TestRegion
{
    [AddIn("RotateDev", Version = "1.0.0.0", Publisher = "zhenhua.mao", Description = "顺时针旋转设备")]
    public class HuaweiAddIn_RotateDev : IRibbonMenuAV
    {
        private MainHostAV _host;

        #region Implementation of IRibbonMenuAV

        public void Init(MainHostAV host)
        {
            _host = host;
            host.AppendRibbonMenu("Huawei", "顺时针旋转设备45°", null, null);
        }
        /// <summary>
        /// 给定一个平层设备，顺时针旋转该设备45°
        /// </summary>
        /// <param name="tag"></param>
        public void OnMenuInvoked(object tag)
        {
            if (Project.Current == null)
                return;
            var activeControl = _host.GetActiveView();
            var fldView = activeControl as BaseLayoutView;

            //1 获取要旋转的设备
            var layoutDevice = fldView?.LayoutLayer.LayList.OfType<LayoutDevice>().FirstOrDefault();
            if (layoutDevice == null)
                return;

            //2 顺时针旋转45°
            if (fldView.LayMode == LayoutMode.LayoutLD)
            {
                //FLD
                layoutDevice.FLDAngle += 45;
            }
            else
            {
                //NSD
                layoutDevice.Angle += 45;
            }

            fldView.Invalidate();
        }

        #endregion
    }
}