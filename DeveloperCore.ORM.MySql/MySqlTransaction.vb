Imports DeveloperCore.ORM.Core

Public Class MySqlTransaction
    Implements ITransaction
    Private ReadOnly _transaction As MySqlConnector.MySqlTransaction

    Public Sub New(transaction As MySqlConnector.MySqlTransaction)
        _transaction = transaction
    End Sub

    Public ReadOnly Property Transaction As Object Implements ITransaction.Transaction
        Get
            return _transaction
        End Get
    End Property

    Public Sub Commit() Implements ITransaction.Commit
        _transaction.Commit()
    End Sub
        
    Public Sub Rollback() Implements ITransaction.Rollback
        _transaction.Rollback()
    End Sub
End Class