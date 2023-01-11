Imports System.Data
Imports System.Reflection
Imports System.Text
Imports Microsoft.Data.SqlClient

Public Class DataContext

    Public Property ConnectionString As String

    Public Sub New(connectionString As String)
        Me.ConnectionString = connectionString
    End Sub

    Public Function Query(sql As String, type As Type) As List(Of Object)
        Dim conn As New SqlConnection(ConnectionString)
        conn.Open()
        Try
            Dim results As New List(Of Object)
            Dim cmd As New SqlCommand(sql, conn)
            Dim adp As New SqlDataAdapter(cmd)
            Dim dt As New DataTable
            adp.Fill(dt)
            For Each dr As DataRow In dt.Rows
                Dim obj As Object = Activator.CreateInstance(type)
                For Each dc As DataColumn In dt.Columns
                    Dim prop As PropertyInfo = GetProp(type, dc.ColumnName)
                    prop?.SetValue(obj, dr(dc.ColumnName))
                Next
                results.Add(obj)
            Next
            Return results
        Finally
            conn.Close()
            conn.Dispose()
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

    Public Function Query(Of T)(sql As String) As List(Of T)
        Return Query(sql, GetType(T)).Cast(Of T).ToList
    End Function

    Public Sub Insert(obj As Object, type As Type)
        Dim conn As New SqlConnection(ConnectionString)
        conn.Open()
        Try
            Dim props As PropertyInfo() = type.GetProperties.Where(Function(x) x.GetCustomAttribute(Of IgnoreAttribute) Is Nothing AndAlso x.GetCustomAttribute(Of IdentityAttribute) Is Nothing).ToArray
            Dim sql As New StringBuilder($"insert into [{type.Name}] values(")
            Dim cmd As New SqlCommand() With {.Connection = conn}
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
        Finally
            conn.Close()
            conn.Dispose()
        End Try
    End Sub

    Public Sub Insert(Of T)(obj As T)
        Insert(obj, GetType(T))
    End Sub

    Public Sub Delete(obj As Object, type As Type)
        Dim conn As New SqlConnection(ConnectionString)
        conn.Open()
        Try
            Dim keyProp As PropertyInfo = type.GetProperties.FirstOrDefault(Function(x) x.GetCustomAttribute(Of KeyAttribute) IsNot Nothing)
            Dim cmd As New SqlCommand($"delete from [{type.Name}] where [{keyProp.Name}]=@{keyProp.Name}", conn)
            Dim param As SqlParameter = cmd.CreateParameter
            With param
                .ParameterName = $"@{keyProp.Name}"
                .Value = keyProp.GetValue(obj)
            End With
            cmd.Parameters.Add(param)
            cmd.ExecuteNonQuery()
        Finally
            conn.Close()
            conn.Dispose()
        End Try
    End Sub

    Public Sub Delete(Of T)(obj As T)
        Delete(obj, GetType(T))
    End Sub

    Public Sub Update(obj As Object, type As Type)
        Dim conn As New SqlConnection(ConnectionString)
        conn.Open()
        Try
            Dim keyProp As PropertyInfo = type.GetProperties.FirstOrDefault(Function(x) x.GetCustomAttribute(Of KeyAttribute) IsNot Nothing)
            Dim keyNameAttr As ColumnNameAttribute = keyProp.GetCustomAttribute(Of ColumnNameAttribute)
            Dim props As PropertyInfo() = type.GetProperties.Where(Function(x) x.GetCustomAttribute(Of IgnoreAttribute) Is Nothing AndAlso x.GetCustomAttribute(Of IdentityAttribute) Is Nothing).ToArray
            Dim sql As New StringBuilder($"update [{type.Name}] set ")
            Dim cmd As New SqlCommand() With {.Connection = conn}
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
        Finally
            conn.Close()
            conn.Dispose()
        End Try
    End Sub

    Public Sub Update(Of T)(obj As T)
        Update(obj, GetType(T))
    End Sub

End Class