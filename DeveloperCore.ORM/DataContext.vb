Imports System.Data
Imports System.Reflection
Imports System.Text
Imports Microsoft.Data.SqlClient
Imports DeveloperCore.ORM.Columns
Imports System.Linq.Expressions

Public Class DataContext
    ReadOnly _conn As SqlConnection
    Dim _changeSet As New ChangeSet
    Public ReadOnly Property ConnectionString As String
    Public Property EnableChangeTracking As Boolean = False

    Public ReadOnly Property ChangeSet As ChangeSet
        Get
            Return _changeSet
        End Get
    End Property

    Public Sub New(connectionString As String)
        Me.ConnectionString = connectionString
        _conn = New SqlConnection(connectionString)
    End Sub

    Public Function Fetch(sql As String, type As Type, ParamArray params As Object()) As List(Of Object)
        Try
            _conn.Open()
            Dim results As New List(Of Object)
            Dim cmd As New SqlCommand(sql, _conn)
            For i As Integer = 0 To params.Length - 1
                Dim param As SqlParameter = cmd.CreateParameter
                With param
                    .ParameterName = $"@Item{i + 1}"
                    .Value = params(i)
                End With
                cmd.Parameters.Add(param)
            Next
            Dim adp As New SqlDataAdapter(cmd)
            Dim dt As New DataTable
            adp.Fill(dt)
            For Each dr As DataRow In dt.Rows
                Dim obj As Object = Activator.CreateInstance(type)
                For Each dc As DataColumn In dt.Columns
                    Dim prop As PropertyInfo = GetProp(type, dc.ColumnName)
                    prop?.SetValue(obj, dr(dc.ColumnName))
                Next
                If EnableChangeTracking Then _changeSet.Updates.Add(obj)
                results.Add(obj)
            Next
            Return results
        Finally
            _conn.Close()
        End Try
        Return New List(Of Object)
    End Function

    Private Shared Function GetProp(type As Type, dc As String) As PropertyInfo
        Dim resProp As PropertyInfo
        For Each prop As PropertyInfo In type.GetProperties
            Dim attr As ColumnNameAttribute = prop.GetCustomAttribute(Of ColumnNameAttribute)
            If attr IsNot Nothing AndAlso attr.Name = dc Then
                resProp = prop
            End If
        Next
        resProp = If(resProp, type.GetProperty(dc))
        Dim ignoreAttr As IgnoreAttribute = resProp.GetCustomAttribute(Of IgnoreAttribute)
        Return If(ignoreAttr Is Nothing, resProp, Nothing)
    End Function

    Public Function Fetch(Of T)(sql As String, ParamArray params As Object()) As List(Of T)
        Return Fetch(sql, GetType(T), params).Cast(Of T).ToList
    End Function

    Public Sub Insert(obj As Object, type As Type)
        Dim transaction As SqlTransaction
        Try
            _conn.Open()
            transaction = _conn.BeginTransaction
            Insert(obj, type, transaction)
            transaction.Commit()
        Catch ex As Exception
            Try
                transaction?.Rollback()
            Catch ex1 As Exception
                Throw
            End Try
            Throw
        Finally
            _conn.Close()
        End Try
    End Sub

    Public Sub Insert(obj As Object, type As Type, transaction As SqlTransaction)
        Dim props As PropertyInfo() = type.GetProperties.Where(Function(x) x.GetCustomAttribute(Of IgnoreAttribute) Is Nothing AndAlso x.GetCustomAttribute(Of IdentityAttribute) Is Nothing).ToArray
        Dim sql As New StringBuilder($"insert into [{type.Name}] values(")
        Dim cmd As New SqlCommand() With {.Connection = _conn, .Transaction = transaction}
        For Each prop As PropertyInfo In props
            sql.Append($"@{prop.Name}{If(prop Is props.Last, ")", ",")}")
            Dim param As SqlParameter = cmd.CreateParameter
            With param
                .ParameterName = $"@{prop.Name}"
                .Value = prop.GetValue(obj)
            End With
            cmd.Parameters.Add(param)
        Next
        cmd.CommandText = sql.ToString
        cmd.ExecuteNonQuery()
    End Sub

    Public Sub Insert(Of T)(obj As T)
        Insert(obj, GetType(T))
    End Sub

    Public Sub Update(obj As Object, type As Type)
        Dim transaction As SqlTransaction
        Try
            _conn.Open()
            transaction = _conn.BeginTransaction()
            Update(obj, type, transaction)
            transaction.Commit()
        Catch ex As Exception
            Try
                transaction?.Rollback()
            Catch ex1 As Exception
                Throw
            End Try
            Throw
        Finally
            _conn.Close()
        End Try
    End Sub

    Public Sub Update(obj As Object, type As Type, transaction As SqlTransaction)
        Dim keyProp As PropertyInfo = type.GetProperties.FirstOrDefault(Function(x) x.GetCustomAttribute(Of KeyAttribute) IsNot Nothing)
        Dim keyNameAttr As ColumnNameAttribute = keyProp.GetCustomAttribute(Of ColumnNameAttribute)
        Dim props As PropertyInfo() = type.GetProperties.Where(Function(x) x.GetCustomAttribute(Of IgnoreAttribute) Is Nothing AndAlso x.GetCustomAttribute(Of IdentityAttribute) Is Nothing).ToArray
        Dim sql As New StringBuilder($"update [{type.Name}] set ")
        Dim cmd As New SqlCommand() With {.Connection = _conn, .Transaction = transaction}
        For Each prop As PropertyInfo In props
            Dim nameAttr As ColumnNameAttribute = prop.GetCustomAttribute(Of ColumnNameAttribute)
            sql.Append($"{If(nameAttr Is Nothing, prop.Name, nameAttr.Name)}=@{prop.Name}{If(prop Is props.Last, " ", ",")}")
            Dim param As SqlParameter = cmd.CreateParameter
            With param
                .ParameterName = $"@{prop.Name}"
                .Value = prop.GetValue(obj)
            End With
            cmd.Parameters.Add(param)
        Next
        Dim filterParam As SqlParameter = cmd.CreateParameter()
        With filterParam
            .ParameterName = $"@{keyProp.Name}"
            .Value = keyProp.GetValue(obj)
        End With
        cmd.Parameters.Add(filterParam)
        sql.Append($"where {If(keyNameAttr Is Nothing, keyProp.Name, keyNameAttr.Name)}=@{keyProp.Name}")
        cmd.CommandText = sql.ToString
        cmd.ExecuteNonQuery()
    End Sub

    Public Sub Update(Of T)(obj As T)
        Update(obj, GetType(T))
    End Sub

    Public Sub Delete(obj As Object, type As Type)
        Dim transaction As SqlTransaction
        Try
            _conn.Open()
            transaction = _conn.BeginTransaction
            Delete(obj, type, transaction)
            transaction.Commit()
        Catch ex As Exception
            Try
                transaction?.Rollback()
            Catch ex1 As Exception
                Throw
            End Try
            Throw
        Finally
            _conn.Close()
        End Try
    End Sub

    Public Sub Delete(obj As Object, type As Type, transaction As SqlTransaction)
        Dim keyProp As PropertyInfo = type.GetProperties.FirstOrDefault(Function(x) x.GetCustomAttribute(Of KeyAttribute) IsNot Nothing)
        Dim cmd As New SqlCommand($"delete from [{type.Name}] where [{keyProp.Name}]=@{keyProp.Name}", _conn, transaction)
        Dim param As SqlParameter = cmd.CreateParameter
        With param
            .ParameterName = $"@{keyProp.Name}"
            .Value = keyProp.GetValue(obj)
        End With
        cmd.Parameters.Add(param)
        cmd.ExecuteNonQuery()
    End Sub

    Public Sub Delete(Of T)(obj As T)
        Delete(obj, GetType(T))
    End Sub

    Public Sub InsertOnSubmit(obj As Object)
        _changeSet.Inserts.Add(obj)
    End Sub

    Public Sub UpdateOnSubmit(obj As Object)
        _changeSet.Updates.Add(obj)
    End Sub

    Public Sub DeleteOnSubmit(obj As Object)
        _changeSet.Deletes.Add(obj)
    End Sub

    Public Sub SubmitChanges()
        Dim transaction As SqlTransaction
        Try
            _conn.Open()
            transaction = _conn.BeginTransaction
            For Each insertObj As Object In _changeSet.Inserts
                Insert(insertObj, insertObj.GetType, transaction)
            Next
            For Each updateObj As Object In _changeSet.Updates
                Update(updateObj, updateObj.GetType, transaction)
            Next
            For Each deleteObj As Object In _changeSet.Deletes
                Delete(deleteObj, deleteObj.GetType, transaction)
            Next
            transaction.Commit()
            _changeSet = New ChangeSet
        Catch ex As Exception
            Try
                transaction?.Rollback()
            Catch ex1 As Exception
                Throw
            End Try
            Throw
        Finally
            _conn.Close()
        End Try
    End Sub

    Public Function Query(Of T)(exp As Expression(Of Func(Of T, Boolean))) As List(Of T)
        While exp.CanReduce
            exp = exp.Reduce
        End While
    End Function

End Class