Imports DeveloperCore.ORM.MSSQL
Imports Microsoft.Data.SqlClient

Namespace Core
    Public Interface IDelete
        Function Table(tableName As String) As MSSQLDelete
        Function Filter(column As String, value As Object) As MSSQLDelete
        Function GetCommand() As ICommand
    End Interface
End Namespace