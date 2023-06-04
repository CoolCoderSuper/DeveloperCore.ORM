Namespace Core
    Public interface ICommand
        Property Connection As IConnection
        Property Transaction As ITransaction
        ReadOnly Property Parameters As Dictionary(Of String, Object)
        Property CommandText As String
        Function Execute() As Integer
    end interface
End Namespace