<AttributeUsage(AttributeTargets.Property)>
Public Class ColumnNameAttribute
    Inherits Attribute

    Public ReadOnly Property Name As String

    Public Sub New(name As String)
        Me.Name = name
    End Sub

End Class