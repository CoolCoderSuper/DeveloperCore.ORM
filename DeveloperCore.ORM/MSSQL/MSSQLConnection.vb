Imports DeveloperCore.ORM.Core
Imports Microsoft.Data.SqlClient

Namespace MSSQL
    Public Class MSSQLConnection
        Implements IConnection
        Private ReadOnly _conn As SqlConnection
        
        Public Sub New(connectionString As String)
            _conn = New SqlConnection(connectionString)
        End Sub

        Public ReadOnly Property Connection As Object Implements IConnection.Connection
            Get
                return _conn
            End Get
        End Property

        Public Sub Connect() Implements IConnection.Connect
            _conn.Open()
        End Sub
        
        Public Sub Disconnect() Implements IConnection.Disconnect
            _conn.Close()
        End Sub
        
        Public Function Insert() As IInsert Implements IConnection.Insert
            Return New MSSQLInsert()
        End Function
        
        Public Function Update() As IUpdate Implements IConnection.Update
            Return New MSSQLUpdate()
        End Function
        
        Public Function Delete() As IDelete Implements IConnection.Delete
            Return New MSSQLDelete()
        End Function
        
        Public Function Transaction() As ITransaction Implements IConnection.Transaction
            Return New MSSQLTransaction(_conn.BeginTransaction())
        End Function
    End Class
End NameSpace