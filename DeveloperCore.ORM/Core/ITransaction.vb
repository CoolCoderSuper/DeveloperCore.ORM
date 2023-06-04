Namespace Core
    Public Interface ITransaction
        ReadOnly Property Transaction As Object
        Sub Commit()
        Sub Rollback()
    End Interface
End NameSpace