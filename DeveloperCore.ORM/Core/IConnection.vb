Namespace Core
    Public Interface IConnection
        ReadOnly Property Connection As Object
        Sub Connect()
        Sub Disconnect()
        Function Insert() As IInsert
        Function Update() As IUpdate
        Function Delete() As IDelete
        Function Transaction() As ITransaction
        Function Command() As ICommand
        Function Generate() As IQueryGenerator
    End Interface
End Namespace