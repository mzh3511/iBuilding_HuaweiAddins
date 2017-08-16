using System;
using System.AddIn;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using ASM.Entity;
using RanOpt.iBuilding.BLL;
using RanOpt.iBuilding.DBM.Device;
using RanOpt.iBuilding.Extensions.AddInView;
using RanOpt.iBuilding.LayoutModel;

namespace HuaweiAddins.TestRegion
{
    [AddIn("CrossPts", Version = "1.0.0.0", Publisher = "zhenhua.mao", Description = "复制含穿墙点的线缆对象")]
    public class HuaweiAddIn_MoveCableHandlePt : IRibbonMenuAV
    {
        private MainHostAV _host;

        #region Implementation of IRibbonMenuAV

        public void Init(MainHostAV host)
        {
            _host = host;
            host.AppendRibbonMenu("Huawei", "上移选中线缆的关键点", null, null);
        }
        /// <summary>
        /// 给定一个平层的线缆，要求该线缆有拐点
        /// 该示例代码需要的项目：在平层添加信源和天线，连接，然后拖拽线缆的关键点，使线缆增加拐点
        /// 选中该线缆，点击按钮，线缆的拐点的位置上移1米
        /// </summary>
        /// <param name="tag"></param>
        public void OnMenuInvoked(object tag)
        {
            if (Project.Current == null)
                return;
            var activeControl = _host.GetActiveView();
            var fldView = activeControl as BaseLayoutView;

            //1 获取需要替换的线缆
            var layoutCable = fldView?.LayoutLayer.GetSelectedObject(fldView).OfType<LayoutCable>().FirstOrDefault();
            if (layoutCable == null)
                return;
            throw new NotImplementedException();

        }
        #endregion
    }
}