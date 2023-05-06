Public Class SqlDataContext
    Inherits DataContext

    Public Sub New(connStr As String)
        MyBase.New(connStr)
    End Sub
End Class