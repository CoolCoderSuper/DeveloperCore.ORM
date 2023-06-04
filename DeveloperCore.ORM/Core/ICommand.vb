Namespace Core
    Public interface ICommand
        Property Connection As IProvider
        Property Transaction As ITransaction
        Function Execute() As Integer
    end interface
End Namespace