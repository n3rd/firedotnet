// Copyright (c) 2009 Boaz den Besten. All rights reserved.
// Use of this source code is governed by a BSD-style license that can be
// found in the LICENSE file.

using System;
using System.Web;
using System.Diagnostics;
using System.Configuration;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Data;
using System.Text;

namespace FireDotNet
{
    public class FireNet
    {

        private List<object[]> _LogEntries = new List<object[]>();

        private bool _WriteLog
        {
            get { return (ConfigurationManager.AppSettings["FireDotNet"] != "disabled"); }
        }

        private FireNet()
        {
        }

        public static FireNet Instance
        {
            get
            {
                if (!HttpContext.Current.Items.Contains("FireNet"))
                {
                    HttpContext.Current.Items["FireNet"] = new FireNet();
                }
                return HttpContext.Current.Items["FireNet"] as FireNet;
            }
        }

        internal void WriteLog()
        {
            if (!_WriteLog)
            {
                return;
            }

            HttpContext.Current.Response.AddHeader("X-Wf-Protocol-1", "http://meta.wildfirehq.org/Protocol/JsonStream/0.2");
            HttpContext.Current.Response.AddHeader("X-Wf-1-Plugin-1", "http://meta.firephp.org/Wildfire/Plugin/FirePHP/Library-FirePHPCore/0.3");
            HttpContext.Current.Response.AddHeader("X-Wf-1-Structure-1", "http://meta.firephp.org/Wildfire/Structure/FirePHP/FirebugConsole/0.1");

            for (int i = 1; i <= _LogEntries.Count; i++)
            {
                string json = JavaScriptConvert.SerializeObject(_LogEntries[i - 1]);
                HttpContext.Current.Response.AddHeader("X-Wf-1-1-1-" + i, String.Format("{0}|{1}|", Encoding.UTF8.GetByteCount(json), json));
            }
        }

        private void AddLog(object message, LogType logType)
        {
            StackFrame callStack = new StackFrame(2, true);

            _LogEntries.Add(
                new object[2] { 
                    new LogHeaders() { 
                        Type = logType.ToString(), 
                        File = callStack.GetFileName(), 
                        Line = callStack.GetFileLineNumber() 
                    }, 
                    message 
                }
            );
        }

        public void Log(object message)
        {
            AddLog(message, LogType.LOG);
        }

        public void Info(object message)
        {
            AddLog(message, LogType.INFO);
        }

        public void Warn(object message)
        {
            AddLog(message, LogType.WARN);
        }

        public void Error(object message)
        {
            AddLog(message, LogType.ERROR);
        }

        public void Exception(Exception e)
        {
            StackTrace stackTrace;

            List<object> trace = new List<object>();
            Exception cE = e;
            while (cE.InnerException != null)
            {
                stackTrace = new StackTrace(cE, true);
                cE = cE.InnerException;

                trace.Add(
                    new {
                        file = stackTrace.GetFrame(0).GetFileName(),
                        line = stackTrace.GetFrame(0).GetFileLineNumber().ToString(),
                        function = cE.GetType().Name, 
                        args = new object[] { cE.Message.ToString() } 
                    }
                );
            }

            stackTrace = new StackTrace(e, true);
            _LogEntries.Add(
                new object[2] { 
                    new LogHeaders() { 
                        Type = LogType.EXCEPTION.ToString() 
                    }, 
                    new LogException() { 
                        Class = e.GetType().Name,
                        Message = e.Message,
                        File = stackTrace.GetFrame(0).GetFileName(),
                        Line = stackTrace.GetFrame(0).GetFileLineNumber().ToString(),
                        Trace = trace.ToArray()
                    }
                }
           );
        }

        public void Table(string label, object[][] table)
        {
            _LogEntries.Add(
                new object[2] { 
                    new LogHeaders() { 
                        Type = LogType.TABLE.ToString(),
                        Label = label
                    }, 
                    table
                }
           );
        }

        public void Table(DataTable dataTable)
        {
            List<object[]> table = new List<object[]>();

            List<string> header = new List<string>();
            foreach (DataColumn dc in dataTable.Columns)
            {
                header.Add(dc.ColumnName);
            }
            table.Add(header.ToArray());

            foreach (DataRow dr in dataTable.Rows)
            {
                List<string> row = new List<string>();
                foreach (DataColumn dc in dataTable.Columns)
                {
                    row.Add(dr[dc.ColumnName].ToString());
                }
                table.Add(row.ToArray());
            }

            Table(String.IsNullOrEmpty(dataTable.TableName) ? "DataTable" : dataTable.TableName, table.ToArray());
        }
    }
}
