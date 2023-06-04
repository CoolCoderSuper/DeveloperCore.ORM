Imports DeveloperCore.ORM.Core
Imports Microsoft.Data.SqlClient

Namespace MSSQL
    Public Class MSSQLProvider
        Implements IProvider
        Private ReadOnly _conn As SqlConnection
        
        Public Sub New(connectionString As String)
            _conn = New SqlConnection(connectionString)
        End Sub

        Public ReadOnly Property Connection As Object Implements IProvider.Connection
            Get
                return _conn
            End Get
        End Property

        Public Sub Connect() Implements IProvider.Connect
            _conn.Open()
        End Sub
        
        Public Sub Disconnect() Implements IProvider.Disconnect
            _conn.Close()
        End Sub
        
        Public Function Insert() As IInsert Implements IProvider.Insert
            Return New MSSQLInsert()
        End Function
        
        Public Function Update() As IUpdate Implements IProvider.Update
            Return New MSSQLUpdate()
        End Function
        
        Public Function Delete() As IDelete Implements IProvider.Delete
            Return New MSSQLDelete()
        End Function
        
        Public Function Transaction() As ITransaction Implements IProvider.Transaction
            Return New MSSQLTransaction(_conn.BeginTransaction())
        End Function
    End Class
End NameSpace