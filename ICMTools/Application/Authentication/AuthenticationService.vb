Public Class AuthenticationService

    Public Function ValidateToken(
    model As String,
    user As String,
    key As String
) As AuthenticationResult


        Dim userAccess As String = ""
        Dim modelAccess As String = ""



        Dim DecodeModel As String = DecodificarCredencial(model)
        Dim DecodeICMUser As String = DecodificarCredencial(user)
        Dim DecodeKey As String = DecodificarCredencial(key)

        userAccess = DecodeICMUser
        modelAccess = DecodeModel

        Dim cstTimeZoneInfo As TimeZoneInfo = TimeZoneInfo.Utc
        Dim HoraActual As DateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, cstTimeZoneInfo)

        Dim DateKey As DateTime = DecodeKey
        Dim TimeDiff As TimeSpan = HoraActual - DateKey
        Dim TMinutesDiff As Double = TimeDiff.TotalMinutes


        If TMinutesDiff < 0 AndAlso DecodeModel <> "DEBUG" Then
            Return New AuthenticationResult With {
                .Status = AuthenticationStatus.InvalidToken,
                .Model = modelAccess,
                .User = userAccess
            }
        End If

        If TMinutesDiff > 10 AndAlso DecodeModel <> "DEBUG" Then
            Return New AuthenticationResult With {
                .Status = AuthenticationStatus.ExpiredToken,
                .Model = modelAccess,
                .User = userAccess
            }
        End If

        Return New AuthenticationResult With {
            .Status = AuthenticationStatus.Valid,
            .Model = modelAccess,
            .User = userAccess
        }

    End Function


    Private Function DecodificarCredencial(Key As String) As String
        Try
            If (String.IsNullOrEmpty(Key)) Then
                Throw New ArgumentNullException("Las credenciales no pueden estar vacías, por favor ingresa nuevamente.")
            End If

            Dim DecodeKey As String = Encoding.UTF8.GetString(Convert.FromBase64String(Key))
            Return DecodeKey

        Catch ex As FormatException
            Throw New FormatException("Las credenciales no están en un formato válido, por favor ingresa nuevamente.", ex)
        Catch ex As ArgumentNullException
            Throw New ArgumentNullException("Las credenciales no pueden estar vacías, por favor ingresa nuevamente.", ex)
        Catch ex As Exception
            Throw New ApplicationException("Las credenciales de acceso sin inválidas, por favor ingresa nuevamente.", ex)
        End Try
    End Function

    Public Enum AuthenticationStatus
        Valid
        InvalidToken
        ExpiredToken
    End Enum
End Class
