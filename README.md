# CodeGenerator
![main](https://user-images.githubusercontent.com/3859838/87387021-6ebf7080-c5d4-11ea-989d-a81aa9972852.png)

Use Code Generator create C# code file. Include generate Thrift IDL template file. The engine base on [mono t4](https://github.com/mono/t4).  
Some of code in this project https://github.com/yuanrui/Examples

In template file, you can use meta list as follows.

Column Info:

| No | Name | Type | 
|:-------------|:------------- |:------------- |
| 1 | Id | String |
| 2 | Name | String |
| 3 | RawType | String |
| 4 | DataType | DbType |
| 5 | Type | Type |
| 6 | TypeName | String |
| 7 | Comment | String |
| 8 | IsPrimaryKey | Boolean |
| 9 | IsForeignKey | Boolean |
| 10 | IsUnique | Boolean |
| 11 | IsNullable | Boolean |
| 12 | Length | Int32 |
| 13 | Precision | Int16 |
| 14 | Scale | Int16 |
| 15 | Index | Int32 |
| 16 | Table | Table |

[TODO List](https://github.com/yuanrui/CodeGenerator/issues/1)
