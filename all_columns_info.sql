IF OBJECT_ID (N'all_columns_info', N'V') IS NOT NULL
	DROP VIEW all_columns_info
GO
-- ==============================================================
--      Author: YuanRui
-- Create date: 2015-7-8
-- Description:	查看当前数据库的数据表中每一列的具体信息
-- ==============================================================
CREATE VIEW all_columns_info
AS
WITH colConsCTE(TABLE_NAME, COLUMN_NAME, CONSTRAINT_TYPE) AS (
     SELECT A.TABLE_NAME, A.COLUMN_NAME, B.CONSTRAINT_TYPE FROM INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE A
     LEFT JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS B ON A.CONSTRAINT_NAME = B.CONSTRAINT_NAME AND A.TABLE_NAME = B.TABLE_NAME 
)
SELECT a.object_id AS TableID, a.name AS TableName, b.column_id AS ID, b.name as Name, d.value AS Comment , c.name AS DBType, b.max_length AS 'Length',
	CASE WHEN (SELECT COUNT(1) FROM colConsCTE cte WHERE b.name = cte.COLUMN_NAME AND a.name = cte.TABLE_NAME AND cte.CONSTRAINT_TYPE ='PRIMARY KEY') > 0 THEN 1 ELSE 0 END AS IsPrimaryKey,
	CASE WHEN (SELECT COUNT(1) FROM colConsCTE cte WHERE b.name = cte.COLUMN_NAME AND a.name = cte.TABLE_NAME AND cte.CONSTRAINT_TYPE ='FOREIGN KEY') > 0 THEN 1 ELSE 0 END AS IsForeignKey,
	b.IS_NULLABLE as 'IsNullAble'
FROM sys.tables a 
LEFT JOIN sys.columns b on a.object_id = b.object_id
LEFT JOIN sys.types c on b.user_type_id = c.user_type_id
LEFT JOIN sys.extended_properties d on a.object_id = d.major_id and d.minor_id = b.column_id

GO
