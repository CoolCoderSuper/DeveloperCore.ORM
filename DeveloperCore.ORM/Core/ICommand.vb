Namespace Core
    Public Interface ICommand
        Property Connection As IConnection
        Property Transaction As ITransaction
        ReadOnly Property Parameters As Dictionary(Of String, Object)
        Property CommandText As String
        Function Execute() As Integer
    End Interface
End Namespace