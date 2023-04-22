Namespace Attributes

    <AttributeUsage(AttributeTargets.Class)>
    Public Class ForeignKeyAttribute
        Inherits Attribute

        Public ReadOnly Property Column As String

        Public Sub New(column As String)
            Me.Column = column
        End Sub

    End Class

End Namespace