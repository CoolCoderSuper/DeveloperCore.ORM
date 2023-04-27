Imports System.ComponentModel
Imports System.Data
Imports System.Reflection
Imports System.Reflection.Emit
Imports System.Text
Imports DeveloperCore.ORM.Attributes
Imports Microsoft.Data.SqlClient

Public Class DataContext
    ReadOnly _conn As SqlConnection
    Dim _changeSet As New ChangeSet
    ReadOnly _propDelegateCache As New Dictionary(Of String, Dictionary(Of String, Action(Of Object, Object)))
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

    Public Function Fetch(sql As String, type As Type, ParamArray params As Object()) As IEnumerable(Of Object)
        Try
            If EnableChangeTracking AndAlso type.GetInterface(GetType(INotifyPropertyChanged).FullName) Is Nothing Then Throw New Exception("Change tracking is enabled, but the type does not implement INotifyPropertyChanged")
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
            Dim sdr As SqlDataReader = cmd.ExecuteReader
            Dim props As New Dictionary(Of String, PropertyInfo)
            Dim allProps As PropertyInfo() = type.GetProperties
            Dim keyProp As PropertyInfo = allProps.FirstOrDefault(Function(x) x.CustomAttributes.Any(Function(y) y.AttributeType.FullName = "DeveloperCore.ORM.Attributes.KeyAttribute"))
            Dim fkProps As PropertyInfo() = allProps.Where(Function(x) x.PropertyType.FullName.StartsWith("System.Collections.Generic.IEnumerable")).ToArray
            Dim propDelegates As Dictionary(Of String, Action(Of Object, Object))
            If _propDelegateCache.ContainsKey(type.FullName) Then
                propDelegates = _propDelegateCache(type.FullName)
            Else
                propDelegates = New Dictionary(Of String, Action(Of Object, Object))
                For Each prop As PropertyInfo In allProps
                    Dim method As New DynamicMethod($"SetValue{prop.Name}", Nothing, {GetType(Object), GetType(Object)})
                    Dim il As ILGenerator = method.GetILGenerator
                    il.Emit(OpCodes.Ldarg_0)
                    il.Emit(OpCodes.Castclass, type)
                    il.Emit(OpCodes.Ldarg_1)
                    If prop.PropertyType.IsValueType Then
                        il.Emit(OpCodes.Unbox_Any, prop.PropertyType)
                    Else
                        il.Emit(OpCodes.Castclass, prop.PropertyType)
                    End If
                    il.Emit(OpCodes.Callvirt, prop.GetSetMethod)
                    il.Emit(OpCodes.Ret)
                    Dim del As Action(Of Object, Object) = method.CreateDelegate(GetType(Action(Of Object, Object)))
                    propDelegates.Add(prop.Name, del)
                Next
                _propDelegateCache.Add(type.FullName, propDelegates)
            End If
            Dim nameCache As New Dictionary(Of Integer, String)
            While sdr.Read
                Dim record As IDataRecord = sdr
                Dim obj As Object = Activator.CreateInstance(type)
                Dim keyValue As String = Nothing
                For i As Integer = 0 To record.FieldCount - 1
                    Dim name As String = record.GetName(i)
                    Dim prop As PropertyInfo = Nothing
                    If props.ContainsKey(name) Then
                        prop = props(name)
                    Else
                        prop = GetProp(allProps, name)
                        props.Add(name, prop)
                    End If
                    If prop = keyProp Then keyValue = record(i)
                    propDelegates(prop.Name)(obj, record(i))
                Next
                For Each fkProp As PropertyInfo In fkProps
                    'TODO: Optimize
                    Dim fk As Object = Activator.CreateInstance(Type.GetType("DeveloperCore.ORM.ForeignKeyEnumerable`1").MakeGenericType(fkProp.PropertyType.GetGenericArguments), Me, keyValue)
                    propDelegates(fkProp.Name)(obj, fk)
                Next
                If EnableChangeTracking Then
                    Dim notify As INotifyPropertyChanged = obj
                    AddHandler notify.PropertyChanged, AddressOf OnPropertyChanged
                End If
                results.Add(obj)
            End While
            Return results
        Finally
            _conn.Close()
        End Try
        Return New List(Of Object)
    End Function

    Private Sub OnPropertyChanged(sender As Object, e As PropertyChangedEventArgs)
        _changeSet.Updates.Add(sender)
    End Sub

    Private Shared Function GetProp(props As PropertyInfo(), dc As String) As PropertyInfo
        Dim resProp As PropertyInfo
        For Each prop As PropertyInfo In props
            Dim attr As CustomAttributeData = prop.CustomAttributes.FirstOrDefault(Function(x) x.AttributeType.FullName = "DeveloperCore.ORM.Attributes.ColumnNameAttribute")
            If attr IsNot Nothing AndAlso attr.ConstructorArguments(0).Value.ToString = dc Then
                resProp = prop
            End If
        Next
        resProp = If(resProp, props.FirstOrDefault(Function(x) x.Name = dc))
        Dim ignoreAttr As CustomAttributeData = resProp.CustomAttributes.FirstOrDefault(Function(x) x.AttributeType.FullName = "DeveloperCore.ORM.Attributes.IgnoreAttribute")
        Return If(ignoreAttr Is Nothing, resProp, Nothing)
    End Function

    Public Function Fetch(Of T)(sql As String, ParamArray params As Object()) As IEnumerable(Of T)
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
        Dim tableNameAttr As TableNameAttribute = type.GetCustomAttribute(Of TableNameAttribute)
        Dim props As PropertyInfo() = type.GetProperties.Where(Function(x) x.GetCustomAttribute(Of IgnoreAttribute) Is Nothing AndAlso x.GetCustomAttribute(Of IdentityAttribute) Is Nothing).ToArray
        Dim sql As New StringBuilder($"insert into [{If(tableNameAttr Is Nothing, type.Name, tableNameAttr.Name)}] values(")
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
        Dim tableNameAttr As TableNameAttribute = type.GetCustomAttribute(Of TableNameAttribute)
        Dim keyProp As PropertyInfo = type.GetProperties.FirstOrDefault(Function(x) x.GetCustomAttribute(Of KeyAttribute) IsNot Nothing)
        Dim keyNameAttr As ColumnNameAttribute = keyProp.GetCustomAttribute(Of ColumnNameAttribute)
        Dim props As PropertyInfo() = type.GetProperties.Where(Function(x) x.GetCustomAttribute(Of IgnoreAttribute) Is Nothing AndAlso x.GetCustomAttribute(Of IdentityAttribute) Is Nothing).ToArray
        Dim sql As New StringBuilder($"update [{If(tableNameAttr Is Nothing, type.Name, tableNameAttr.Name)}] set ")
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
        Dim tableNameAttr As TableNameAttribute = type.GetCustomAttribute(Of TableNameAttribute)
        Dim keyProp As PropertyInfo = type.GetProperties.FirstOrDefault(Function(x) x.GetCustomAttribute(Of KeyAttribute) IsNot Nothing)
        Dim cmd As New SqlCommand($"delete from [{If(tableNameAttr Is Nothing, type.Name, tableNameAttr.Name)}] where [{keyProp.Name}]=@{keyProp.Name}", _conn, transaction)
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

End Class