Imports System.ComponentModel

Public Class AdvancedBindingList(Of T)
    Inherits BindingList(Of T)

    Private ReadOnly _dc As IDataContext

    Public Sub New(l As List(Of T), dc As IDataContext)
        MyBase.New(l)
        _dc = dc
    End Sub

    Private Sub AdvancedBindingList_ListChanged(sender As Object, e As ListChangedEventArgs) Handles Me.ListChanged
        Select Case e.ListChangedType
            Case ListChangedType.ItemAdded
                _dc.InsertOnSubmit(Item(e.NewIndex))
            Case ListChangedType.ItemDeleted
                _dc.DeleteOnSubmit(Item(e.NewIndex))
            Case ListChangedType.ItemChanged
                Dim obj As T = Item(e.NewIndex)
                If Not _dc.ChangeSet.Updates.Contains(obj) Then _dc.UpdateOnSubmit(obj)
        End Select
    End Sub

    Public Sub Save()
        _dc.SubmitChanges()
    End Sub

End Class