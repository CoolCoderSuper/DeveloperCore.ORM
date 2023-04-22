Imports DeveloperCore.ORM
Imports DeveloperCore.ORM.Attributes

Module Program

    Sub Main()
        Dim dc As New DataContext("Server=cch\codingcool;Database=SampleDB;Integrated Security=True;TrustServerCertificate=True") With {.EnableChangeTracking = True}
        Dim res As List(Of Person) = dc.Fetch(Of Person)("select * from [User]").ToList
        Dim assigments As List(Of Assignment) = res.First.Assignments.ToList
        'Dim objUser As New Person() With {.FullName = "Sup"}
        'dc.Insert(objUser)
        'dc.Delete(res.Last)
        'transaction stuff
        'dc.InsertOnSubmit(objUser)
        'dc.DeleteOnSubmit(res.Last)
        'dc.SubmitChanges()
        'dc.Query(Of User)(Function(x) x.Id > 3)
    End Sub

End Module

<TableName("User")>
Public Class Person

    <Identity>
    <Key>
    Public Property Id As Integer

    <ColumnName("Name")>
    Public Property FullName As String

    Public Property Assignments As IEnumerable(Of Assignment)

    <Ignore>
    Public Property Test As String

End Class

<ForeignKey("UserId")>
Public Class Assignment

    <Identity>
    <Key>
    <ColumnName("ID")>
    Public Property Id As Integer

    Public Property Name As String

    Public Property UserId As Integer

End Class