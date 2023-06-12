Imports System.Reflection
Imports DeveloperCore.ORM.Attributes
Imports DeveloperCore.ORM.Core

Namespace Relation
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
            Dim generator As IQueryGenerator = _dc.Connection.Generate()
            generator.From(If(tableNameAttr Is Nothing, tableType.Name, tableNameAttr.Name)).SelectAll().Filter(fkAttr.Column, "=", "@Item1", _value)
            Return _dc.Fetch(Of T)(generator.GetCommand()).GetEnumerator
        End Function

        Private Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Return GetEnumerator()
        End Function
    End Class
End Namespace