Imports System.Text
Imports DeveloperCore.ORM.Core
Imports Microsoft.Data.SqlClient

Namespace MSSQL

    Public Class MSSQLUpdate
        Implements IUpdate
        Private _tableName As String
        Private ReadOnly _values As New Dictionary(Of String, Object)
        Private _filterColumn As String
        Private _filterValue As Object

        Public Function Table(tableName As String) As MSSQLUpdate Implements IUpdate.Table
            _tableName = tableName
            Return Me
        End Function

        Public Function [Set](column As String, value As Object) As MSSQLUpdate Implements IUpdate.[Set]
            _values.Add(column, value)
            Return Me
        End Function

        Public Function Filter(column As String, value As Object) As MSSQLUpdate Implements IUpdate.Filter
            _filterColumn = column
            _filterValue = value
            Return Me
        End Function

        Public Function GetCommand() As ICommand Implements IUpdate.GetCommand
            Dim cmd As New SqlCommand
            Dim sql As New StringBuilder($"update [{_tableName}] set ")
            For Each kvp As KeyValuePair(Of String, Object) In _values
                sql.Append($"[{kvp.Key}]=@{kvp.Key}{If(kvp.Equals(_values.Last), " ", ",")}")
                cmd.Parameters.AddWithValue($"@{kvp.Key}", kvp.Value)
            Next
            sql.Append($"where [{_filterColumn}]=@{_filterColumn}")
            cmd.CommandText = sql.ToString
            cmd.Parameters.AddWithValue($"@{_filterColumn}", _filterValue)
            Return New MSSQLCommand(cmd)
        End Function
    End Class
End NameSpace