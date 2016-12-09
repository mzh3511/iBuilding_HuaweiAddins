/*******************************************************************************
 * Copyright：All rights reserved by Ranplan Co.
 * CLR  版本：4.0.30319.18052
 * 功能 说明：
 * 处理 记录：
 * 1、2014/6/4 15:59:41 MAOZHENHUA-PC created.
 *******************************************************************************/
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using RanOpt.iBuilding.ReportEX;
using Stimulsoft.Report;
using Stimulsoft.Report.Viewer;

namespace HuaweiAddins.Report
{
    public class HuaweiSolutionHandler : ReportHandlerBase
    {
        private DataSet _DstReportData;

        #region Overrides of StiReportBase

        public override void LoadData()
        {
            _DstReportData = new DataSet("iBuildNet DataSet");
            DataTable dt = new DataTable("DTImage");
            _DstReportData.Tables.Add(dt);
            dt.Columns.Add("Name", typeof(string));
            dt.Columns.Add("Image", typeof(byte[]));
            dt.Columns.Add("IsSelected", typeof(bool));

            dt.Rows.Add("Name1", Convert(@"D:\workDir\Test\StiReportTest\bin\Debug\Image\aa.jpg"), true);
            dt.Rows.Add("Name2", Convert(@"D:\workDir\Test\StiReportTest\bin\Debug\Image\bb.png"), false);
            dt.Rows.Add("Name3", Convert(@"D:\workDir\Test\StiReportTest\bin\Debug\Image\cc.png"), true);



            DataSet dstDemo = new DataSet();
            dstDemo.ReadXml(@"D:\workDir\Trunk4\Product\Extensions\AddIns\HuaweiAddins.Report\Demo.xml");
            DataTable dtCountry = dstDemo.Tables["Countries"].Copy();
            dtCountry.TableName = "Country";
            _DstReportData.Tables.Add(dtCountry);
        }
        public byte[] Convert(string imagePath)
        {
            byte[] photo_byte = null;
            using (FileStream fs =
            new FileStream(imagePath, FileMode.Open, FileAccess.Read))
            {
                using (BinaryReader br = new BinaryReader(fs))
                {
                    photo_byte = br.ReadBytes((int)fs.Length);
                }
            }
            return photo_byte;
        }

        public byte[] ConvertImage(Image image)
        {
            MemoryStream ms = new MemoryStream();
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(ms, (object)image);
            ms.Close();
            return ms.ToArray();
        }

        public override void Render(object viewer)
        {
            StiReport report = new StiReport();
            report.RegData(_DstReportData);
            report.Load(ReportPath);
            report.Compile();
            report.Render(true);
            if (viewer is StiViewerControl)
                (viewer as StiViewerControl).Report = report;
        }
        //XXX站点室内分布系统设计方案3.mrt

        #endregion
    }
}
