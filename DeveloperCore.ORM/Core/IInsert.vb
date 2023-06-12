Imports DeveloperCore.ORM.MSSQL
Imports Microsoft.Data.SqlClient

Namespace Core
    Public Interface IInsert
        Function Table(tableName As String) As IInsert
        Function [Set](value As Object) As IInsert
        Function GetCommand() As ICommand
    End Interface
End Namespace