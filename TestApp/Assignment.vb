Imports System.ComponentModel
Imports DeveloperCore.ORM.Attributes
Imports DeveloperCore.ORM.Relation

<ForeignKey("UserId")>
<GeneratedQuery>
Public Class Assignment
    Implements INotifyPropertyChanged

    Private _name As String
    Private _userId As Integer

    <Identity>
    <Key>
    <ColumnName("ID")>
    Public Property Id As Integer

    Public Property Name As String
        Get
            Return _name
        End Get
        Set
            _name = Value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(NameOf(Name)))
        End Set
    End Property

    Public Property UserId As Integer
        Get
            Return _userId
        End Get
        Set
            _userId = Value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(NameOf(UserId)))
        End Set
    End Property

    Public Property User As ParentRef(Of Person)

    Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged
End Class