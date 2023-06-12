Imports DeveloperCore.ORM.Core

Public Class MSSQLQueryGenerator
    Implements IQueryGenerator
    Private ReadOnly _connection As IConnection
    Private _table As String
    Private ReadOnly _columns As New HashSet(Of String)
    Private ReadOnly _filters As New List(Of String)
    Private ReadOnly _params As New Dictionary(Of String, Object)
    Private _orderBy As String
    
    Public Sub New(connection As IConnection)
        _connection = connection
    End Sub
        
    Public Function From(table As String) As IQueryGenerator Implements IQueryGenerator.From
        _table = table
        Return Me
    End Function
    
    Public Function [Select](col As String) As IQueryGenerator Implements IQueryGenerator.[Select]
        _columns.Add(col)
        Return Me
    End Function
    
    Public Function [Select](ParamArray cols As String()) As IQueryGenerator Implements IQueryGenerator.[Select]
        For Each col As String In cols
            _columns.Add(col)
        Next
        Return Me
    End Function
    
    Public Function SelectAll() As IQueryGenerator Implements IQueryGenerator.SelectAll
        _columns.Clear()
        Return Me
    End Function
        
    Public Function Filter(col As String, op As String, val As String) As IQueryGenerator Implements IQueryGenerator.Filter
        _filters.Add($"[{col}]{op}'{val}'")
        Return Me
    End Function
        
    Public Function Filter(col As String, op As String, paramName As String, paramValue As String) As IQueryGenerator Implements IQueryGenerator.Filter
        _filters.Add($"[{col}]{op}{paramName}")
        _params.Add(paramName, paramValue)
        Return Me
    End Function
        
    Public Function [And]() As IQueryGenerator Implements IQueryGenerator.[And]
        _filters.Add("and")
        Return Me
    End Function
        
    Public Function [Or]() As IQueryGenerator Implements IQueryGenerator.[Or]
        _filters.Add("or")
        Return Me
    End Function
        
    Public Function [Not]() As IQueryGenerator Implements IQueryGenerator.[Not]
        _filters.Add("not")
        Return Me
    End Function
        
    Public Function OrderBy(col As String) As IQueryGenerator Implements IQueryGenerator.OrderBy
        _orderBy = col
        Return Me
    End Function
        
    Public Function OrderByDesc(col As String) As IQueryGenerator Implements IQueryGenerator.OrderByDesc
        _orderBy = $"{col} desc"
        Return Me
    End Function
    
    Public Function GetCommand() As ICommand Implements IQueryGenerator.GetCommand
        Dim command As ICommand = _connection.Command()
        command.CommandText = GetCommandText()
        For Each param As KeyValuePair(Of String, Object) In _params
            command.Parameters.Add(param.Key, param.Value)
        Next
        Return command
    End Function

    Private Function GetCommandText() As String
        Dim sb As New Text.StringBuilder()
        sb.Append("select ")
        If _columns.Count = 0 Then
            sb.Append("*")
        Else
            Dim first As Boolean = True
            For Each col As String In _columns
                If Not first Then
                    sb.Append(", ")
                End If
                sb.Append("[")
                sb.Append(col)
                sb.Append("]")
                first = False
            Next
        End If
        sb.Append(" from [")
        sb.Append(_table)
        sb.Append("]")
        If _filters.Any() Then
            sb.Append(" where")
        End If
        For Each filter As String In _filters
            sb.Append(" ")
            sb.Append(filter)
        Next
        If Not String.IsNullOrEmpty(_orderBy) Then
            sb.Append(" order by ")
            sb.Append(_orderBy)
        End If
        Return sb.ToString()
    End Function
End Class