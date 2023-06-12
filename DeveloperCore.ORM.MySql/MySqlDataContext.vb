Imports DeveloperCore.ORM.Core

Public Class MySqlDataContext
    Inherits DataContext

    Public Sub New(connStr As String)
        MyBase.New(New MySqlConnection(connStr))
    End Sub
End Class