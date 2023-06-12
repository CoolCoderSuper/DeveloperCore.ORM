Imports DeveloperCore.ORM.MSSQL
Imports Microsoft.Data.SqlClient

Namespace Core
    Public Interface IUpdate
        Function Table(tableName As String) As IUpdate
        Function [Set](column As String, value As Object) As IUpdate
        Function Filter(column As String, value As Object) As IUpdate
        Function GetCommand() As ICommand
    End Interface
End Namespace