Imports System.Text
Imports DeveloperCore.ORM.Core
Imports Microsoft.Data.SqlClient

Namespace MSSQL

    Public Class MSSQLInsert
        Implements IInsert
        Private _tableName As String
        Private ReadOnly _values As New List(Of Object)
        
        Public Function Table(tableName As String) As MSSQLInsert Implements IInsert.Table
            _tableName = tableName
            Return Me
        End Function
    
        Public Function [Set](value As Object) As MSSQLInsert Implements IInsert.[Set]
            _values.Add(value)
            Return Me
        End Function
    
        Public Function GetCommand() As ICommand Implements IInsert.GetCommand
            Dim cmd As New SqlCommand
            Dim sql As New StringBuilder($"insert into [{_tableName}] values (")
            For Each value As Object In _values
                sql.Append($"@{value}{If(value.Equals(_values.Last), ")", ",")}")
                cmd.Parameters.AddWithValue($"@{value}", value)
            Next
            cmd.CommandText = sql.ToString
            Return New MSSQLCommand(cmd)
        End Function
    End Class
End NameSpace