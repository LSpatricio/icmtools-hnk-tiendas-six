Imports System.Data.SqlClient
Imports System.Collections.Generic


Public Class ControlsData

#Region "Variables Locales"
#End Region

    Public Enum ControlType
        Special
        Status
        Assigned
        KPIS
    End Enum
    'Regresa las Sociedades disponibles para el usuario Logueado
    Public Function getSocieties(Model As String, User As String) As List(Of Societies)
        Return Nothing
    End Function

    'Regresa las Divisiones de Personal disponibles para la Sociedad seleccionada y el usuario Logueado
    Public Function getPersonnelDivisions(Model As String, User As String, Society As String) As List(Of PersonnelDivisions)
        Return Nothing
    End Function

    Public Function getPeriods(Model As String, Limit As Integer) As List(Of Periods)
        Return Nothing
    End Function
End Class

<Serializable()>
Public Class DictionaryItem
    Public Property Key As String
    Public Property Value As Object
    Public Sub New(_key As String, _Value As Object)
        Key = _key
        Value = _Value
    End Sub
End Class
