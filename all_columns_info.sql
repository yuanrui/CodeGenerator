IF OBJECT_ID (N'all_columns_info', N'V') IS NOT NULL
	DROP VIEW all_columns_info
GO
-- ==============================================================
--      Author: YuanRui
-- Create date: 2014-7-24
-- Description:	查看当前数据库的数据表中每一列的具体信息
-- ==============================================================
CREATE VIEW all_columns_info
as
select a.object_id as TableID, a.name as TableName, b.column_id as ID, b.name as Name, d.value as Comment , c.name as DBType, b.max_length as 'Length',
'IsPrimaryKey' = case when f.CONSTRAINT_TYPE = 'PRIMARY KEY' then CAST(1 as bit) else CAST(0 as bit) end, 'IsForeignKey'=case when f.CONSTRAINT_TYPE = 'FOREIGN KEY' then CAST(1 as bit) else CAST(0 as bit) end, b.IS_NULLABLE as 'IsNullAble'
from sys.tables a 
inner join sys.columns b on a.object_id = b.object_id
inner join sys.types c on b.user_type_id = c.user_type_id
left join sys.extended_properties d on a.object_id = d.major_id and d.minor_id = b.column_id
left join INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE e on a.name = e.TABLE_NAME and b.name = e.COLUMN_NAME
left join INFORMATION_SCHEMA.TABLE_CONSTRAINTS f on e.CONSTRAINT_NAME = f.CONSTRAINT_NAME

GO