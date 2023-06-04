Imports DeveloperCore.ORM.MSSQL
Imports Microsoft.Data.SqlClient

Namespace Core

    Public interface IDelete
        Function Table(tableName As String) As MSSQLDelete
        Function Filter(column As String, value As Object) As MSSQLDelete
        Function GetCommand() As ICommand
    end interface
End NameSpace