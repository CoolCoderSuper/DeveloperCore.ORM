Namespace Core
    Public Interface IDelete
        Function Table(tableName As String) As IDelete
        Function Filter(column As String, value As Object) As IDelete
        Function GetCommand() As ICommand
    End Interface
End Namespace