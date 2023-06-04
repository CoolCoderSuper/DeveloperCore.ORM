Imports DeveloperCore.ORM.MSSQL
Imports Microsoft.Data.SqlClient

Namespace Core
    Public Interface IInsert
        Function Table(tableName As String) As MSSQLInsert
        Function [Set](value As Object) As MSSQLInsert
        Function GetCommand() As ICommand
    End Interface
End Namespace