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
    public class HuaweiAddIn_CrossPts : IRibbonMenuAV
    {
        private MainHostAV _host;

        #region Implementation of IRibbonMenuAV

        public void Init(MainHostAV host)
        {
            _host = host;
            host.AppendRibbonMenu("Huawei", "穿墙点", null, null);
        }
        /// <summary>
        /// 给定一个穿层的线缆，删除该线缆然后在原位置上重新绘制新线缆
        /// 该示例代码需要的项目：在1层添加信源，2层添加天线，连接，然后在1 2层分别拖拽线缆的关键点使关键点数目增多
        /// 在应用时请根据需要，在Step1中指定具体的线缆对象，并且要求该指定线缆必须跨层
        /// </summary>
        /// <param name="tag"></param>
        public void OnMenuInvoked(object tag)
        {
            if (Project.Current == null)
                return;
            var activeControl = _host.GetActiveView();
            var fldView = activeControl as BaseLayoutView;
            if (fldView == null)
                return;

            //1 获取需要替换的线缆
            var layoutCable = fldView.LayoutLayer.LayList.OfType<LayoutCable>().FirstOrDefault();
            if (layoutCable == null)
                return;

            //2 保存线缆位置信息、连接设备信息
            var crossPointArray1 = new List<PointF>(layoutCable.CrossPointArray1);
            var crossPointArray2 = new List<PointF>(layoutCable.CrossPointArray2);
            var crossLocation = layoutCable.LinkCross.Rectangles[0].GetCenterPt();
            var linkInfo0 = new NodeLink(layoutCable.NodeLink[0]);
            var linkInfo1 = new NodeLink(layoutCable.NodeLink[1]);

            //3 断开并删除原线缆
            layoutCable.DisconnectLink(0);
            layoutCable.DisconnectLink(1);
            fldView.LayoutLayer.Remove(layoutCable);

            //线缆模版
            var cableTemplate = LayoutObject.GetTemplateListByDeviceType(DeviceTypeNew.Cable);
            //4 创建新线缆并连接设备
            var newCable = LinkDevice(fldView, cableTemplate[0],
                linkInfo0.obj as LayoutDevice, linkInfo0.linkIndex,
                linkInfo1.obj as LayoutDevice, linkInfo1.linkIndex);
            //5 给新线缆的穿墙点设定位置
            newCable.LinkCross.SetLocation(0, crossLocation);
            newCable.LinkCross.SetLocation(1, crossLocation);
            newCable.LinkCross.NotifyMovement();
            //6 给新线缆的线条设定关键点
            InsertMiddlePts(crossPointArray1, newCable.CrossPointArray1);
            InsertMiddlePts(crossPointArray2, newCable.CrossPointArray2);

            //重新计算功率
            Project.Current.SystemDesign.LayoutLayer.CalculatePower();
        }
        /// <summary>
        /// 把<paramref name="srcPts"/>中除了首尾的其他点都插入到<paramref name="destPts"/>中
        /// </summary>
        /// <param name="srcPts"></param>
        /// <param name="destPts"></param>
        private void InsertMiddlePts(List<PointF> srcPts, List<PointF> destPts)
        {
            var midPtCount = srcPts.Count - 2;
            if(midPtCount <=0)
                return;

            var midPts = new PointF[midPtCount];
            srcPts.CopyTo(1,midPts,0,midPtCount);
            destPts.InsertRange(1,midPts);
        }

        /// <summary>
        /// 连接设备
        /// </summary>
        /// <param name="fldView"></param>
        /// <param name="cableTemplate"></param>
        /// <param name="srcObj"></param>
        /// <param name="srcPortIndex"></param>
        /// <param name="destObj"></param>
        /// <param name="destPortIndex"></param>
        /// <returns></returns>
        private LayoutCable LinkDevice(BaseLayoutView fldView, TemplateLine cableTemplate,
            LayoutDevice srcObj, int srcPortIndex,
            LayoutDevice destObj, int destPortIndex)
        {
            //获取集合中第一个耦合器模板，并取出DeviceGUID，DeviceGUID是设备在数据库中的GUID值，用于唯一标识一个设备
            var deviceGuid = cableTemplate.Paramters["DeviceGUID"].Value;
            if (string.IsNullOrEmpty(deviceGuid))
                return null;
            //使用DeviceGUID 从数据库中取出设备，并创建设备对象
            var cable = Project.Current.DeviceLibrary.FindDeviceByGuid(deviceGuid);
            if (cable == null || cable.DeviceTypeNew != DeviceTypeNew.Cable)
                return null;
            var layoutCable = new LayoutCable(fldView, 0, 0, 1, 1, cable);
            fldView.LayoutLayer.Add(layoutCable);
            // 以下方式直接连接两个设备，线缆呈一条直线
            layoutCable.MakeLink(0, srcObj, srcPortIndex);
            layoutCable.MakeLink(1, destObj, destPortIndex);
            return layoutCable;
        }

        #endregion
    }

    public static class RectangleFExtension
    {
        /// <summary>
        /// 返回矩形的中心点
        /// </summary>
        /// <param name="rect"></param>
        /// <returns></returns>
        public static PointF GetCenterPt(this RectangleF rect)
        {
            var x = (rect.Left + rect.Right) / 2f;
            var y = (rect.Top + rect.Bottom) / 2f;
            return new PointF(x, y);
        }
    }
}