Namespace Core
    Public interface ITransaction
        ReadOnly Property Transaction As Object
        Sub Commit()
        Sub Rollback()
    end interface
End NameSpace