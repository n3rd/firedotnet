using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NLog;
using System.Text;

namespace FireDotNetExampleSite.Controllers
{
    public class FireDotNetController : Controller
    {

        private static Logger logger = LogManager.GetCurrentClassLogger();

        public ActionResult Index()
        {
            MethodToTrace();
            logger.Debug("Sample debug message");
            logger.Info("Sample informational message");
            logger.Warn("Sample warning message");
            logger.Error("Sample error message");
            logger.Fatal("Sample fatal error message");

            try
            {
                try
                {
                    try
                    {
                        int i = 0;
                        var x = 5 / i;
                    }
                    catch (Exception e)
                    {
                        throw new Exception("inner excpetion", e);
                    }
                }
                catch (Exception e)
                {
                    throw new Exception("test exception", e);
                }
            }
            catch (Exception e)
            {
                logger.DebugException("test", e);
            }

            if (Request.Headers.AllKeys.Contains("X-FirePHP-Version"))
            {
                logger.Debug("X-FirePHP-Version header found");
            }

            if (Request.UserAgent.Contains("FirePHP"))
            {
                logger.Debug("Modified useragent found");
            }

            string longMessage = "48K test message {0} ";
            StringBuilder sb = new StringBuilder();
            for (int i = 0, length = longMessage.Length; i < 48 * 1024; i += length)
            {
                sb.Append(longMessage);
            }
            logger.Debug(String.Format(sb.ToString(), 1)); // write a 48K message
            logger.Debug(String.Format(sb.ToString(), 2)); // write another 48K message

            logger.Debug("Last test");

            return View();
        }

        private void MethodToTrace()
        {
            logger.Trace("Sample trace message");
        }
    }
}
