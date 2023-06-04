Imports DeveloperCore.ORM.MSSQL
Imports Microsoft.Data.SqlClient

Namespace Core

    Public interface IInsert
        Function Table(tableName As String) As MSSQLInsert
        Function [Set](value As Object) As MSSQLInsert
        Function GetCommand() As ICommand
    end interface
End NameSpace