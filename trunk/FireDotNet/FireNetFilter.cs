// Copyright (c) 2009 Boaz den Besten. All rights reserved.
// Use of this source code is governed by a BSD-style license that can be
// found in the LICENSE file.

using Castle.MonoRail.Framework;

namespace FireDotNet
{
    public class FireNetFilter : IFilter
    {
        #region IFilter Members

        public bool Perform(ExecuteEnum exec, IRailsEngineContext context, Controller controller)
        {
            FireNet.Instance.WriteLog();

            return true;
        }

        #endregion
    }
}
