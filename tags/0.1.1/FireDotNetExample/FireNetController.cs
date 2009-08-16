using System;
using System.Collections.Generic;
using Castle.MonoRail.Framework;
using FireDotNet;
using System.Data;

namespace FireDotNetExample
{

    [Filter(ExecuteEnum.AfterRendering, typeof(FireNetFilter))]
    public class FireNetController : Controller
    {

        public void Index()
        {
            FireNet.Instance.Log("test Log");
            FireNet.Instance.Warn("test Warn");
            FireNet.Instance.Info("test Info");
            FireNet.Instance.Error("test Error");
            FireNet.Instance.Log("test unicode €");

            FireNet.Instance.Log(
                (new List<string> { "test", "test1", "test2" }).ToArray()
            );

            try
            {
                throw new Exception("Test Exception", new IndexOutOfRangeException("Test InnerExeption"));
            }
            catch (Exception e)
            {
                FireNet.Instance.Exception(e);
            }

            object[][] table = new object[][] {
                new object[] {"test1", "test2"},
                new object[] {"test3", new object[] {"array", "in", "a", "table"}}
            };
            FireNet.Instance.Table("Test table", table);

            DataTable dt = new DataTable("DataTable test");
            dt.Columns.Add(new DataColumn() { ColumnName = "test1" } );
            dt.Columns.Add(new DataColumn() { ColumnName = "test2" } );

            DataRow dr = dt.NewRow();
            dr["test1"] = "test1-1";
            dr["test2"] = "test2-1";
            dt.Rows.Add(dr);

            FireNet.Instance.Table(dt);
        }

    }
}
