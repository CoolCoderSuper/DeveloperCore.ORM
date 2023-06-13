Imports System.ComponentModel
Imports DeveloperCore.ORM.Attributes

<TableName("User")>
<GeneratedQuery>
Public Class Person
    Implements INotifyPropertyChanged

    Private _fullName As String

    <Identity>
    <Key>
    Public Property Id As Integer

    <ColumnName("Name")>
    Public Property FullName As String
        Get
            Return _fullName
        End Get
        Set
            _fullName = Value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(NameOf(FullName)))
        End Set
    End Property

    <Ignore>
    Public Property Assignments As IEnumerable(Of Assignment)

    <Ignore>
    Public Property Test As String

    Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged
End Class