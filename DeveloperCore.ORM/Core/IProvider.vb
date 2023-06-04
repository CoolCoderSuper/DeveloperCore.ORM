Namespace Core
    Public interface IProvider
        ReadOnly Property Connection As Object
        Sub Connect()
        Sub Disconnect()
        Function Insert() As IInsert
        Function Update() As IUpdate
        Function Delete() As IDelete
        Function Transaction() As ITransaction
    end interface
End NameSpace