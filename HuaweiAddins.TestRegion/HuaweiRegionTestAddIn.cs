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
    [AddIn("HuaweiReportAddIn", Version = "1.0.0.0", Publisher = "zhenhua.mao", Description = "Huawei custom report")]
    public class HuaweiRegionTestAddIn : IRibbonMenuAV
    {
        private MainHostAV _Host;

        #region Implementation of IRibbonMenuAV

        public void Init(MainHostAV host)
        {
            _Host = host;
            host.AppendRibbonMenu("Huawei", "TestRegion", null, null);
        }

        public void OnMenuInvoked(object tag)
        {
            if (Project.Current == null)
                return;
            Control activeControl = _Host.GetActiveView();
            if (activeControl != null)
            {
                BaseLayoutView fldView = activeControl as BaseLayoutView;
                if (fldView != null)
                {
                    _Building = Project.Current.FindBuildingByName(fldView.BuildingName);
                    _Floor = _Building.GetFloorByIndex(fldView.FloorIndex);

                    //获取系统模版
                    List<TemplateLine> srcTemplate = LayoutSource.GetSystemTemplateList();
                    //耦合器模版
                    List<TemplateLine> couplerTemplate = LayoutObject.GetTemplateListByDeviceType(DeviceTypeNew.Coupler);
                    //定向天线模版
                    List<TemplateLine> antTemplate = LayoutObject.GetTemplateListByDeviceType(DeviceTypeNew.DirectionalAntenna);
                    //线缆模版
                    List<TemplateLine> cableTemplate = LayoutObject.GetTemplateListByDeviceType(DeviceTypeNew.Cable);

                    //注意，添加信源与其它设备的方法不同

                    //使用系统模版来添加信源
                    LayoutDevice layoutDevice1 = PutSource(fldView, srcTemplate[0].Name, 0, 0, 2.4f);
                    //添加耦合器
                    LayoutDevice layoutDevice2 = PutDevice(fldView, couplerTemplate[0].Name, 5, 0, 2.4f);
                    //添加天线
                    LayoutDevice layoutDevice3 = PutDevice(fldView, antTemplate[0].Name, 10, 0, 2.4f);

                    List<PointF> midPtList = new List<PointF>(3);
                    midPtList.AddRange(new PointF[] { new PointF(1, 1), new PointF(2, 1), new PointF(4, 4) });
                    //使用线缆连接信源和耦合器，并且使线缆中间经过以上三个点
                    LinkDevice(fldView, cableTemplate[0].Name, layoutDevice1, 0, layoutDevice2, 0, midPtList);

                    midPtList = new List<PointF>(3);
                    midPtList.AddRange(new PointF[] { new PointF(6, -1), new PointF(8, -1), new PointF(10, -4) });
                    //使用线缆连接耦合器和天线，并且使线缆中间经过以上三个点
                    LinkDevice(fldView, cableTemplate[0].Name, layoutDevice2, 1, layoutDevice3, 0, midPtList);
                    //重新计算功率
                    Project.Current.SystemDesign.LayoutLayer.CalculatePower();
                }
            }
        }

        private IBuilding _Building;
        private IFloor _Floor;

        /// <summary>
        /// 添加信源
        /// </summary>
        /// <param name="fldView">The FLD view.</param>
        /// <param name="templateName">系统模版名称</param>
        /// <param name="x">位置的X坐标</param>
        /// <param name="y">位置的Y坐标</param>
        /// <param name="z">位置的Z坐标(相对于该楼层地板的高度)</param>
        /// <returns>返回新添加的信源</returns>
        private LayoutSource PutSource(BaseLayoutView fldView, string templateName, float x, float y, float z)
        {
            //以下两句代码主要是为了获取SignalSource
            LayoutObject layoutObject = LayoutFactory.CreateLayoutObject(typeof(LayoutSource).Name);
            LayoutSource layoutSource = layoutObject as LayoutSource;
            if (layoutSource == null)
                return null;
            layoutSource.InitFromTemplate(templateName);
            //使用以上步骤创建出来的SignalSource来进行初始化
            //layoutSource = new LayoutSource(fldView, layoutSource.SignalSource, 0, 0);
            //layoutSource.InitFromSystemTemplate(templateName);
            //为设备设置楼层ID,这决定此设备将会在哪一个楼层的平面图中显示
            layoutSource.AllocatedFloorID = fldView.FloorID;
            //为设备设置楼体
            layoutSource.AllocatedBuildingName = _Building.Name;
            ////为设备布局对象的PartGUID赋值
            //将设备布局对象加入到设备集合中
            Project.Current.SystemDesign.LayoutLayer.Add(layoutSource);
            //设备位置
            layoutSource.SetPosition(fldView, x, y, _Floor.FloorElevation + z);
            return layoutSource;
        }
        /// <summary>
        /// 添加设备（除信源和线缆类）
        /// </summary>
        /// <param name="fldView">The FLD view.</param>
        /// <param name="templateName">设备模版名称</param>
        /// <param name="x">位置的X坐标</param>
        /// <param name="y">位置的Y坐标</param>
        /// <param name="z">位置的Z坐标(相对于该楼层地板的高度)</param>
        /// <returns>返回新添加的设备</returns>
        private LayoutDevice PutDevice(BaseLayoutView fldView, string templateName, float x, float y, float z)
        {
            //根据名称获取模版
            TemplateLine template = LayoutObject.GetTemplate(templateName);
            if (template == null)
                return null;
            //获取集合中第一个耦合器模板，并取出DeviceGUID，DeviceGUID是设备在数据库中的GUID值，用于唯一标识一个设备
            string deviceGuid = template.Paramters["DeviceGUID"].Value;
            if (string.IsNullOrEmpty(deviceGuid))
                return null;
            //使用DeviceGUID 从数据库中取出设备，并创建设备对象
            IDevice device = (IDevice)Project.Current.DeviceLibrary.FindDeviceByGuid(deviceGuid);
            if (device == null || device.DeviceTypeNew == DeviceTypeNew.Cable || device.DeviceTypeNew == DeviceTypeNew.RadiatingCable)
                return null;

            //利用布局对象工厂，创建设备布局对象，CreateLayoutDevice的第一个参数标识要在哪个窗体中绘制设备。(0,0,1,1)绘制矩形的大小，此大小会在软    
            //件内部进行修正，在这里传入0,0,1,1 即可。 最后一个参数就是设备对象
            LayoutDevice layoutdevice = LayoutFactory.CreateLayoutDevice(fldView, 0, 0, 1, 1, device);
            //使用模板来初始化设备布局对象的属性值
            layoutdevice.InitFromTemplate(templateName);
            //为设备设置楼体
            layoutdevice.AllocatedBuildingName = Project.Current.BuildingsCache[0].Name;
            //为设备设置楼层ID,这决定此设备将会在哪一个楼层的平面图中显示
            layoutdevice.AllocatedFloorID = fldView.FloorID;
            //将设备布局对象加入到设备集合中
            Project.Current.SystemDesign.LayoutLayer.Add(layoutdevice);
            //设备位置
            layoutdevice.SetPosition(fldView, x, y, _Floor.FloorElevation + z);
            return layoutdevice;
        }
        /// <summary>
        /// 使用模版名称为<paramref name="cableTemplate"/>的线缆连接两个设备
        /// </summary>
        /// <param name="fldView"></param>
        /// <param name="cableTemplate">线缆模版名称</param>
        /// <param name="srcObj">线缆输入端连接的设备</param>
        /// <param name="srcPortIndex">连接到<paramref name="srcObj"/>对象的该端口上</param>
        /// <param name="destObj">线缆输出端连接的设备</param>
        /// <param name="destPortIndex">连接到<paramref name="destObj"/>对象的该端口上</param>
        /// <param name="midPtList">连接时中间经过的关键点坐标列表</param>
        private void LinkDevice(BaseLayoutView fldView, string cableTemplate,
            LayoutDevice srcObj, int srcPortIndex,
            LayoutDevice destObj, int destPortIndex, List<PointF> midPtList)
        {
            //根据名称获取模版
            TemplateLine template = LayoutObject.GetTemplate(cableTemplate);
            if (template == null)
                return;
            //获取集合中第一个耦合器模板，并取出DeviceGUID，DeviceGUID是设备在数据库中的GUID值，用于唯一标识一个设备
            string deviceGuid = template.Paramters["DeviceGUID"].Value;
            if (string.IsNullOrEmpty(deviceGuid))
                return;
            //使用DeviceGUID 从数据库中取出设备，并创建设备对象
            IDevice cable = (IDevice)Project.Current.DeviceLibrary.FindDeviceByGuid(deviceGuid);
            if (cable == null || cable.DeviceTypeNew != DeviceTypeNew.Cable)
                return;
            LayoutCable layoutCable = new LayoutCable(fldView, 0, 0, 1, 1, cable);
            Project.Current.SystemDesign.LayoutLayer.Add(layoutCable);
            // 以下方式直接连接两个设备，线缆呈一条直线
            layoutCable.MakeLink(0, srcObj, srcPortIndex);
            layoutCable.MakeLink(1, destObj, destPortIndex);

            //如果中间有需要经过的关键点则插入关键点
            if (midPtList != null && midPtList.Count != 0)
            {
                List<PointF> ptArr = layoutCable.GetActivePList(fldView);
                PointF srcPt = ptArr[0];
                PointF destPt = ptArr[1];
                ptArr.Clear();
                ptArr.Add(srcPt);
                for (int i = 0; i < midPtList.Count; i++)
                {
                    //如果关键点传入的是像素，则不要使用Display2Pixel方法进行转化
                    ptArr.Add(LengthConverter.Instance.Display2Pixel(midPtList[i]));
                }
                ptArr.Add(destPt);
                layoutCable.NotifyMovement();
            }
        }
        #endregion
    }
}
