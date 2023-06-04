Imports DeveloperCore.ORM.MSSQL
Imports Microsoft.Data.SqlClient

Namespace Core

    Public interface IUpdate
        Function Table(tableName As String) As MSSQLUpdate
        Function [Set](column As String, value As Object) As MSSQLUpdate
        Function Filter(column As String, value As Object) As MSSQLUpdate
        Function GetCommand() As ICommand
    end interface
End NameSpace