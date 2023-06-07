Namespace Core
    Public Interface IReader
        Function Read() As Boolean
        Function GetRecord() As IRecord
    End Interface
End Namespace