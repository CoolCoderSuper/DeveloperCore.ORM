Imports System.Data
Imports DeveloperCore.ORM.Core
Imports Microsoft.Data.SqlClient

Namespace MSSQL
    Public Class MSSQLReader
        Implements IReader
        Private ReadOnly _reader As SqlDataReader

        Public Sub New(reader As SqlDataReader)
            _reader = reader
        End Sub
        
        Public Function Read() As Boolean Implements IReader.Read
            Return _reader.Read()
        End Function
        
        Public Function GetRecord() As IDataRecord Implements IReader.GetRecord
            Return _reader
        End Function
    End Class
End NameSpace