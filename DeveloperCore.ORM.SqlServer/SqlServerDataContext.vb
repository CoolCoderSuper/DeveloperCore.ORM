Imports DeveloperCore.ORM.Core

Public Class SqlServerDataContext
    Inherits DataContext

    Public Sub New(connStr As String)
        MyBase.New(New MSSQLConnection(connStr))
    End Sub
End Class