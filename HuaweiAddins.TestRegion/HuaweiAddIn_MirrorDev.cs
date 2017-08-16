using System.AddIn;
using System.Drawing;
using System.Linq;
using RanOpt.iBuilding.BLL;
using RanOpt.iBuilding.Extensions.AddInView;
using RanOpt.iBuilding.LayoutModel;

namespace HuaweiAddins.TestRegion
{
    [AddIn("MirrorDev", Version = "1.0.0.0", Publisher = "zhenhua.mao", Description = "反转设备")]
    public class HuaweiAddIn_MirrorDev : IRibbonMenuAV
    {
        private MainHostAV _host;

        #region Implementation of IRibbonMenuAV

        public void Init(MainHostAV host)
        {
            _host = host;
            host.AppendRibbonMenu("Huawei", "反转设备", null, null);
        }
        /// <summary>
        /// 给定一个平层设备，反转该设备45°
        /// </summary>
        /// <param name="tag"></param>
        public void OnMenuInvoked(object tag)
        {
            if (Project.Current == null)
                return;
            var activeControl = _host.GetActiveView();
            var fldView = activeControl as BaseLayoutView;

            //1 获取要反转的设备
            var layoutDevice = fldView?.LayoutLayer.LayList.OfType<LayoutDevice>().FirstOrDefault();
            if (layoutDevice == null)
                return;

            //2 反转
            if (fldView.LayMode == LayoutMode.LayoutLD)
            {
                //FLD
                layoutDevice.FLDMirror = !layoutDevice.FLDMirror;
            }
            else
            {
                //NSD
                layoutDevice.Mirror = !layoutDevice.Mirror;
            }

            fldView.Invalidate();
        }

        #endregion
    }
}