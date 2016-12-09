using System;
using System.AddIn;
using System.Drawing;
using RanOpt.iBuilding.BLL;
using RanOpt.iBuilding.BSMModule;
using RanOpt.iBuilding.DiagrammingCore;
using RanOpt.iBuilding.Extensions.AddInView;

namespace HuaweiAddins.TestRegion
{
    [AddIn("HuaweiAddVWall", Version = "1.0.0.0", Publisher = "zhenhua.mao", Description = "Huawei Add VWall")]
    public class HuaweiAddVWall : IRibbonMenuAV
    {
        private MainHostAV _Host;

        #region Implementation of IRibbonMenuAV

        public void Init(MainHostAV host)
        {
            _Host = host;
            host.AppendRibbonMenu("Huawei", "AddVWall", null, null);
        }

        public void OnMenuInvoked(object tag)
        {
            if (Project.Current == null)
                return;
            var bsmView = _Host.GetActiveView() as DiagrammingControl;
            if (bsmView == null)
                return;

            //以下变量请自行设置，坐标单位是像素，高度厚度单位为米
            var pt1 = new PointF(0, 0);
            var pt2 = new PointF(100, 50);
            var height = 2f;
            var thickness = 0.1f;
            var planType = PlaneType.Wall;
            var material = Project.Current.MaterialLibrary.Materials[0];

            using (new DisploseOfCommand(bsmView.Controller, "AddVWall"))
            {

                try
                {
                    //实例化垂直墙
                    VPlaneShape newWallShape = new VPlaneShape(bsmView.Controller.Model);
                    newWallShape.SuspendSync();
                    newWallShape.Type = planType;
                    newWallShape.Thickness = thickness;
                    newWallShape.Height = height;
                    newWallShape.Start = pt1;
                    newWallShape.StartTop = height;
                    newWallShape.StartBottom = 0f;
                    newWallShape.End = pt2;
                    newWallShape.EndTop = height;
                    newWallShape.EndBottom = 0f;
                    newWallShape.Material = material;
                    newWallShape.ResumeSync(false);

                    //添加墙体到当前视图
                    bsmView.Model.AddShape(newWallShape);
                }
                finally
                {
                    bsmView.Invalidate();
                }
            }
        }
        #endregion


        public sealed class DisploseOfCommand : IDisposable
        {
            private readonly IController _controller;

            public DisploseOfCommand(IController controller, string commandName)
            {
                _controller = controller;
                if (!_controller.UndoManager.IsCommandStarted)
                    _controller.UndoManager.Start(commandName);
            }

            public void Dispose()
            {
                if (_controller.UndoManager.IsCommandStarted)
                    _controller.UndoManager.Commit();
            }
        }
    }
}
