using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace NLog.Targets.FireDotNet
{

    internal static class BrowserSupport
    {

        private static readonly int _K = 1024;

        private static readonly int _M = _K * 1024;

        private static readonly int _5K = 5 * _K;

        private static readonly int _64K = 64 * _K;

        private static readonly int _16M = 16 * _M;

        /// <summary>
        /// Gets the max length for a single header
        /// </summary>
        /// <param name="browser">The HttpBrowserCapabilities object for the user's browser</param>
        /// <returns>The max number of bytes supported for the user's browser</returns>
        public static int GetMaxHeaderLength(HttpBrowserCapabilitiesBase browser)
        {
            if (browser.Type.ToUpperInvariant().Contains("FIREFOX"))
            {
                return _5K;
            }

            // even if there is no limit for a single line,
            //  there still is a limit for the total header length
            return GetMaxHeaderSize(browser);
        }

        /// <summary>
        /// Gets the max total header length
        /// </summary>
        /// <remarks>
        /// Information source: http://code.google.com/p/chromium/issues/detail?id=22928
        /// </remarks>
        /// <param name="browser">The HttpBrowserCapabilities object for the user's browser</param>
        /// <returns>The max number of bytes supported for the user's browser</returns>
        public static int GetMaxHeaderSize(HttpBrowserCapabilitiesBase browser)
        {
            if (browser.Type.ToUpperInvariant().Contains("FIREFOX")
                && browser.MajorVersion >= 3)
            {
                return _16M;
            }

            return _64K; // just to be safe
        }

    }
}
