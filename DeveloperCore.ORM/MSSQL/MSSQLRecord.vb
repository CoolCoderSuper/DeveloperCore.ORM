Imports System.Data
Imports DeveloperCore.ORM.Core

Namespace MSSQL
    Public Class MSSQLRecord
        Implements IRecord
        Private ReadOnly _record As IDataRecord

        Public Sub New(record As IDataRecord)
            _record = record
        End Sub
        
        Public ReadOnly Property FieldCount As Integer Implements IRecord.FieldCount
            Get
                Return _record.FieldCount
            End Get
        End Property
        
        Public Function GetName(i As Integer) As String Implements IRecord.GetName
            Return _record.GetName(i)
        End Function
        
        Public ReadOnly Default Property Item(i As Integer) As Object Implements IRecord.Item
            Get
                Return _record(i)
            End Get
        End Property
        
        Public ReadOnly Default Property Item(col As String) As Object Implements IRecord.Item
            Get
                Return _record(col)
            End Get
        End Property
    End Class
End NameSpace