Namespace Core
    Public Interface IRecord
        ReadOnly Property FieldCount As Integer
        ReadOnly Default Property Item(i As Integer) As Object
        ReadOnly Default Property Item(col As String) As Object
        Function GetName(i As Integer) As String
    End Interface
End Namespace