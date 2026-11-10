# 数据库类型图标来源

本目录图标用于 AutoCode 数据库连接树中「连接节点」的数据库类型标识，取自
[DBeaver](https://github.com/dbeaver/dbeaver) 仓库分支 `devel`，取用时的提交为
[`9bf55bc412e5bcfded3da7ea2fe60a6d667eda6c`](https://github.com/dbeaver/dbeaver/tree/9bf55bc412e5bcfded3da7ea2fe60a6d667eda6c)（2026-11-10）。

- 上游许可证：[Apache License 2.0](https://github.com/dbeaver/dbeaver/blob/devel/LICENSE.md)（2026-11-10 通过 GitHub API 确认仓库 `license.spdx_id` 为 `Apache-2.0`）。
- 按 Apache-2.0 要求保留本说明作为版权与出处标注；上游版权归 DBeaver 项目及其贡献者所有。
- 这些图形是各数据库厂商的标识，商标权归各自所有者。本项目仅用于在连接树中标识数据库类型，不表示任何隶属或背书关系。

## 文件清单

| 数据库 | 本仓库文件 | 上游文件（`dbeaver/dbeaver`） | 尺寸 | 转换 |
| --- | --- | --- | --- | --- |
| MySQL | `Icons.16x16.mysql.png` | [`plugins/org.jkiss.dbeaver.ext.mysql/icons/mysql_icon.png`](https://github.com/dbeaver/dbeaver/blob/devel/plugins/org.jkiss.dbeaver.ext.mysql/icons/mysql_icon.png) | 16×16 | 无，原图 |
| MySQL | `Icons.32x32.mysql.png` | [`plugins/org.jkiss.dbeaver.ext.mysql/icons/mysql_icon@2x.png`](https://github.com/dbeaver/dbeaver/blob/devel/plugins/org.jkiss.dbeaver.ext.mysql/icons/mysql_icon@2x.png) | 32×32 | 无，原图 |
| PostgreSQL | `Icons.16x16.postgresql.png` | [`plugins/org.jkiss.dbeaver.ext.postgresql/icons/postgresql_icon.png`](https://github.com/dbeaver/dbeaver/blob/devel/plugins/org.jkiss.dbeaver.ext.postgresql/icons/postgresql_icon.png) | 16×16 | 无，原图 |
| PostgreSQL | `Icons.32x32.postgresql.png` | [`plugins/org.jkiss.dbeaver.ext.postgresql/icons/postgresql_icon@2x.png`](https://github.com/dbeaver/dbeaver/blob/devel/plugins/org.jkiss.dbeaver.ext.postgresql/icons/postgresql_icon@2x.png) | 32×32 | 无，原图 |
| SQLite | `Icons.16x16.sqlite.png` | [`plugins/org.jkiss.dbeaver.ext.sqlite/icons/sqlite_icon.png`](https://github.com/dbeaver/dbeaver/blob/devel/plugins/org.jkiss.dbeaver.ext.sqlite/icons/sqlite_icon.png) | 16×16 | 无，原图 |
| SQLite | `Icons.32x32.sqlite.png` | [`plugins/org.jkiss.dbeaver.ext.sqlite/icons/sqlite_icon@2x.png`](https://github.com/dbeaver/dbeaver/blob/devel/plugins/org.jkiss.dbeaver.ext.sqlite/icons/sqlite_icon@2x.png) | 32×32 | 无，原图 |
| SQL Server | `Icons.16x16.sqlserver.png` | [`plugins/org.jkiss.dbeaver.ext.mssql/icons/mssql_icon.png`](https://github.com/dbeaver/dbeaver/blob/devel/plugins/org.jkiss.dbeaver.ext.mssql/icons/mssql_icon.png) | 16×16 | 无，原图 |
| SQL Server | `Icons.32x32.sqlserver.png` | [`plugins/org.jkiss.dbeaver.ext.mssql/icons/mssql_icon@2x.png`](https://github.com/dbeaver/dbeaver/blob/devel/plugins/org.jkiss.dbeaver.ext.mssql/icons/mssql_icon@2x.png) | 32×32 | 无，原图 |
| Oracle | `Icons.16x16.oracle.png` | [`plugins/org.jkiss.dbeaver.ext.oracle/icons/oracle_icon.png`](https://github.com/dbeaver/dbeaver/blob/devel/plugins/org.jkiss.dbeaver.ext.oracle/icons/oracle_icon.png) | 16×16 | 无，原图 |
| Oracle | `Icons.32x32.oracle.png` | [`plugins/org.jkiss.dbeaver.ext.oracle/icons/oracle_icon@2x.png`](https://github.com/dbeaver/dbeaver/blob/devel/plugins/org.jkiss.dbeaver.ext.oracle/icons/oracle_icon@2x.png) | 32×32 | 无，原图 |

上游同时提供 16 px 与 `@2x`（32 px）PNG，因此无需缩放或重新导出，透明通道与配色保持不变。

## 复现方式

```powershell
$base = 'https://raw.githubusercontent.com/dbeaver/dbeaver/devel/'
Invoke-WebRequest ($base + 'plugins/org.jkiss.dbeaver.ext.mysql/icons/mysql_icon.png')    -OutFile 'Icons.16x16.mysql.png'
Invoke-WebRequest ($base + 'plugins/org.jkiss.dbeaver.ext.mysql/icons/mysql_icon@2x.png') -OutFile 'Icons.32x32.mysql.png'
# 其余四个数据库按上表路径同样下载
```

## 运行时使用

- `Properties/Resources.resx` 只引用 16×16 文件，嵌入为 `Banana.AutoCode.Properties.Resources.Icons_16x16_*`，与仓库原有图标资源方式一致，net40 与 net8 双目标通用。
- 32×32 文件是上游 `@2x` 原图，随仓库保存，供后续高 DPI 场景（ImageList 使用 32 px）替换使用；当前连接树 ImageList 为 16×16。
