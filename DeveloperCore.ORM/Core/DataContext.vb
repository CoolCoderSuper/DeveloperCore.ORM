Imports System.ComponentModel
Imports System.Data
Imports System.Reflection
Imports System.Reflection.Emit
Imports DeveloperCore.ORM.Attributes
Imports Microsoft.Data.SqlClient

Namespace Core

    'TODO: FK editing like in linq2sql
    'TODO: Fetch should use delayed query like ForeignKeyEnumerable
    Public MustInherit Class DataContext
        Dim _changeSet As New ChangeSet
        ReadOnly _propDelegateCache As New Dictionary(Of String, Dictionary(Of String, Action(Of Object, Object)))
        ReadOnly _connection As IConnection
        
        Public Property EnableChangeTracking As Boolean = False

        Public ReadOnly Property ChangeSet As ChangeSet
            Get
                Return _changeSet
            End Get
        End Property

        Public Sub New(connection As IConnection)
            _connection = connection
        End Sub

        Public Overridable Function Fetch(sql As String, type As Type, ParamArray params As Object()) As IEnumerable(Of Object)
            Try
                If EnableChangeTracking AndAlso type.GetInterface(GetType(INotifyPropertyChanged).FullName) Is Nothing Then Throw New Exception("Change tracking is enabled, but the type does not implement INotifyPropertyChanged")
                _connection.Connect()
                Dim results As New List(Of Object)
                Dim cmd As New SqlCommand(sql, _connection.Connection)
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
                While sdr.Read
                    Dim record As IDataRecord = sdr
                    Dim obj As Object = Activator.CreateInstance(type)
                    Dim keyValue As String = Nothing
                    For i As Integer = 0 To record.FieldCount - 1
                        Dim name As String = record.GetName(i)
                        Dim prop As PropertyInfo
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
                        Dim args As Type() = fkProp.PropertyType.GenericTypeArguments
                        If args(0).CustomAttributes.Any(Function(x) x.AttributeType.FullName = "DeveloperCore.ORM.Attributes.ForeignKeyAttribute") Then
                            'TODO: Optimize
                            Dim fk As Object = Activator.CreateInstance(Type.GetType("DeveloperCore.ORM.Relation.ForeignKeyEnumerable`1").MakeGenericType(args), Me, keyValue)
                            propDelegates(fkProp.Name)(obj, fk)
                        End If
                    Next
                    'TODO: Optimize
                    For Each pProp As PropertyInfo In allProps.Where(Function(x) x.PropertyType.FullName.StartsWith("DeveloperCore.ORM.Relation.ParentRef`1"))
                        Dim fkAttr As ForeignKeyAttribute = type.GetCustomAttribute(Of ForeignKeyAttribute)()
                        If fkAttr IsNot Nothing Then
                            Dim p As Object = Activator.CreateInstance(Type.GetType("DeveloperCore.ORM.Relation.ParentRef`1").MakeGenericType(pProp.PropertyType.GenericTypeArguments), Me, record(fkAttr.Column).ToString)
                            propDelegates(pProp.Name)(obj, p)
                        End If
                    Next
                    If EnableChangeTracking Then
                        Dim notify As INotifyPropertyChanged = obj
                        AddHandler notify.PropertyChanged, AddressOf OnPropertyChanged
                    End If
                    results.Add(obj)
                End While
                Return results
            Finally
                _connection.Disconnect()
            End Try
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

        Public Overridable Sub Insert(obj As Object, type As Type)
            Dim transaction As ITransaction
            Try
                _connection.Connect()
                transaction = _connection.Transaction()
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
                _connection.Disconnect()
            End Try
        End Sub

        Public Overridable Sub Insert(obj As Object, type As Type, transaction As ITransaction)
            Dim tableNameAttr As TableNameAttribute = type.GetCustomAttribute(Of TableNameAttribute)
            Dim props As PropertyInfo() = type.GetProperties.Where(Function(x) x.GetCustomAttribute(Of IgnoreAttribute) Is Nothing AndAlso x.GetCustomAttribute(Of IdentityAttribute) Is Nothing).ToArray
            Dim insert As IInsert = _connection.Insert()
            insert.Table(If(tableNameAttr Is Nothing, type.Name, tableNameAttr.Name))
            For Each prop As PropertyInfo In props
                insert.Set(prop.GetValue(obj))
            Next
            With insert.GetCommand()
                .Connection = _connection
                .Transaction = transaction
                .Execute()
            End With
        End Sub

        Public Sub Insert(Of T)(obj As T)
            Insert(obj, GetType(T))
        End Sub

        Public Overridable Sub Update(obj As Object, type As Type)
            Dim transaction As ITransaction
            Try
                _connection.Connect()
                transaction = _connection.Transaction()
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
                _connection.Disconnect()
            End Try
        End Sub

        Public Overridable Sub Update(obj As Object, type As Type, transaction As ITransaction)
            Dim tableNameAttr As TableNameAttribute = type.GetCustomAttribute(Of TableNameAttribute)
            Dim keyProp As PropertyInfo = type.GetProperties.FirstOrDefault(Function(x) x.GetCustomAttribute(Of KeyAttribute) IsNot Nothing)
            Dim keyNameAttr As ColumnNameAttribute = keyProp.GetCustomAttribute(Of ColumnNameAttribute)
            Dim props As PropertyInfo() = type.GetProperties.Where(Function(x) x.GetCustomAttribute(Of IgnoreAttribute) Is Nothing AndAlso x.GetCustomAttribute(Of IdentityAttribute) Is Nothing).ToArray
            Dim update As IUpdate = _connection.Update()
            update.Table(If(tableNameAttr Is Nothing, type.Name, tableNameAttr.Name))
            For Each prop As PropertyInfo In props
                Dim nameAttr As ColumnNameAttribute = prop.GetCustomAttribute(Of ColumnNameAttribute)
                update.Set(If(nameAttr Is Nothing, prop.Name, nameAttr.Name), prop.GetValue(obj))
            Next
            update.Filter(If(keyNameAttr Is Nothing, keyProp.Name, keyNameAttr.Name), keyProp.GetValue(obj))
            With update.GetCommand()
                .Connection = _connection
                .Transaction = transaction
                .Execute()
            End With
        End Sub

        Public Sub Update(Of T)(obj As T)
            Update(obj, GetType(T))
        End Sub

        Public Overridable Sub Delete(obj As Object, type As Type)
            Dim transaction As ITransaction
            Try
                _connection.Connect()
                transaction = _connection.Transaction()
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
                _connection.Disconnect()
            End Try
        End Sub

        Public Overridable Sub Delete(obj As Object, type As Type, transaction As ITransaction)
            Dim tableNameAttr As TableNameAttribute = type.GetCustomAttribute(Of TableNameAttribute)
            Dim keyProp As PropertyInfo = type.GetProperties.FirstOrDefault(Function(x) x.GetCustomAttribute(Of KeyAttribute) IsNot Nothing)
            Dim delete As IDelete = _connection.Delete()
            delete.Table(If(tableNameAttr Is Nothing, type.Name, tableNameAttr.Name)).Filter(keyProp.Name, keyProp.GetValue(obj))
            With delete.GetCommand()
                .Connection = _connection
                .Transaction = transaction
                .Execute()
            End With
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
            Dim transaction As ITransaction
            Try
                _connection.Connect()
                transaction = _connection.Transaction()
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
                _connection.Disconnect()
            End Try
        End Sub

    End Class
End NameSpace