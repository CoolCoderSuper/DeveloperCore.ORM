Namespace Attributes

    <AttributeUsage(AttributeTargets.Class)>
    Public Class TableNameAttribute
        Inherits Attribute

        Public ReadOnly Property Name As String

        Public Sub New(name As String)
            Me.Name = name
        End Sub

    End Class

End Namespace