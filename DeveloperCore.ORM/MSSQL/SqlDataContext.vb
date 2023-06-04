Imports DeveloperCore.ORM.Core

Namespace MSSQL
    Public Class SqlDataContext
        Inherits DataContext

        Public Sub New(connStr As String)
            MyBase.New(New MSSQLConnection(connStr))
        End Sub
    End Class
End NameSpace