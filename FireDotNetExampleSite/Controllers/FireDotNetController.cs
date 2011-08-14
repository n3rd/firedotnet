using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NLog;

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
                catch(Exception e)
                {
                    throw new Exception("test exception", e);
                }
            }
            catch(Exception e)
            {
                logger.DebugException("test", e);
            }

            if ("Firefox".Equals(Request.Browser.Browser))
            {
                logger.Debug(Enumerable.Repeat("very long test ", 700).Aggregate((sum, s) => sum + s).ToString());
            }

            if (Request.Headers.AllKeys.Contains("X-FirePHP-Version"))
            {
                logger.Debug("X-FirePHP-Version header found");
            }

            if (Request.UserAgent.Contains("FirePHP"))
            {
                logger.Debug("Modified useragent found");
            }

            logger.Debug("Last test");

            return View();
        }

        private void MethodToTrace()
        {
            logger.Trace("Sample trace message");
        }
    }
}
