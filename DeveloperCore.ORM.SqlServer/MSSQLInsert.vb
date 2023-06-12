Imports System.Text
Imports DeveloperCore.ORM.Core

Public Class MSSQLInsert
    Implements IInsert
    Private _tableName As String
    Private ReadOnly _values As New List(Of Object)
        
    Public Function Table(tableName As String) As IInsert Implements IInsert.Table
        _tableName = tableName
        Return Me
    End Function
    
    Public Function [Set](value As Object) As IInsert Implements IInsert.[Set]
        _values.Add(value)
        Return Me
    End Function
    
    Public Function GetCommand() As ICommand Implements IInsert.GetCommand
        Dim cmd As New MSSQLCommand()
        Dim sql As New StringBuilder($"insert into [{_tableName}] values (")
        For Each value As Object In _values
            sql.Append($"@{value}{If(value.Equals(_values.Last), ")", ",")}")
            cmd.Parameters.Add($"@{value}", value)
        Next
        cmd.CommandText = sql.ToString
        Return cmd
    End Function
End Class