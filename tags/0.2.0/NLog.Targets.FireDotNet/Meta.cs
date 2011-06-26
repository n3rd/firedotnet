// Copyright (c) 2011 Boaz den Besten. All rights reserved.
// Use of this source code is governed by a BSD-style license that can be
// found in the LICENSE file.

using System;
using System.Web.Script.Serialization;

namespace NLog.Targets.FireDotNet
{

    class Meta
    {

        [ScriptIgnore]
        public LogLevel Level { get; set; }

        public string Type
        {
            get { return Level.ToString(); }
        }

        public string File { get; set; }
        
        public int Line { get; set; }

        public string Label { get; set; }

    }
}
