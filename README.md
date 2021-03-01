# <img src="https://raw.githubusercontent.com/yuanrui/CodeGenerator/master/Banana.AutoCode/Resources/Icons.128x128.Logo.ico"  height="32px"> AutoCode
[README in English](README-EN.md)

一个小而美的代码生成器，用于创建前后端代码，辅助项目开发，提高软件开发效率。代码生成器采用C#语言编写，采用 [mono t4模板引擎](https://github.com/mono/t4)，内置多个代码模板，可生成MyBatis、JPA、Asp.Net Mvc、EasyUI、LayUI、Entity Framework等代码。
![main ui](https://user-images.githubusercontent.com/3859838/87500061-367c6880-c68e-11ea-8506-4a9683413402.png)

## 用户指南

### 系统要求

操作系统：Windows XP +,  Windows Server 2003 +

软件环境：[.NET 4.0 +](https://download.microsoft.com/download/9/5/A/95A9616B-7A37-4AF6-BC36-D6EA96C8DAAE/dotNetFx40_Full_x86_x64.exe)

### 支持数据库

- Sql Server

- Oracle

- MySql

- SQLite

### 使用说明

1. 从[项目首页](https://github.com/yuanrui/CodeGenerator)下载[最新版本](https://github.com/yuanrui/CodeGenerator/releases)；
2. 解压文件，运行AutoCode.exe；
3. 设置数据源，勾选数据表；
4. 运行生成代码；
5. 拷贝Output文件夹中的源码到项目解决方案。

文件夹用途说明：Config存放配置文件，Templates存放模板文件，Output存放生成的代码。

生成的源码所依赖的类库见：https://github.com/yuanrui/Examples

### 模板配置

用于代码生成的T4模板文件约定存放在Templates文件夹，生成代码时每个模板文件将生成对应的源代码，不同的模板可以使用文件夹进行归类。模板需要遵循[T4模板语法](https://docs.microsoft.com/zh-cn/visualstudio/modeling/code-generation-and-t4-text-templates)，文件命名以.tt或.ttinclude结尾，一个简单的模板配置如下所示。

```c#
<#@ template language="C#" hostSpecific="true" debug="false" #>
<#@ output encoding="utf-8" extension=".cs" #>
<#@ include file="../TemplateFileManager.ttinclude" #>
<# 
	CustomHost host = (CustomHost)(Host);
	Table table = host.Table;
    var manager = Manager.Create(host, GenerationEnvironment);
	manager.StartNewFile(table.DisplayName + "Entity.generated.cs", host.GetValue("OutputPath").ToString() + "\\Samples\\Generated");
#>
//------------------------------------------------------------------------------
// <copyright file="<#= table.DisplayName #>Entity.cs">
//    Copyright (c) <#= DateTime.Now.ToString("yyyy") #>, All rights reserved.
// </copyright>
// <author><#= Environment.UserName #></author>
// <date><#= DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") #></date>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;

namespace Banana.Data
{
    /// <summary>
    /// <#= string.IsNullOrEmpty(table.Comment) ? string.Format("Table/View [{0}] map to [{1}] entity class", table.Name, table.DisplayName) : table.Comment #>
    /// </summary>
    public partial class <#= table.DisplayName #>Entity : ICloneable
    {
    <# 
        foreach(var column in table.Columns)
        {
    #>
        /// <summary>
        /// get or set <#= column.Comment #>
        /// </summary>
        public virtual <#= column.TypeName #> <#= column.Name #> { get; set; }

    <#
        }
    #>
        public virtual <#= table.DisplayName #>Entity CloneFrom(<#= table.DisplayName #>Entity thatObj)
        {
            if (thatObj == null)
            {
                throw new ArgumentNullException("thatObj");
            }

    <# 
        foreach(var column in table.Columns)
        {
    #>
            this.<#= column.Name #> = thatObj.<#= column.Name #>;
    <#
        }
    #>

            return this;
        }

        public virtual <#= table.DisplayName #>Entity CloneTo(<#= table.DisplayName #>Entity thatObj)
        {
            if (thatObj == null)
            {
                throw new ArgumentNullException("thatObj");
            }

    <# 
        foreach(var column in table.Columns)
        {
    #>
            thatObj.<#= column.Name #> = this.<#= column.Name #>;
    <#
        }
    #>

            return thatObj;
        }

        public virtual <#= table.DisplayName #>Entity Clone()
        {
            var thatObj = new <#= table.DisplayName #>Entity();

            return this.CloneTo(thatObj);
        }

        object ICloneable.Clone()
        {
            return this.Clone();
        }
    }
}
<# 
	manager.EndBlock(); 
#>
```

host.Table对象的类型为Table，包含属性字段如下:

| 序号 | 名称                 | 类型          | 说明             |
| :--- | :------------------- | :------------ | ---------------- |
| 1    | Id                   | String        | 编号             |
| 2    | Name                 | String        | 表名称           |
| 3    | Comment              | String        | 表注释           |
| 4    | DisplayName          | String        | 显示名称         |
| 5    | Owner                | String        | 数据库           |
| 6    | PrimaryKeyIsNumber   | Boolean       | 主键是否数字类型 |
| 7    | Columns              | IList<Column> | 列集合           |
| 8    | PrimaryKeyColumns    | IList<Column> | 主键列集合       |
| 9    | NonPrimaryKeyColumns | IList<Column> | 非主键列集合     |

Column类型包含属性字段如下：

| 序号 | 名称         | 类型    | 说明         |
| :--- | :----------- | :------ | ------------ |
| 1    | Id           | String  | 编号         |
| 2    | Name         | String  | 字段名称     |
| 3    | RawType      | String  | 字段原始类型 |
| 4    | DataType     | DbType  | .NET DbType  |
| 5    | Type         | Type    | .NET Type    |
| 6    | TypeName     | String  | 字段类型名称 |
| 7    | Comment      | String  | 字段注释     |
| 8    | IsPrimaryKey | Boolean | 是否主键     |
| 9    | IsForeignKey | Boolean | 是否外键     |
| 10   | IsUnique     | Boolean | 是否唯一     |
| 11   | IsNullable   | Boolean | 是否可为空   |
| 12   | Length       | Int32   | 长度         |
| 13   | Precision    | Int16   | 精度         |
| 14   | Scale        | Int16   | 精度范围     |
| 15   | Index        | Int32   | 索引编号     |
| 16   | Table        | Table   | Table对象    |

[TODO List](https://github.com/yuanrui/CodeGenerator/issues/1)