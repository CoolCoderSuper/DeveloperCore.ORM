Namespace Core
    Public interface IRecord
        ReadOnly Property FieldCount As Integer
        ReadOnly Default Property Item(i As Integer) As Object
        Function GetName(i As Integer) As String
    end interface
End NameSpace