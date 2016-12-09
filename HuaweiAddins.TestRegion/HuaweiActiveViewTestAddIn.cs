using System;
using System.AddIn;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
//using RanOpt.Util;
using HuaweiAddins.TestRegion.Properties;
using RanOpt.iBuilding.BLL;
using RanOpt.iBuilding.BSMModule;
using RanOpt.iBuilding.Extensions.AddInView;
using RanOpt.iBuilding.LayoutModel;
using RanOpt.iBuilding.Model;

namespace HuaweiAddins.TestRegion
{
    [AddIn("GetActiveView", Publisher = "zhenhua.mao", Version = "1.0.0.0", Description = "测试GetActiveView()接口")]
    public class HuaweiActiveViewTestAddIn : IRibbonMenuAV
    {
        private MainHostAV _Host;
        public void Init(MainHostAV host)
        {
            //记住插件的载体
            _Host = host;
            //注册插件的响应按钮（该按钮会出现在主界面的工具栏Addin页里面）
            host.AppendRibbonMenu(null, "GetActiveView", Resources.Export, null);
            //AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }

        Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            return Assembly.Load(args.Name);
        }

        public void OnMenuInvoked(object tag)
        {
            //按钮响应时最好先判断一下当前是否已打开项目
            if (Project.Current == null)
            {
                //Form1 form1 = new Form1();
                //form1.ShowDialog();

                MessageBox.Show("There is no project opened! Please open a project before using this tool.");
                return;
            }
            //通过MainHostAV.GetActiveView()获取当前活动窗体（非属性窗口、项目树窗口等）的视图
            //如果是FLD、NSD，该方法则返回对应的布局视图BaseLayoutView
            //如果是BSM，该方法则返回对应的画布DiagrammingControl
            //其它情况如3D、或者所有窗口都已关闭则返回null
            Control activeControl = _Host.GetActiveView();
            if (activeControl != null)
            {
                int floorIndex = -1;
                string buildingName = string.Empty;
                string activeMode = string.Empty;
                BaseLayoutView fldView = activeControl as BaseLayoutView;
                if (fldView != null)
                {
                    //FLD视图中，可以通过BaseLayoutView.BuildingName和FloorIndex获取当前的楼层信息
                    //然后使用Project.Current.FindBuildingByName(buildingName)获取当前视图对应的建筑对象
                    //并使用Building.GetFloorByIndex(floorIndex)获取当前视图对应的楼层对象

                    activeMode = fldView.LayMode.ToString();
                    buildingName = fldView.BuildingName;
                    floorIndex = fldView.FloorIndex;
                    //如果fldView.LayMode=LayoutMode.LayoutSD，则BuildingName为空，FloorIndex=0
                }
                DiagrammingControl bsmControl = activeControl as DiagrammingControl;
                if (bsmControl != null)
                {
                    //BSM视图时，可以通过DiagrammingControl.BsmModel.Floor获取当前的楼层对象
                    //如需当前视图的建筑，可使用IFloor.ParentBuilding获取建筑对象
                    activeMode = "BSM";
                    IFloor floor = bsmControl.BsmModel.Floor;
                    if (floor != null)
                    {
                        buildingName = floor.ParentBuilding.Name;
                        floorIndex = floor.FloorIndex;
                    }
                }
                string info = string.Format("Mode={0} BuildingName={1} FloorIndex={2}", activeMode, buildingName,
                                            floorIndex);
                MessageBox.Show(info, "当前活动视图信息");
            }

            /**对_Host.GetActiveView()的后续使用：
             * 1、判断天线是否在区域范围内
             * 2、获取线缆两端的设备
             * */

            BaseLayoutView layoutView = _Host.GetActiveView() as BaseLayoutView;
            if (layoutView == null || layoutView.LayMode != LayoutMode.LayoutLD)
            {
                MessageBox.Show("要使用BaseLayoutView请切换到FLD视图");
                return;
            }
            else
            {
                List<LayoutObject> objList = layoutView.LayoutLayer.GetSelectedObject(layoutView);

                /*后续使用1、判断天线是否在区域范围内
                 * */
                string errorMsg = "要判断天线是否在区域范围内，必须选择两个对象，一个天线一个区域";
                if (objList.Count != 2)
                {
                    MessageBox.Show(errorMsg);
                }
                else
                {
                    LayoutAnt layoutAnt = null;
                    LayoutPolygonRegion layoutPolygonRegion = null;
                    foreach (LayoutObject layoutObject in objList)
                    {
                        if (layoutObject is LayoutAnt)
                            layoutAnt = layoutObject as LayoutAnt;
                        if (layoutObject is LayoutPolygonRegion)
                            layoutPolygonRegion = layoutObject as LayoutPolygonRegion;
                    }
                    if (layoutAnt == null || layoutPolygonRegion == null)
                    {
                        MessageBox.Show(errorMsg);
                    }
                    else
                    {
                        PointF antPt2f = layoutAnt.GetCenterPoint(layoutView.LayMode);
                        //注意，天线的高度原本应该用layoutAnt.Height来获取（单位米），但由于区域的默认高度为1米，
                        //这里直接使用1米来代替
                        Point3F antPt3f = new Point3F(antPt2f.X, antPt2f.Y, 1f * layoutView.LayoutLayer.PixelPerUint);
                        bool antIsInRegion2D = layoutPolygonRegion.PointInObject(antPt2f);
                        MessageBox.Show("在2D视图中，天线在区域中：" + antIsInRegion2D);
                        bool antIsInRegion3D = layoutPolygonRegion.IsPointInRegion(antPt3f);
                        MessageBox.Show("在3D视图中，天线在区域中：" + antIsInRegion3D);
                    }
                }

                /*后续使用2、获取线缆两端连接的设备
                 * */
                errorMsg = "要获取线缆两端的设备，必须并只能选择一个线缆";
                if (objList.Count != 1)
                {
                    MessageBox.Show(errorMsg);
                }
                else
                {
                    LayoutCable layoutCable = objList[0] as LayoutCable;
                    if (layoutCable == null)
                    {
                        MessageBox.Show(errorMsg);
                    }
                    else
                    {
                        LayoutDevice layoutDevice0 = layoutCable.NodeLink[0].obj as LayoutDevice;
                        LayoutDevice layoutDevice1 = layoutCable.NodeLink[1].obj as LayoutDevice;
                        if (layoutDevice0 != null)
                            MessageBox.Show(string.Format("选中的线缆一端连接：Name={0},DisplayName={1}", layoutDevice0.Name,
                                                          layoutDevice0.Name));
                        else
                            MessageBox.Show("选中的线缆一端连接：未连接");
                        if (layoutDevice1 != null)
                            MessageBox.Show(string.Format("选中的线缆另一端连接：Name={0},DisplayName={1}", layoutDevice1.Name,
                                                          layoutDevice1.Name));
                        else
                            MessageBox.Show("选中的线缆另一端连接：未连接");
                    }
                }

            }

            //插件主体功能

        }
    }
}
