﻿Imports System.Reflection
Imports DeveloperCore.ORM.Attributes
Imports DeveloperCore.ORM.Core

Namespace Relation
    Public Class ParentRef(Of T)
        Private ReadOnly _dc As DataContext
        Private ReadOnly _value As String

        Public Sub New(dc As DataContext, value As String)
            _dc = dc
            _value = value
        End Sub

        Public ReadOnly Property Value As T
            Get
                Dim tableType As Type = GetType(T)
                Dim tableNameAttr As TableNameAttribute = tableType.GetCustomAttribute(Of TableNameAttribute)
                Dim keyProp As PropertyInfo = tableType.GetProperties.FirstOrDefault(Function(x) x.GetCustomAttribute(Of KeyAttribute) IsNot Nothing)
                If keyProp IsNot Nothing Then
                    Dim nameAttr As ColumnNameAttribute = keyProp.GetCustomAttribute(Of ColumnNameAttribute)
                    Dim generator As IQueryGenerator = _dc.Connection.Generate()
                    generator.From(If(tableNameAttr Is Nothing, tableType.Name, tableNameAttr.Name)).SelectAll().Filter(If(nameAttr Is Nothing, keyProp.Name, nameAttr.Name), "=", "@Item1", _value)
                    Return _dc.Fetch(Of T)(generator.GetCommand()).FirstOrDefault
                End If
                Return Nothing
            End Get
        End Property
    End Class
End NameSpace