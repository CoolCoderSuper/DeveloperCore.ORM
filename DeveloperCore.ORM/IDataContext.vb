Imports Microsoft.Data.SqlClient

Public Interface IDataContext
    ReadOnly Property ChangeSet As ChangeSet
    ReadOnly Property ConnectionString As String
    Property EnableChangeTracking As Boolean
    Sub Delete(obj As Object, type As Type)
    Sub Delete(obj As Object, type As Type, transaction As SqlTransaction)
    Sub Delete(Of T)(obj As T)
    Sub DeleteOnSubmit(obj As Object)
    Sub Insert(obj As Object, type As Type)
    Sub Insert(obj As Object, type As Type, transaction As SqlTransaction)
    Sub Insert(Of T)(obj As T)
    Sub InsertOnSubmit(obj As Object)
    Sub SubmitChanges()
    Sub Update(obj As Object, type As Type)
    Sub Update(obj As Object, type As Type, transaction As SqlTransaction)
    Sub Update(Of T)(obj As T)
    Sub UpdateOnSubmit(obj As Object)
    Function Fetch(Of T)(sql As String, ParamArray params() As Object) As IEnumerable(Of T)
    Function Fetch(sql As String, type As Type, ParamArray params() As Object) As IEnumerable(Of Object)
End Interface
