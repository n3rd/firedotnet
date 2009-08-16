// Copyright (c) 2009 Boaz den Besten. All rights reserved.
// Use of this source code is governed by a BSD-style license that can be
// found in the LICENSE file.

namespace FireDotNet
{
    public class LogException
    {
        public string Class;
        public string Message;
        public string File;
        public string Line;
        public string Type = "throw";
        public object[] Trace;
    }
}
