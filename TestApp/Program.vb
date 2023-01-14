Imports DeveloperCore.ORM

Module Program

    Sub Main(args As String())
        Dim dc As New DataContext("Server=cch\codingcool;Database=SampleDB;Integrated Security=True;TrustServerCertificate=True")
        Dim res As List(Of User) = dc.Query(Of User)("select * from [User]")
        Dim objUser As New User() With {.FullName = "Sup"}
        'dc.Insert(objUser)
        'dc.Delete(res.Last)
        'transaction stuff
        dc.InsertOnSubmit(objUser)
        dc.DeleteOnSubmit(res.Last)
        dc.SubmitChanges()
    End Sub

End Module

Public Class User

    <Identity>
    <Key>
    Public Property Id As Integer

    <ColumnName("Name")>
    Public Property FullName As String

    <Ignore>
    Public Property Test As String

End Class