Imports DeveloperCore.ORM.MSSQL
Imports Microsoft.Data.SqlClient

Namespace Core
    Public Interface IUpdate
        Function Table(tableName As String) As MSSQLUpdate
        Function [Set](column As String, value As Object) As MSSQLUpdate
        Function Filter(column As String, value As Object) As MSSQLUpdate
        Function GetCommand() As ICommand
    End Interface
End Namespace