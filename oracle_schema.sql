
WITH colConsCTE AS (
     SELECT a.*, b.CONSTRAINT_TYPE FROM user_cons_columns a
     LEFT JOIN user_constraints b ON a.constraint_name = b.constraint_name AND a.table_name = b.table_name 
     WHERE a.TABLE_NAME = :TableName
)

SELECT tab.COLUMN_ID AS Id, tab.TABLE_NAME AS TableName, tab.COLUMN_NAME AS ColumnName, col.COMMENTS AS Remark
, tab.DATA_TYPE AS DbType
, tab.DATA_LENGTH AS Length
, tab.DATA_PRECISION AS Precision
, tab.DATA_SCALE AS Scale
, CASE tab.NULLABLE WHEN 'Y' THEN 1 ELSE 0 END AS IsNullable
, CASE WHEN (SELECT COUNT(1) FROM colConsCTE cte WHERE tab.COLUMN_NAME = cte.COLUMN_NAME AND tab.TABLE_NAME = cte.TABLE_NAME AND cte.CONSTRAINT_TYPE ='P') > 0 THEN 1 ELSE 0 END AS IsPrimaryKey
, CASE WHEN (SELECT COUNT(1) FROM colConsCTE cte WHERE tab.COLUMN_NAME = cte.COLUMN_NAME AND tab.TABLE_NAME = cte.TABLE_NAME AND cte.CONSTRAINT_TYPE ='R') > 0 THEN 1 ELSE 0 END AS IsForeignKey
, CASE WHEN (SELECT COUNT(1) FROM colConsCTE cte WHERE tab.COLUMN_NAME = cte.COLUMN_NAME AND tab.TABLE_NAME = cte.TABLE_NAME AND cte.CONSTRAINT_TYPE ='U') > 0 THEN 1 ELSE 0 END AS IsUnique
, CASE WHEN (SELECT COUNT(1) FROM colConsCTE cte WHERE tab.COLUMN_NAME = cte.COLUMN_NAME AND tab.TABLE_NAME = cte.TABLE_NAME AND cte.CONSTRAINT_TYPE ='O') > 0 THEN 1 ELSE 0 END AS IsReadOnly
, 0 AS IsIdentity
FROM USER_TAB_COLUMNS tab
LEFT JOIN USER_COL_COMMENTS col ON tab.table_name = col.table_name AND tab.COLUMN_NAME = col.COLUMN_NAME
WHERE tab.table_name = :TableName
