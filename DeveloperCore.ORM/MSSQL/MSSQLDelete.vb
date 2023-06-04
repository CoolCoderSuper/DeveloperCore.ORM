Imports DeveloperCore.ORM.Core
Imports Microsoft.Data.SqlClient

Namespace MSSQL

    Public Class MSSQLDelete
        Implements IDelete
        Private _tableName As String
        Private _filterColumn As String
        Private _filterValue As Object
    
        Public Function Table(tableName As String) As MSSQLDelete Implements IDelete.Table
            _tableName = tableName
            Return Me
        End Function
    
        Public Function Filter(column As String, value As Object) As MSSQLDelete Implements IDelete.Filter
            _filterColumn = column
            _filterValue = value
            Return Me
        End Function
    
        Public Function GetCommand() As ICommand Implements IDelete.GetCommand
            Dim cmd As New SqlCommand
            cmd.CommandText = $"delete from [{_tableName}] where [{_filterColumn}]=@{_filterColumn}"
            cmd.Parameters.AddWithValue($"@{_filterColumn}", _filterValue)
            Return New MSSQLCommand(cmd)
        End Function
    End Class
End NameSpace