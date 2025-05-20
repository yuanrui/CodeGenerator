//------------------------------------------------------------------------------
// <copyright file="DbViewModel.cs">
//    Copyright (c) 2025, https://github.com/yuanrui All rights reserved.
// </copyright>
// <author>Yuan Rui</author>
// <date>2025-05-08 18:00:00</date>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Banana.AutoCode.DbSchema
{
    public class DbViewModel
    {
        public string Provider { get; set; }

        public string Server { get; set; }

        public int Port { get; set; }

        public string User { get; set; }

        public string Password { get; set; }

        public string Name { get; set; }

        public string Instance { get; set; }
    }
}
