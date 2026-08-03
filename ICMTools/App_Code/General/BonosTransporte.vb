Imports System.Data
Imports System.Data.SqlClient


Public Class BonoTransporteDetail
    Public Property IDBonoDetail As String
    Public Property IDBono As String
    Public Property Payee As String
    Public Property DateBono As String
    Public Property CCNom As String
    Public Property Amount As String
    Public Property Reason As String
    Public Property MessageResponse As String
    Public Property StatusCode As String
    Public Property StatusDescription As String

    Public Shared Function ValidateBonosTransporte(idBono As Int32) As Boolean
        Dim CnnFEMCO_Transfer As String = ""
        Dim res As Int32 = 0
        Dim resultList As New List(Of BonoTransporteDetail)
        Try
            Using dbFactory As DataBase = New DataBase(CnnFEMCO_Transfer)
                res = dbFactory.ExecuteStoredProcedure("[FEMCOEPSAP].[spICMToolsBonosTransporteValidate]", DataBase.EnumExecutionType.NonQuery, New SqlParameter("@IdBono", idBono))
            End Using
            Return True
        Catch ex As Exception
            Return False
        End Try
        Return False
    End Function
    Public Shared Function GetBonosDetailByID(idBono As Int32, onlyActive As Int32) As List(Of BonoTransporteDetail)
        Dim CnnFEMCO_Transfer As String = ""
        Dim dt As DataTable = Nothing
        Dim resultList As New List(Of BonoTransporteDetail)

        Using dbFactory As DataBase = New DataBase(CnnFEMCO_Transfer)
            dt = dbFactory.GetDataAsDataTable("[FEMCOEPSAP].[spICMToolsBonosTransporteDetail]", New SqlParameter("@IdBono", idBono), New SqlParameter("@OnlyActive", onlyActive))
        End Using

        If dt IsNot Nothing Then
            For Each row As DataRow In dt.Rows
                Dim bono = New BonoTransporteDetail()
                bono.IDBonoDetail = row.Item(0).ToString()
                bono.IDBono = row.Item(1).ToString()
                bono.Payee = row.Item(2).ToString()
                bono.DateBono = row.Item(3).ToString()
                bono.CCNom = row.Item(4).ToString()
                bono.Amount = row.Item(5).ToString()
                bono.Reason = row.Item(6).ToString()
                bono.MessageResponse = row.Item(7).ToString()
                bono.StatusCode = row.Item(8).ToString()
                bono.StatusDescription = row.Item(9).ToString()
                resultList.Add(bono)
            Next
            Return resultList
        Else
            Return New List(Of BonoTransporteDetail)
        End If
    End Function

End Class

Public Class BonoTransporteLote
#Region "Propiedades"
    Public Property IDBono As String
    Public Property CreationEmployee As String
    Public Property CreationDate As String
    Public Property CreationComment As String
    Public Property AuthorizedEmployee As String
    Public Property AuthorizedDate As String
    Public Property AuthorizedComment As String
    Public Property lastUpdate As String
    Public Property userUpdate As String
    Public Property StatusCode As String
    Public Property StatusDescription As String
    Public Property DivisionID As String
    Public Property DivsionName As String
    Public Property Periodo As String
    Public Property PeriodoId As String
    Public Property Society As String
    Public Property SocietyId As String
    Public Property AuthorizedEmail As String
