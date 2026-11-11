# 连接字符串格式说明

> 文档状态：与当前代码一致的使用说明。
>
> 事实基线：`Banana.AutoCode/Core/DbConnectionStringFormats.cs`（连接字符串拼装）、`Banana.AutoCode/Core/DbConnectionInfoFormatter.cs`（人读连接信息清单）与 `Banana.AutoCode/Forms/DbPanel.cs`（右键菜单入口）。文中示例均由实现实际生成，可与代码逐条对照。

## 目录

- [1. 功能入口](#entry)
- [2. 密码与敏感信息](#password)
- [3. 占位符与待填写项](#placeholder)
- [4. 适用性矩阵](#matrix)
- [5. 各格式拆解](#formats)
- [6. 编码与转义规则](#encoding)
- [7. 连接信息清单（连接节点）](#info)
- [8. 已知限制](#limits)
- [9. 菜单的边界行为](#edge)
- [10. 与实现的对应关系](#impl)

<a id="entry"></a>

## 1. 功能入口

数据库面板的树有三层：连接节点（顶级，显示连接名）→ 数据库节点 → 表节点。两个菜单分别挂在两层节点上，用途不同：

| 节点 | 菜单 | 产物 | 面向 |
| --- | --- | --- | --- |
| 连接节点 | 复制连接信息 / Copy connection info | 一行一项的纯文本清单 | 给人阅读、转发分享 |
| 数据库节点 | 复制连接字符串 / Copy connection string | 九种生态/协议的连接字符串 | 给程序使用 |

连接字符串菜单下列出九种格式；点击可用项即写入剪贴板。反馈为跟随鼠标的浮动提示（约 2.5 秒），同时写入输出面板日志，不弹模态对话框。

连接配置取自被右键的数据库节点所属的连接（根节点的 `Tag`），库名取自数据库节点自身，不依赖「当前选中连接」这类隐式状态。连接节点只提供「复制连接信息」，表节点不提供任何菜单。

<a id="password"></a>

## 2. 密码与敏感信息

- **存储形式：明文。** 界面保存连接时，`DbConnBuilderPanel.AddOrUpdateConnectionStrings` 直接调用 `ConfigurationManager.OpenExeConfiguration(...).Save(...)`，把 `DbProviderConfig.ToSettings` 生成的连接串写入 `Config/db.config`，密码字段未做任何加密或混淆。读取侧 `DbViewExtension.ToXxxModel` 直接取 `builder.Password`，因此拼装时拿到的就是原始明文。
- **连接字符串与信息清单都默认含真实密码**，否则复制出来的内容无法直接使用。这意味着剪贴板、终端历史、日志中都可能留下明文密码，请自行控制使用范围。
- **反馈与日志只写格式名或模式**（例如「已复制：JDBC URL」「已复制连接信息（含明文密码）」），不会把含密码的正文写入输出面板或 `Trace`。
- 连接节点的信息清单额外提供「不含密码」变体（见第 7 节），用于贴到工单、群聊等不需要凭据的场合。
- 界面保存的账号密码本身就以明文存在于 `Config/db.config`，因此本功能没有引入新的泄露面，只是把同一份信息复制到剪贴板。

<a id="placeholder"></a>

## 3. 占位符与待填写项

连接信息不足以拼出某一段时，不灰显、不留空，而是把该段写成 `[提示词]`，用户替换中括号内容即可直接使用。

### 3.1 三种状态

| 状态 | 表现 | 判定 |
| --- | --- | --- |
| 可生成且完整 | 输出真实值，无提示 | 所需字段齐全 |
| 可生成但缺字段 | 输出带 `[...]` 的字符串，菜单项后缀「（含 N 处待填写）」，复制提示里重复 N | 字段为空，但格式对该类型适用 |
| 该格式对当前数据库类型语义不存在 | 菜单项灰显，悬停提示「当前类型不支持此格式」 | `IsSupported` 返回 false |

第 3 种不是「缺数据」，而是「语义不存在」：Go DSN 对 SQL Server、Rust `DATABASE_URL` 对 Oracle、OLE DB 对 MySQL/PostgreSQL/SQLite、SQLAlchemy 对 Oracle、Node.js/Rust 对 SQLite 都属于这类，任何字段都补齐也不会生成。

### 3.2 占位符对照表

| 本地化键 | 中文 | 英文 | 出现在 |
| --- | --- | --- | --- |
| `ConnectionString_Placeholder_Host` | `[主机]` | `[host]` | 主机/服务器地址为空 |
| `ConnectionString_Placeholder_Database` | `[数据库]` | `[database]` | 库名为空（Oracle 之外） |
| `ConnectionString_Placeholder_ServiceName` | `[SID/服务名]` | `[SID/service name]` | Oracle 的 SERVICE_NAME 为空 |
| `ConnectionString_Placeholder_User` | `[用户名]` | `[user name]` | 账号为空 |
| `ConnectionString_Placeholder_InstanceName` | `[实例名]` | `[instance name]` | SQL Server 写了 `host\` 但实例名为空 |
| `ConnectionString_Placeholder_FilePath` | `[文件路径]` | `[file path]` | SQLite 文件路径为空 |
| `ConnectionString_Placeholder_Password` | `[密码]` | `[password]` | 仅连接信息清单（第 7 节） |

占位词由 `Localization.resx`（英文，中性）/`Localization.zh-CN.resx`（中文）提供，跟随当前 UI 语言；格式类只持有键名，不写死文案。

### 3.3 替换步骤

1. 复制后把 `[主机]`、`[数据库]` 等整体替换为真实值，保留中括号外的分隔符与引号/花括号。
2. 替换后按所在协议的规则编码（见第 6 节）：
   - URL 风格（JDBC 的库名、SQLAlchemy、Node.js、Go 的 PostgreSQL、Rust）：空格写成 `%20`，`\` 写成 `%5C`，`@` 写成 `%40`，依此类推；
   - ADO.NET 键值对：值里有 `;`、`=`、引号或首尾空白时加引号；
   - ODBC/OLE DB：值保持在 `{...}` 内，内部 `}` 写成 `}}`；
   - Go 的 MySQL DSN、PHP PDO DSN：原样插入，不编码。
3. 占位符本身**不做编码**，保持可读；所以原文里的 `[实例名]` 不会被写成 `%5B实例名%5D`。用户输入的字符串里若本身含 `[`/`]`，按上表规则照常编码或原样保留，不会影响占位符识别——占位符个数在拼装时累加，不靠事后扫描文本。

### 3.4 密码为空

- **连接字符串**：密码为空视为合法空密码（集成认证、信任认证、无密码库），输出 `Password=;`、`PWD={};`，URL 风格省略 `:password` 段，不产生占位符。若目标环境其实需要密码，请自行补 `Password=`。
- **连接信息清单**：给人看时空密码无法区分「没有密码」和「忘了填」，因此统一输出 `[密码]` 占位符。

<a id="matrix"></a>

## 4. 适用性矩阵

`√` 表示该格式对当前数据库类型可用（字段缺失时会带占位符）；`-` 表示菜单中灰显（`IsSupported` 返回 false），不会生成任何字符串。

| 格式 | MySQL | PostgreSQL | SQL Server | Oracle | SQLite | 达梦 |
| --- | --- | --- | --- | --- | --- | --- |
| ADO.NET | √ | √ | √ | √ | √ | - |
| JDBC URL | √ | √ | √ | √ | √ | - |
| ODBC（DSN-less） | √ | √ | √ | √ | √ | - |
| OLE DB | - | - | √ | √ | - | - |
| SQLAlchemy URL | √ | √ | √ | - | √ | - |
| Node.js URL | √ | √ | √ | - | - | - |
| Go DSN | √ | √ | - | - | - | - |
| PHP PDO DSN | √ | √ | √ | √ | √ | - |
| Rust `DATABASE_URL` | √ | √ | √ | - | - | - |

`-` 与「含占位符」的区别：

- `-` 是**语义不存在**：该生态没有对应数据库类型的稳定实现（OLE DB 没有 MySQL 提供程序、Go 没有 SQL Server 驱动约定、sqlx 的 SQLite URL 斜杠规则与 SQLAlchemy 不同）。补任何字段都不会生成，菜单固定灰显。
- 「含占位符」（`√` 且缺字段）是**数据缺失**：格式本身可用，只是当前连接配置少了主机、库名、SID 等，输出 `[...]` 让用户补齐。

达梦整列为 `-`：当前的「数据库类型」下拉里没有达梦（`DbProviderConfig._settingFactory` 未注册），面板上无法建立达梦连接，因此 Go 的 `dm://user:pass@host:port` 不会由本功能生成。

<a id="formats"></a>

## 5. 各格式拆解

统一记法：`host` 为服务器地址，`port` 为端口（连接未填端口时取默认值：SQL Server 1433、MySQL 3306、Oracle 1521、PostgreSQL 5432、SQLite 无端口），`db` 为库名（Oracle 为 SERVICE_NAME，SQLite 为文件路径），`user`/`password` 为账号密码。

各小节的「占位符示例」都是真实输出，可直接对照复制。

### 5.1 ADO.NET

各 Provider 的原生连接串，键值对以 `;` 分隔，关键字大小写不敏感。值里出现 `;`、`=`、引号或首尾空白时，按 ADO.NET 规则加引号（见第 6 节）。

| 数据库 | 结构 |
| --- | --- |
| SQL Server | `Data Source=host[,port];Initial Catalog=db;User ID=user;Password=password;` |
| MySQL | `Server=host;Port=port;Database=db;User ID=user;Password=password;SslMode=None;AllowPublicKeyRetrieval=True;AllowUserVariables=True;CharacterSet=utf8mb4;` |
| Oracle | `Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=host)(PORT=port))(CONNECT_DATA=(SERVICE_NAME=db)));User ID=user;Password=password;` |
| SQLite | `Data Source=path;`（配置了密码时追加 `Password=password;`） |
| PostgreSQL | `Host=host;Port=port;Database=db;Username=user;Password=password;` |

SQL Server 的端口规则与工程内既有实现一致：端口为空或等于 1433 时省略，非默认端口才写成 `host,port`；具名实例写成 `host\instance`，不带端口。

实际输出（账号 `app`，密码 `p@ss:word/? #&;`，库 `demo`）：

```text
Data Source=db.example.com;Initial Catalog=demo;User ID=app;Password="p@ss:word/? #&;";
Server=db.example.com;Port=3306;Database=demo;User ID=app;Password="p@ss:word/? #&;";SslMode=None;AllowPublicKeyRetrieval=True;AllowUserVariables=True;CharacterSet=utf8mb4;
Host=db.example.com;Port=5432;Database=demo;Username=app;Password="p@ss:word/? #&;";
Data Source=IdentityServer.db;
```

占位符示例（SQL Server 只有主机、账号库名都为空；随后是 MySQL 全空、SQLite 无路径）：

```text
Data Source=srv1;Initial Catalog=[数据库];User ID=[用户名];Password=;
Server=[主机];Port=3306;Database=[数据库];User ID=[用户名];Password=;SslMode=None;AllowPublicKeyRetrieval=True;AllowUserVariables=True;CharacterSet=utf8mb4;
Data Source=[文件路径];
```

具名实例缺失（配置里写了 `srv1\` 但实例名为空）：

```text
Data Source=srv1\[实例名];Initial Catalog=[数据库];User ID=[用户名];Password=;
```

替换后预期：把 `[数据库]` 换成真实库名即可连接；`Password=` 为空表示不带密码，需要时自行补值。

### 5.2 JDBC URL

结构：`jdbc:<子协议>://<host>:<port>/<database>`，SQL Server 用 `;` 分隔的键值对而非路径。

| 数据库 | 结构 | 示例 |
| --- | --- | --- |
| MySQL | `jdbc:mysql://host:port/db` | `jdbc:mysql://db.example.com:3306/demo` |
| PostgreSQL | `jdbc:postgresql://host:port/db` | `jdbc:postgresql://db.example.com:5432/demo` |
| SQL Server | `jdbc:sqlserver://host:port;databaseName=db` | `jdbc:sqlserver://db.example.com:1433;databaseName=demo` |
| SQL Server（具名实例） | `jdbc:sqlserver://host:port;instanceName=实例;databaseName=db` | `jdbc:sqlserver://srv:1433;instanceName=SQLEXPRESS;databaseName=demo` |
| Oracle | `jdbc:oracle:thin:@//host:port/service` | `jdbc:oracle:thin:@//192.168.1.170:1521/test` |
| SQLite | `jdbc:sqlite:path` | `jdbc:sqlite:IdentityServer.db` |

库名（Oracle 为 service 名）做百分号编码；SQLite 路径中的 `\` 统一转成 `/`，绝对路径保留盘符：`jdbc:sqlite:C:/data/app.db`。

占位符示例（主机、库名缺失；具名实例缺失；Oracle 缺 SERVICE_NAME；SQLite 缺路径）：

```text
jdbc:mysql://[主机]:3306/[数据库]
jdbc:sqlserver://srv1:1433;instanceName=[实例名];databaseName=[数据库]
jdbc:oracle:thin:@//ora1:1521/[SID/服务名]
jdbc:sqlite:[文件路径]
```

替换后预期：`instanceName` 填实例名（默认实例可整段删除），service 名替换后直接可用；若名字里有空格等字符，按 URL 规则编码。

### 5.3 ODBC（DSN-less）

结构：`Driver={驱动名};` + 若干 `键=值;`。值一律用 `{...}` 包裹，内部 `}` 写成 `}}`。不依赖任何已配置的 DSN，驱动名写死在生成结果里。

| 数据库 | 结构 | 示例 |
| --- | --- | --- |
| SQL Server | `Driver={ODBC Driver 17 for SQL Server};Server={host,port};Database={db};UID={user};PWD={password};` | `Driver={ODBC Driver 17 for SQL Server};Server={db.example.com,1433};Database={demo};UID={app};PWD={p@ss:word/? #&;};` |
| MySQL | `Driver={MySQL ODBC 8.0 Unicode Driver};Server={host};Port=port;Database={db};UID={user};PWD={password};` | `Driver={MySQL ODBC 8.0 Unicode Driver};Server={db.example.com};Port=3306;Database={demo};UID={app};PWD={p@ss:word/? #&;};` |
| PostgreSQL | `Driver={PostgreSQL Unicode};Server={host};Port=port;Database={db};UID={user};PWD={password};` | `Driver={PostgreSQL Unicode};Server={db.example.com};Port=5432;Database={demo};UID={app};PWD={p@ss:word/? #&;};` |
| Oracle | `Driver={Oracle in OraClient19Home1};DBQ={host:port/service};UID={user};PWD={password};` | `Driver={Oracle in OraClient19Home1};DBQ={192.168.1.170:1521/test};UID={test};PWD={test};` |
| SQLite | `Driver={SQLite3 ODBC Driver};Database={path};` | `Driver={SQLite3 ODBC Driver};Database={C:\data\app.db};` |

**为什么只生成 DSN-less：** DSN 形式（`DSN=名称;UID=...;PWD=...;`）依赖目标机器上已经用 `odbcad32` 建好同名数据源，名称、驱动、位数（32/64 位）都取决于运行环境；AutoCode 无法知道也无法验证这些信息，生成出来很可能指向不存在的 DSN。DSN-less 只依赖驱动名，字符串本身即可自证内容，因此本功能只提供这一种。

SQL Server 的 ODBC/OLE DB/PHP 端口规则与 ADO.NET 不同：这里总是显式写出端口（缺省补 1433），具名实例仍写 `host\instance`。

占位符示例：

```text
Driver={ODBC Driver 17 for SQL Server};Server={srv1,1433};Database={[数据库]};UID={[用户名]};PWD={};
Driver={ODBC Driver 17 for SQL Server};Server={srv1\[实例名]};Database={[数据库]};UID={[用户名]};PWD={};
Driver={SQLite3 ODBC Driver};Database={[文件路径]};
```

替换后预期：中括号内的值直接替换，仍留在 `{...}` 内；值里若含 `}` 需写成 `}}`。

### 5.4 OLE DB

结构：`Provider=<提供程序>;Data Source=...;Initial Catalog=...;User ID=...;Password=...;`，值用 `{}` 包裹。

| 数据库 | 结构 | 示例 |
| --- | --- | --- |
| SQL Server | `Provider=SQLOLEDB;Data Source={host,port};Initial Catalog={db};User ID={user};Password={password};` | `Provider=SQLOLEDB;Data Source={db.example.com,1433};Initial Catalog={demo};User ID={app};Password={p@ss:word/? #&;};` |
| Oracle | `Provider=OraOLEDB.Oracle;Data Source={host:port/service};User ID={user};Password={password};` | `Provider=OraOLEDB.Oracle;Data Source={192.168.1.170:1521/test};User ID={test};Password={test};` |

MySQL、PostgreSQL、SQLite 没有随系统或驱动包稳定分发的 OLE DB 提供程序，因此菜单中灰显，不生成 `Provider=...` 字符串。

占位符示例：

```text
Provider=SQLOLEDB;Data Source={srv1,1433};Initial Catalog={[数据库]};User ID={[用户名]};Password=;
Provider=OraOLEDB.Oracle;Data Source={ora1:1521/[SID/服务名]};User ID=scott;Password=;
```

### 5.5 SQLAlchemy URL

结构：`<方言>+<驱动>://<user>:<password>@<host>:<port>/<db>`，用户名、密码、库名做百分号编码。

| 数据库 | 结构 | 示例 |
| --- | --- | --- |
| PostgreSQL | `postgresql+psycopg2://u:p@host:port/db` | `postgresql+psycopg2://app:p%40ss%3Aword%2F%3F%20%23%26%3B@db.example.com:5432/demo` |
| MySQL | `mysql+pymysql://u:p@host:port/db` | `mysql+pymysql://app:p%40ss%3Aword%2F%3F%20%23%26%3B@db.example.com:3306/demo` |
| SQL Server | `mssql+pyodbc://u:p@host:port/db?driver=ODBC+Driver+17+for+SQL+Server` | `mssql+pyodbc://app:p%40ss%3Aword%2F%3F%20%23%26%3B@db.example.com:1433/demo?driver=ODBC+Driver+17+for+SQL+Server` |
| SQLite（相对） | `sqlite:///相对路径`（3 个斜杠） | `sqlite:///IdentityServer.db` |
| SQLite（绝对） | `sqlite:////绝对路径`（4 个斜杠） | `sqlite:////C:/data/app.db` |

Oracle 方言需要额外的驱动名（`oracle+oracledb` 等），本功能不生成，菜单灰显。

占位符示例（SQL Server 具名实例缺失时，`\` 编码为 `%5C`，实例名占位符保持可读）：

```text
mssql+pyodbc://[用户名]@srv1%5C[实例名]:1433/[数据库]?driver=ODBC+Driver+17+for+SQL+Server
sqlite:///[文件路径]
```

替换后预期：`[实例名]` 替换为实例名（不加 `%5C`，`%5C` 已经写在前面），名字含空格时写成 `%20`。

### 5.6 Node.js URL

结构：`<scheme>://<user>:<password>@<host>:<port>/<db>`，用户名与密码百分号编码；PostgreSQL 追加 `?sslmode=disable`。

| 数据库 | 示例 |
| --- | --- |
| PostgreSQL | `postgres://app:p%40ss%3Aword%2F%3F%20%23%26%3B@db.example.com:5432/demo?sslmode=disable` |
| MySQL | `mysql://app:p%40ss%3Aword%2F%3F%20%23%26%3B@db.example.com:3306/demo` |
| SQL Server | `mssql://app:p%40ss%3Aword%2F%3F%20%23%26%3B@db.example.com:1433/demo` |

占位符示例：

```text
mysql://[用户名]@[主机]:3306/[数据库]
mssql://[用户名]@srv1%5C[实例名]:1433/[数据库]
```

### 5.7 Go DSN

| 数据库 | 结构 | 示例 |
| --- | --- | --- |
| MySQL | `user:password@tcp(host:port)/db?charset=utf8mb4&parseTime=True&loc=Local` | `app:p@ss:word/? #&;@tcp(db.example.com:3306)/demo?charset=utf8mb4&parseTime=True&loc=Local` |
| PostgreSQL | `postgres://user:password@host:port/db?sslmode=disable` | `postgres://app:p%40ss%3Aword%2F%3F%20%23%26%3B@db.example.com:5432/demo?sslmode=disable` |

MySQL 的 DSN 不是 URL，按 go-sql-driver 的语法原样插入用户名与密码（不做百分号编码），所以示例里的特殊字符保持原样；PostgreSQL 用 URL 形式，按第 6 节编码。

SQL Server、Oracle、SQLite 没有对应的 Go DSN 约定，菜单灰显。达梦的 `dm://user:pass@host:port` 同样不生成：面板没有达梦连接类型。

占位符示例：

```text
[用户名]:@tcp([主机]:3306)/[数据库]?charset=utf8mb4&parseTime=True&loc=Local
```

替换后预期：原样替换，不编码；密码为空时 `@` 前没有内容（`user:@tcp(...)`）。

### 5.8 PHP PDO DSN

结构：`<驱动>:<键=值;...>`。PDO 的 DSN 只描述位置，账号密码通常作为 `PDO` 构造参数单独传入，因此这里不含 `UID`/`PWD`（SQL Server 例外地使用 `Server`/`Database` 两个键）。

| 数据库 | 结构 | 示例 |
| --- | --- | --- |
| MySQL | `mysql:host=h;port=p;dbname=db;charset=utf8mb4` | `mysql:host=db.example.com;port=3306;dbname=demo;charset=utf8mb4` |
| PostgreSQL | `pgsql:host=h;port=p;dbname=db` | `pgsql:host=db.example.com;port=5432;dbname=demo` |
| SQL Server | `sqlsrv:Server=h,port;Database=db` | `sqlsrv:Server=db.example.com,1433;Database=demo` |
| SQLite | `sqlite:path` | `sqlite:IdentityServer.db` / `sqlite:C:/data/app.db` |
| Oracle | `oci:dbname=h:port/service;charset=AL32UTF8` | `oci:dbname=192.168.1.170:1521/test;charset=AL32UTF8` |

占位符示例：

```text
mysql:host=[主机];port=3306;dbname=[数据库];charset=utf8mb4
sqlsrv:Server=srv1\[实例名];Database=[数据库]
oci:dbname=ora1:1521/[SID/服务名];charset=AL32UTF8
sqlite:[文件路径]
```

### 5.9 Rust `DATABASE_URL`

结构：URL 风格，供 sqlx 等库直接读取环境变量。

| 数据库 | 示例 |
| --- | --- |
| PostgreSQL | `postgres://app:p%40ss%3Aword%2F%3F%20%23%26%3B@db.example.com:5432/demo?sslmode=disable` |
| MySQL | `mysql://app:p%40ss%3Aword%2F%3F%20%23%26%3B@db.example.com:3306/demo` |
| SQL Server | `mssql://app:p%40ss%3Aword%2F%3F%20%23%26%3B@db.example.com:1433/demo` |

SQLite 与 Oracle 不生成：sqlx 的 SQLite URL 斜杠规则与 SQLAlchemy 不同，混用会产生歧义。

占位符示例：

```text
postgres://[用户名]@[主机]:5432/[数据库]?sslmode=disable
```

<a id="encoding"></a>

## 6. 编码与转义规则

### 6.1 URL 风格（JDBC 库名、SQLAlchemy、Node.js、Go 的 PostgreSQL、Rust）

保留未保留字符 `A-Z a-z 0-9 - . _ ~`，其余字符按 UTF-8 逐字节写成 `%XX`（大写十六进制）。用户名、密码、库名分别编码；密码为空时省略 `:password` 段。

密码 `p@ss:word/? #&;` 的实际编码结果：

```text
p%40ss%3Aword%2F%3F%20%23%26%3B
```

占位符本身不编码：`[实例名]` 保持中括号原样，用户替换时若填的值含空格等字符，再按上述规则编码。值里本身含 `[`/`]` 时按规则编码（`we[ird]db` → `we%5Bird%5Ddb`），与占位符互不干扰。

### 6.2 键值对风格（ADO.NET）

值包含 `;`、`=`、单双引号或首尾空白时加引号；同种引号在值内部成对转义。选择规则：值里有 `"` 而没有 `'` 时用单引号包裹，否则用双引号并把手内的 `"` 写成 `""`。

| 原值 | 输出 |
| --- | --- |
| `a;b` | `"a;b"` |
| `a"b` | `'a"b'` |
| `plain` | `plain` |

### 6.3 大括号风格（ODBC、OLE DB）

值一律写成 `{value}`，内部 `}` 转义为 `}}`。例如密码 `a}b` 输出为 `PWD={a}}b}`。

### 6.4 原样插入（Go MySQL DSN、PHP PDO DSN）

Go 的 MySQL DSN 与 PDO DSN 按各自语法原样拼接，不做编码。PHP 的 DSN 值里若出现 `;`，PDO 会把它当成下一个键的分隔符，需要业务侧自行规避；Go 的密码里若出现 `@`、`/` 等与 DSN 分隔符冲突的字符，建议改用 `?password=` 等参数形式。

<a id="info"></a>

## 7. 连接信息清单（连接节点）

连接节点右键「复制连接信息 / Copy connection info」，展开两个子项：

| 子项 | 中文 | 英文 | 内容 |
| --- | --- | --- | --- |
| 含密码 | 含密码（明文） | Including password (plain text) | 输出真实密码（为空时 `[密码]`/`[password]`） |
| 不含密码 | 不含密码 | Without password | 整行省略密码 |

**为什么做成两个子项而不是掩码 `******`：** 掩码版本复制后用户仍需手工删改，且掩码本身不是可用的值；两个子项语义明确，需要密码就直接拿，不需要就不会把密码带走。

### 7.1 字段顺序

按「他人复现连接时最需要先知道」的顺序：主机 → 端口 → 实例/服务名 → 用户名 → 密码。

| 数据库 | 输出的行 |
| --- | --- |
| SQL Server | 主机、端口、实例名（仅具名实例）、用户名、密码 |
| MySQL / PostgreSQL | 主机、端口、用户名、密码 |
| Oracle | 主机、端口、服务名（SID/服务名）、用户名、密码 |
| SQLite | 文件路径、密码（仅含密码变体） |

清单**不含数据库类型与连接名称**：数据库类型已由各字段的取值与标签体现，连接名称只是本地配置的别名，对拿到清单的人复现连接没有帮助。清单也**不含数据库名**：库名属于数据库节点的职责（见第 5 节），连接节点只能看到连接级信息。清单还不含 `=`、`;` 等连接串语法，避免被当成可解析的连接串误用。

### 7.2 端口

- 配置里显式填了端口：原样输出，例如 `端口: 3307`。
- 配置里没填端口（`DbViewModel.Port` 为 0）：输出该数据库类型的标准默认端口——SQL Server 1433、MySQL 3306、PostgreSQL 5432、Oracle 1521——**不加任何标注**（没有「（默认）」、括号、星号或注释）。

默认端口映射集中在一张按 Provider 索引的常量表中（`DbConnectionInfoFormatter.DefaultPorts`）；SQLite 没有端口，不参与该映射。补默认值的行与普通值一样直接呈现数字，不产生占位符，也不计入「待填写」数量。

### 7.3 SQL Server 具名实例

`Data Source` 写成 `host\instance` 时，清单把主机与实例名拆成两行；没有具名实例（默认实例）时不输出「实例名」行，避免让阅读方以为漏填。

### 7.4 Oracle 的 SID 与 Service Name

现有连接模型（`ToOracleModel`）只从 `CONNECT_DATA` 中提取 `SERVICE_NAME`，无法区分 SID 与 Service Name，也不保存 SID。因此标签写成「服务名（SID/服务名）」/「Service name (SID/service name)」，值为空时输出 `[SID/服务名]`/`[SID/service name]`，不臆造字段。

### 7.5 SQLite 的特殊呈现

SQLite 没有主机、端口、用户名，库标识就是文件路径。清单用「文件路径」标签输出路径，不把路径塞进「主机」；主机/端口/用户名三行整体不输出，避免机械输出空值造成误导。

### 7.6 空值呈现

清单里出现的字段，值为空时统一输出 `[...]` 占位符（与第 3 节同一套本地化键）；**不输出空字符串、`null` 或「未设置」**——「未设置」无法被对方直接替换，占位符可以直接原地替换。

### 7.7 示例输出

MySQL（端口显式配置为 3307，含密码）：

```text
主机: 10.0.0.8
端口: 3307
用户名: app
密码: S3cret
```

SQL Server（端口为空，补 1433；具名实例，不含密码）：

```text
主机: db1
端口: 1433
实例名: SQLEXPRESS
用户名: sa
```

SQLite（不含密码）：

```text
文件路径: D:\data\app.db
```

Oracle（端口为空，补 1521；缺服务名与密码）：

```text
主机: ora1
端口: 1521
服务名（SID/服务名）: [SID/服务名]
用户名: scott
密码: [密码]
```

英文版（`culture=en`）：

```text
Host: 10.0.0.8
Port: 3307
User name: app
Password: S3cret
```

```text
Host: db1
Port: 1433
Named instance: SQLEXPRESS
User name: sa
```

```text
File path: D:\data\app.db
```

### 7.8 复制提示

| 情形 | 中文 | 英文 |
| --- | --- | --- |
| 含密码 | 已复制连接信息（含明文密码） | Copied connection info (contains plain-text password) |
| 不含密码 | 已复制连接信息（不含密码） | Copied connection info (without password) |
| 有占位符 | 追加「（含 {0} 处待填写）」 | 追加「 ({0} to fill in)」 |
| 失败 | 复制失败：{0} | Copy failed: {0} |

提示不包含连接名称，需要区分来源时以右键的节点为准。提示以鼠标旁的浮动 ToolTip 呈现，约 2.5 秒自动消失，不阻塞操作；日志只记模式（含/不含密码），不写清单正文。

<a id="limits"></a>

## 8. 已知限制

| 场景 | 现状 | 原因 |
| --- | --- | --- |
| Oracle SID 与 Service Name | 只生成 service 形式（`@//host:port/service`、`DBQ=host:port/service`、`oci:dbname=host:port/service`）；缺 SERVICE_NAME 时已用 `[SID/服务名]` 占位符覆盖 | 连接模型用正则从 `CONNECT_DATA` 中提取 `SERVICE_NAME`，用 SID 配置的连接取不到 service 名，也不保存 SID，无法还原 `@host:port:SID` |
| Oracle 的 TNS 别名 / 无 HOST | 已用 `[主机]` 占位符覆盖，不再整条禁用 | 解析只识别 `(HOST=...)(PORT=...)(SERVICE_NAME=...)`，别名形式的 Data Source 解析不出主机 |
| SQL Server 具名实例 | ADO.NET/ODBC/OLE DB/PHP 写 `host\instance`；JDBC 写 `;instanceName=实例`；URL 风格写 `host%5C实例`，实例名为空时为 `host%5C[实例名]` | URL 规范没有统一的实例名参数位置；统一用 `%5C` 表示反斜杠，各客户端行为一致 |
| SQL Server 端口 | ADO.NET 省略默认的 1433；其余格式显式写出 | 与工程内既有 `ToSqlServerConnectionString` 行为保持一致，便于和界面保存的串对照 |
| SQLite 相对/绝对路径 | 相对 `sqlite:///x.db`，绝对 `sqlite:////C:/x.db`；JDBC/PHP 统一把 `\` 换成 `/`；ODBC 保留原始反斜杠 | 各客户端对斜杠数量的约定不同，绝对路径多一个 `/` 表示从根开始 |
| SQLite 加密库密码 | ADO.NET 追加 `Password=...`，其余格式不携带 | `Password` 是 System.Data.SQLite 的私有扩展，JDBC/ODBC/PDO 没有等价语义 |
| 达梦 | 全部灰显 | 面板没有达梦连接类型（`DbProviderConfig` 未注册），无法产生 `DbViewModel`；属于语义不存在，不是缺字段 |
| 驱动名 | ODBC/OLE DB 使用常见发行版的驱动名（如 `ODBC Driver 17 for SQL Server`、`Oracle in OraClient19Home1`） | 驱动名随安装包与版本变化，目标机器若装的是其他版本需要手工替换 |
| PDO 账号密码 | 不在 DSN 中 | PDO 通过构造参数传凭据；`sqlsrv` 也只用 `Server`/`Database` |
| 连接信息清单的数据库名 | 不包含 | 连接节点只代表一条连接配置，库名由数据库节点决定（见第 7 节） |

<a id="edge"></a>

## 9. 菜单的边界行为

| 情形 | 行为 |
| --- | --- |
| 右键连接节点 | 弹「复制连接信息」，两个子项（含/不含密码） |
| 右键数据库节点 | 弹「复制连接字符串」，九项格式；库名取该节点 |
| 右键表节点或空白处 | 不弹菜单，保持原有行为 |
| 数据库节点取不到所属连接（节点绑定异常） | 不弹菜单，不做兜底猜测 |
| Provider 不在 `DbProviderConfig._providerFactory` 中 | 不弹菜单 |
| 连接缺少主机（SQLite 为文件路径）、库名、service 名或实例名 | 菜单仍弹出，对应位置输出占位符，菜单项后缀「（含 N 处待填写）」 |
| 账号或密码为空 | 连接串照常生成（空密码语法）；信息清单输出 `[用户名]`/`[密码]` 占位符 |
| 格式不适用于当前数据库类型 | 单项灰显，悬停提示「当前类型不支持此格式」，点击无效 |
| 复制成功且无占位符 | 浮动提示「已复制：<格式名>」或「已复制连接信息（含明文密码）」 |
| 复制成功但有占位符 | 提示后追加「（含 N 处待填写）」 |
| 剪贴板被其他进程占用 | 捕获异常，浮动提示「复制失败：<原因>」，不弹模态框 |

<a id="impl"></a>

## 10. 与实现的对应关系

| 内容 | 位置 |
| --- | --- |
| 格式枚举与顺序 | `Core/DbConnectionStringFormats.cs` 的 `DbConnectionFormat` / `AllFormats` |
| 适用性矩阵 | 同文件 `IsSupported` |
| 占位符输出与计数 | 同文件私有 `Template`（`Placeholder` / `EncodedOrPlaceholder` / `OdbcOrPlaceholder`），计数写入 `DbConnectionStringResult.PlaceholderCount` |
| 百分号编码 | 同文件 `EncodeUrlComponent` |
| ADO.NET 引号规则 | 同文件 `AdoQuote` |
| ODBC/OLE DB 大括号规则 | 同文件 `OdbcValue` |
| 连接信息清单 | `Core/DbConnectionInfoFormatter.cs` 的 `Build`；字段顺序见同文件 `Build` 内的 provider 分支 |
| 端口默认值 | 信息清单用 `DbConnectionInfoFormatter.DefaultPorts` 常量表；连接字符串用 `DbConnectionStringFormats.GetDefaultPort` |
| 两个菜单的构建与复制反馈 | `Forms/DbPanel.cs` 的 `tvDb_MouseDown` / `ShowConnectionStringMenu` / `ShowConnectionInfoMenu` / `ConnectionStringMenuItem_Click` / `ConnectionInfoMenuItem_Click` |
| 本地化文案 | `Resources/Localization.resx`（英文，中性）与 `Resources/Localization.zh-CN.resx`（中文），键集合必须一致 |
