Imports DeveloperCore.ORM.Core
Imports DeveloperCore.ORM.MySql
Imports DeveloperCore.ORM.SqlServer
'TODO: Modularize it more
Module Program
    Private Const SqlServerConnectionString As String = "Server=cch\codingcool;Database=SampleDB;Integrated Security=True;TrustServerCertificate=True"
    Private Const MySqlConnectionString As String = "Server=localhost;Database=sample;Uid=root;Pwd=k458wstm;"
    
    Sub Main()
        SqlServer()
    End Sub
    
    Private Sub MySql()
        Dim dc As DataContext = New MySqlDataContext(MySqlConnectionString) With {.EnableChangeTracking = True}
        Dim res As List(Of Person) = dc.Fetch(Of Person)("select * from user").ToList
        Dim p As New Person With {.FullName = "Bill"}
        dc.Insert(p)
    End Sub

    Private Sub SqlServer()
        Dim dc As DataContext = New SqlServerDataContext(SqlServerConnectionString) With {.EnableChangeTracking = True}
        Dim res As List(Of Person) = dc.Fetch(Of Person)("select * from [User]").ToList
        Dim p As New Person With {.FullName = "Bill"}
        dc.Insert(p)
    End Sub
End Module