#End Region

    Public Sub New(Optional ByVal idBono As Integer = 0)
        Me.IDBono = idBono
    End Sub

    Public Shared Function GetBonosByUser(user As String, type As String) As List(Of BonoTransporteLote)
        Dim CnnFEMCO_Transfer As String = ""
        Dim dt As DataTable = Nothing
        Dim resultList As New List(Of BonoTransporteLote)

        Using dbFactory As DataBase = New DataBase(CnnFEMCO_Transfer)
            dt = dbFactory.GetDataAsDataTable("[FEMCOEPSAP].[spICMToolsBonosTransporteLotGet]", New SqlParameter("@User", user), New SqlParameter("@Type", type))
        End Using

        If dt IsNot Nothing Then

            For Each row As DataRow In dt.Rows
                Dim bono = New BonoTransporteLote()
                bono.IDBono = row.Item(0).ToString()
                bono.CreationEmployee = row.Item(1).ToString()
                bono.CreationDate = row.Item(2).ToString()
                bono.AuthorizedEmployee = row.Item(3).ToString()
                bono.AuthorizedDate = row.Item(4).ToString()
                bono.lastUpdate = row.Item(5).ToString()
                bono.userUpdate = row.Item(6).ToString()
                bono.StatusCode = row.Item(7).ToString()
                bono.StatusDescription = row.Item(8).ToString()
                bono.DivisionID = row.Item(9).ToString()
                bono.DivsionName = row.Item(10).ToString()
                bono.SocietyId = row.Item(11).ToString()
                bono.Society = row.Item(12).ToString()
                bono.PeriodoId = row.Item(13).ToString()
                bono.Periodo = row.Item(14).ToString()
                bono.AuthorizedComment = row.Item(15).ToString()
                bono.AuthorizedEmail = row.Item(16).ToString()
                resultList.Add(bono)
            Next
            Return resultList
        Else
            Return New List(Of BonoTransporteLote)

        End If
    End Function

    Public Shared Function UpsertBonos(user As String, status As String, Optional ByVal BonoObj As BonoTransporteLote = Nothing) As BonoTransporteLote
        Dim CnnFEMCO_Transfer As String = ""
        Dim dt As DataTable = Nothing
        Dim resultList As New List(Of BonoTransporteLote)

        Using dbFactory As DataBase = New DataBase(CnnFEMCO_Transfer)
            Dim pUser = New SqlParameter("@User", user)
            Dim pstatus = New SqlParameter("@Status", status)
            Dim comment As String = If(Not String.IsNullOrEmpty(BonoObj.CreationComment), BonoObj.CreationComment, BonoObj.AuthorizedComment)
            Dim pComment = New SqlParameter("@Comment", comment)

            If BonoObj.IDBono <> 0 And status <> "P" Then
                Dim pIDBono = New SqlParameter("@IdBono", BonoObj.IDBono)
                dt = dbFactory.GetDataAsDataTable("[FEMCOEPSAP].[spICMToolsBonosTransporteLot]", pUser, pIDBono, pstatus, pComment)
            Else
                Dim pIdSociety = New SqlParameter("@Society", BonoObj.SocietyId)
                Dim pIdDivision = New SqlParameter("@Division", BonoObj.DivisionID)
                Dim pIdPeriod = New SqlParameter("@Period", BonoObj.PeriodoId)

                Select Case status
                    Case "P"
                        Dim pIDBono = New SqlParameter("@IdBono", BonoObj.IDBono)
                        dt = dbFactory.GetDataAsDataTable("[FEMCOEPSAP].[spICMToolsBonosTransporteLot]", pUser, pIDBono, pstatus, pIdSociety, pIdDivision, pIdPeriod, pComment)
                    Case Else
                        dt = dbFactory.GetDataAsDataTable("[FEMCOEPSAP].[spICMToolsBonosTransporteLot]", pUser, pstatus, pIdSociety, pIdDivision, pIdPeriod, pComment)
                End Select
            End If
        End Using

        If dt IsNot Nothing Then

            For Each row As DataRow In dt.Rows
                Dim bono = New BonoTransporteLote()
                bono.IDBono = row.Item(0).ToString()
                bono.CreationEmployee = row.Item(1).ToString()
                bono.CreationDate = row.Item(2).ToString()
                bono.AuthorizedEmployee = row.Item(3).ToString()
                bono.AuthorizedDate = row.Item(4).ToString()
                bono.lastUpdate = row.Item(5).ToString()
                bono.userUpdate = row.Item(6).ToString()
                bono.StatusCode = row.Item(7).ToString()
                bono.StatusDescription = row.Item(8).ToString()
                resultList.Add(bono)
            Next
            Return resultList.First

        Else
            Return New BonoTransporteLote

        End If

    End Function

    Public Shared Function setBulkBonosTransporte(dt As DataTable) As String
        Dim mensaje = String.Empty
        Dim CnnFEMCO_Transfer As String = ""
        Using connection As New SqlConnection(CnnFEMCO_Transfer)
            connection.Open()
            Using bulkCopy As New SqlBulkCopy(connection)
                bulkCopy.DestinationTableName = "[FEMCOEPSAP].[ICMToolsBonosTranUploadDetail]"
                bulkCopy.ColumnMappings.Add("IDBono", "IDBono")
                bulkCopy.ColumnMappings.Add("Payee", "Payee")
                bulkCopy.ColumnMappings.Add("Date", "Date")
                bulkCopy.ColumnMappings.Add("CCNom", "CCNom")
                bulkCopy.ColumnMappings.Add("Amount", "Amount")
                bulkCopy.ColumnMappings.Add("Reason", "Reason")
                bulkCopy.ColumnMappings.Add("Status", "Status")
                bulkCopy.ColumnMappings.Add("MessageResponse", "MessageResponse")
                Try
                    bulkCopy.WriteToServer(dt)
                Catch ex As Exception
                    mensaje = "Error al insertar datos masivamente:" + ex.Message
                    Throw New System.Exception(mensaje)
                End Try

            End Using
            connection.Close()
        End Using
        Return mensaje
    End Function
End Class