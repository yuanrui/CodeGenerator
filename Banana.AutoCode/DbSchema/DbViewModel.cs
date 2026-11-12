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

        /// <summary>
        /// SQL Server 具名实例（Data Source 中 <c>host\instance</c> 的 instance 部分）。
        /// <see cref="Instance"/> 在各 Provider 下另作库名/服务名/文件路径使用，二者不可混用。
        /// </summary>
        public string ServerInstance { get; set; }
    }
}
