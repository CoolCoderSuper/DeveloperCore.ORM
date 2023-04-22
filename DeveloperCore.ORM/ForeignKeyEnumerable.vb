Imports DeveloperCore.ORM.Attributes
Imports System.Reflection

Public Class ForeignKeyEnumerable(Of T)
    Implements IEnumerable(Of T)

    Private ReadOnly _dc As DataContext
    Private ReadOnly _value As String

    Public Sub New(dc As DataContext, value As String)
        _dc = dc
        _value = value
    End Sub

    Public Function GetEnumerator() As IEnumerator(Of T) Implements IEnumerable(Of T).GetEnumerator
        Dim tableType As Type = GetType(T)
        Dim tableNameAttr As TableNameAttribute = tableType.GetCustomAttribute(Of TableNameAttribute)
        Dim fkAttr As ForeignKeyAttribute = tableType.GetCustomAttribute(Of ForeignKeyAttribute)
        Dim sql As String = $"select * from [{If(tableNameAttr Is Nothing, tableType.Name, tableNameAttr.Name)}] where [{fkAttr.Column}] = @Item1"
        Return _dc.Fetch(Of T)(sql, _value).GetEnumerator
    End Function

    Private Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
        Return GetEnumerator()
    End Function
End Class