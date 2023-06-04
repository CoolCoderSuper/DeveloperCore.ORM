Imports DeveloperCore.ORM.Core
Imports Microsoft.Data.SqlClient

Namespace MSSQL
    Public Class MSSQLTransaction
        Implements ITransaction
        Private ReadOnly _transaction As SqlTransaction

        Public Sub New(transaction As SqlTransaction)
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
End Namespace