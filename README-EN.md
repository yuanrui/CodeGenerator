# <img src="https://raw.githubusercontent.com/yuanrui/CodeGenerator/master/Banana.AutoCode/Resources/Icons.128x128.Logo.ico"  height="32px"> AutoCode
[README in Chinese](README.md)

A simple code generator, small and pretty. Used to create front&back end code, assist project development, improve software development efficiency. The code generator write by C#, using the [mono t4 template engine](https://github.com/mono/t4), built-in multiple code templates, can generate the MyBatis, JPA, Asp.Net Mvc, EasyUI, LayUI, Entity Framework, etc.
![main ui](https://user-images.githubusercontent.com/3859838/94229453-20aedd80-ff32-11ea-97dc-6eb6dc1ff315.png)

## User Guide

### Requirements

OS: Windows XP +,  Windows Server 2003 +

Environment: [.NET 4.0 +](https://download.microsoft.com/download/9/5/A/95A9616B-7A37-4AF6-BC36-D6EA96C8DAAE/dotNetFx40_Full_x86_x64.exe)

### Supported databases

- Sql Server

- Oracle

- MySql

- SQLite

### Getting Started

1. Download from [project's website](https://github.com/yuanrui/CodeGenerator) get [the latest version](https://github.com/yuanrui/CodeGenerator/releases);
2. Unzip the file and run: AutoCode.exe;
3. Open Setting windows, set English language environment;
4. Set database connection source and choose data table;
5. Run, code will be generated;
6. Copy the source code from the *Output* folder to your project solution.

Folder purpose: *Config* folder used for save config, *Templates* folder used for save template, *Output* folder used for save generated code.

The generated code depend on class libraries: https://github.com/yuanrui/Examples

### Template Setting

The T4 template files stored in the *Templates* folder. When execute Run, each template file will generate the corresponding source code in *Templates* sub  named folder. Different templates can be classified using folders. The template needs to follow [T4 template syntax](https://docs.microsoft.com/en-us/visualstudio/modeling/code-generation-and-t4-text-templates). The file name ends with .tt or .ttinclude, and a simple template configuration is shown below.

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

host.Table object type is: Table，The include property fields:

| Id   | Name                 | Type          | Description                               |
| :--- | :------------------- | :------------ | ----------------------------------------- |
| 1    | Id                   | String        | database table Id in schema               |
| 2    | Name                 | String        | table name                                |
| 3    | Comment              | String        | table comment                             |
| 4    | DisplayName          | String        | display name, eg: UserEntity, UserManager |
| 5    | Owner                | String        | database                                  |
| 6    | PrimaryKeyIsNumber   | Boolean       |                                           |
| 7    | Columns              | IList<Column> |                                           |
| 8    | PrimaryKeyColumns    | IList<Column> |                                           |
| 9    | NonPrimaryKeyColumns | IList<Column> |                                           |

Column type include property fields:

| Id | Name      | Type | Description |
| :--- | :----------- | :------ | --------------- |
| 1    | Id           | String  | column Id       |
| 2    | Name         | String  | column field name |
| 3    | RawType      | String  | column field raw type |
| 4    | DataType     | DbType  | .NET DbType     |
| 5    | Type         | Type    | .NET Type       |
| 6    | TypeName     | String  | field show type, language base type |
| 7    | Comment      | String  |    |
| 8    | IsPrimaryKey | Boolean |         |
| 9    | IsForeignKey | Boolean |                                     |
| 10   | IsUnique     | Boolean |                                     |
| 11   | IsNullable   | Boolean |                                     |
| 12   | Length       | Int32   |             |
| 13   | Precision    | Int16   |             |
| 14   | Scale        | Int16   |                                     |
| 15   | Index        | Int32   |                                     |
| 16   | Table        | Table   | Table object |

[TODO List](https://github.com/yuanrui/CodeGenerator/issues/1)
