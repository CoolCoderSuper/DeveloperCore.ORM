Imports System.ComponentModel
Imports DeveloperCore.ORM.Attributes
Imports DeveloperCore.ORM.Core
Imports DeveloperCore.ORM.MSSQL
Imports DeveloperCore.ORM.Relation

Module Program
    Private Const ConnectionString As String = "Server=cch\codingcool;Database=SampleDB;Integrated Security=True;TrustServerCertificate=True"

    Sub Main()
        Dim dc As DataContext = New SqlDataContext(ConnectionString) With {.EnableChangeTracking = True}
        Dim res As List(Of Person) = dc.Fetch(Of Person)("select * from [User]").ToList
        Dim assignments As List(Of Assignment) = res.First.Assignments.ToList
        'Dim u = assignments.First.User.Value
        'dc.SubmitChanges()
        'Dim sRes As List(Of Person) = Person.Fetch("select * from [User]", ConnectionString)
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
<GeneratedQuery>
Public Class Person
    Implements INotifyPropertyChanged

    Private _FullName As String

    <Identity>
    <Key>
    Public Property Id As Integer

    <ColumnName("Name")>
    Public Property FullName As String
        Get
            Return _FullName
        End Get
        Set
            _FullName = Value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(NameOf(FullName)))
        End Set
    End Property

    <Ignore>
    Public Property Assignments As IEnumerable(Of Assignment)

    <Ignore>
    Public Property Test As String

    Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged
End Class

<ForeignKey("UserId")>
<GeneratedQuery>
Public Class Assignment
    Implements INotifyPropertyChanged

    Private _Name As String
    Private _UserId As Integer

    <Identity>
    <Key>
    <ColumnName("ID")>
    Public Property Id As Integer

    Public Property Name As String
        Get
            Return _Name
        End Get
        Set
            _Name = Value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(NameOf(Name)))
        End Set
    End Property

    Public Property UserId As Integer
        Get
            Return _UserId
        End Get
        Set
            _UserId = Value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(NameOf(UserId)))
        End Set
    End Property

    Public Property User As ParentRef(Of Person)

    Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged
End Class