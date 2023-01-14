Imports DeveloperCore.ORM
Imports DeveloperCore.ORM.Columns

Module Program

    Sub Main(args As String())
        Dim dc As New DataContext("Server=cch\codingcool;Database=SampleDB;Integrated Security=True;TrustServerCertificate=True") With {.EnableChangeTracking = True}
        Dim res As List(Of User) = dc.Fetch(Of User)("select * from [User] where Id > @Item1 or Id = @Item2", 3, 2)
        'Dim objUser As New User() With {.FullName = "Sup"}
        'dc.Insert(objUser)
        'dc.Delete(res.Last)
        'transaction stuff
        'dc.InsertOnSubmit(objUser)
        'dc.DeleteOnSubmit(res.Last)
        'dc.SubmitChanges()
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