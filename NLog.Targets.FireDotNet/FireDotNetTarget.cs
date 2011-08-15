// Copyright (c) 2011 Boaz den Besten. All rights reserved.
// Use of this source code is governed by a BSD-style license that can be
// found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using System.Linq;
using System.Collections;

namespace NLog.Targets.FireDotNet
{

    [Target("FireDotNet")]
    public sealed class FireDotNetTarget : TargetWithLayout
    {
        private static JavaScriptSerializer JSSerializer = new JavaScriptSerializer();

        private static Dictionary<FireDotNet.LogLevel, NLog.LogLevel[]> LevelMapper = new Dictionary<FireDotNet.LogLevel, NLog.LogLevel[]>() 
        { 
            { LogLevel.TRACE, new[] { NLog.LogLevel.Trace } },
            { LogLevel.LOG, new[] { NLog.LogLevel.Debug } },
            { LogLevel.INFO, new[] { NLog.LogLevel.Info } },
            { LogLevel.WARN, new[] { NLog.LogLevel.Warn } },
            { LogLevel.ERROR, new[] { NLog.LogLevel.Error, NLog.LogLevel.Fatal } },
        };

        private int MessageCount
        {
            get
            {
                if (!HttpContext.Current.Items.Contains("FireDotNet_MessageCount"))
                {
                    MessageCount = 1;
                }

                return (int)HttpContext.Current.Items["FireDotNet_MessageCount"];
            }
            set
            {
                HttpContext.Current.Items["FireDotNet_MessageCount"] = value;
            }
        }

        private int TotalHeaderSize
        {
            get
            {
                if (!HttpContext.Current.Items.Contains("FireDotNet_TotalHeaderSize"))
                {
                    TotalHeaderSize = 0;
                }

                return (int)HttpContext.Current.Items["FireDotNet_TotalHeaderSize"];
            }
            set
            {
                HttpContext.Current.Items["FireDotNet_TotalHeaderSize"] = value;
            }
        }

        private bool IsConsoleEnabled(HttpRequestBase request)
        {
            return request.Headers.AllKeys.Contains("X-FirePHP-Version") || request.UserAgent.Contains("FirePHP");
        }

        public FireDotNetTarget()
            : base()
        {
            // realy ugly hack to get the stacktrace information
            Layout = "${callsite:fileName=true}";
        }

        protected override void Write(LogEventInfo logEvent)
        {
            HttpContextBase httpContext = new HttpContextWrapper(HttpContext.Current);
            HttpResponseBase response = httpContext.Response;

            if (MessageCount < 2 && !IsConsoleEnabled(httpContext.Request))
            {
                return;
            }
            else if (MessageCount < 2)
            {
                response.AppendHeader("X-Wf-Protocol-1", "http://meta.wildfirehq.org/Protocol/JsonStream/0.2");
                response.AppendHeader("X-Wf-1-Plugin-1", "http://meta.firephp.org/Wildfire/Plugin/FirePHP/Library-FirePHPCore/0.3");
                response.AppendHeader("X-Wf-1-Structure-1", "http://meta.firephp.org/Wildfire/Structure/FirePHP/FirebugConsole/0.1");
            }

            LogLevel logLevel = logEvent.Exception != null ? LogLevel.EXCEPTION : LevelMapper.Single(kvp => kvp.Value.Contains(logEvent.Level)).Key;

            object body;
            switch (logLevel)
            {
                case LogLevel.EXCEPTION:
                    body = BodyException(logEvent);
                    break;
                case LogLevel.TRACE:
                    body = BodyTrace(logEvent);
                    break;
                default:
                    body = BodyMessage(logEvent);
                    break;
            }

            string json = JSSerializer.Serialize(new object[] {
                new Meta { 
                    Level = logLevel,
                    File = logEvent.HasStackTrace ? logEvent.UserStackFrame.GetFileName() : String.Empty,
                    Line = logEvent.HasStackTrace ? logEvent.UserStackFrame.GetFileLineNumber() : 0
                },
                body
            });

            
            int byteCount = Encoding.UTF8.GetByteCount(json);
            TotalHeaderSize += byteCount;
            if (TotalHeaderSize >= BrowserSupport.GetMaxHeaderSize(httpContext.Request.Browser))
            {
                throw new Exception("Logging disabled due to header size limitations");
            }

            int pos = 0,
                maxHeaderLength = BrowserSupport.GetMaxHeaderLength(httpContext.Request.Browser);
            while (pos < json.Length)
            {
                string data = (pos == 0 ? byteCount.ToString() : "") + "|" + new String(json.Skip(pos).Take(maxHeaderLength).ToArray()) + "|" + ((pos += maxHeaderLength) < json.Length ? @"\" : "");
                response.AppendHeader("X-Wf-1-1-1-" + (MessageCount++), data);
            }
        }

        private object BodyException(LogEventInfo logEvent)
        {
            StackTrace stackTrace;

            List<object> trace = new List<object>();
            Exception exception = logEvent.Exception;
            while (exception.InnerException != null)
            {
                exception = exception.InnerException;
                stackTrace = new StackTrace(exception, true);

                trace.Add(new
                {
                    file = stackTrace.GetFrame(0).GetFileName(),
                    line = stackTrace.GetFrame(0).GetFileLineNumber(),
                    function = exception.GetType().Name,
                    args = new object[] { exception.Message.ToString() }
                });
            }

            stackTrace = new StackTrace(logEvent.Exception, true);

            return new
            {
                Class = logEvent.Exception.GetType().Name,
                Message = logEvent.Exception.Message,
                File = stackTrace.GetFrame(0).GetFileName(),
                Line = stackTrace.GetFrame(0).GetFileLineNumber(),
                Type = "throw",
                Trace = trace.ToArray()
            };
        }

        private object BodyTrace(LogEventInfo logEvent)
        {
            var trace = new List<object>();

            int framenr = logEvent.UserStackFrameNumber;
            while (++framenr < logEvent.StackTrace.FrameCount - 1)
            {
                StackFrame frame = logEvent.StackTrace.GetFrame(framenr);
                trace.Add(new
                {
                    function = (frame.GetMethod().DeclaringType == null ? String.Empty : frame.GetMethod().DeclaringType.FullName + ".") + frame.GetMethod().Name,
                    file = frame.GetFileName(),
                    line = frame.GetFileLineNumber()
                });
            }

            return new
            {
                Type = String.Empty,
                Class = logEvent.UserStackFrame.GetMethod().DeclaringType == null ? String.Empty : logEvent.UserStackFrame.GetMethod().DeclaringType.FullName + ".",
                Function = logEvent.UserStackFrame.GetMethod().Name,
                Message = logEvent.FormattedMessage,
                File = logEvent.UserStackFrame.GetFileName(),
                Line = logEvent.UserStackFrame.GetFileLineNumber(),
                Trace = trace
            };
        }

        private string BodyMessage(LogEventInfo logEvent)
        {
            return logEvent.FormattedMessage;
        }

    }
}
