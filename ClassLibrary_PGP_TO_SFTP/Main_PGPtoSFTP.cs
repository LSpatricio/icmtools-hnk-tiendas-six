using CsvHelper;
using CsvHelper.Configuration;
using Org.BouncyCastle.Bcpg;
using Org.BouncyCastle.Bcpg.OpenPgp;
using Org.BouncyCastle.Security;
using Renci.SshNet;
using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;

namespace ClassLibrary_PGP_TO_SFTP
{
    public class Main_PGPtoSFTP
    {

        public static bool Proceso(string name, DataTable dt, string model)
        {
            // Validar que el modelo no sea nulo o vacío
            if (string.IsNullOrEmpty(model))
            {
                throw new ArgumentException("El parámetro 'model' no puede ser nulo o vacío.", nameof(model));
            }

            // Construir el nombre de la configuración SFTP basado en el modelo
            // Ejemplo: si model = "femcovs", entonces sftpConfigName = "SFTPConfig_femcovs"
            string sftpConfigName = $"SFTPConfig_{model}";

            //Definición de Modelo
            string varKey = model.EndsWith("prd") ? "Key_PGP" : "Key_PGP_alter";

            // Leer configuración          
            KeyPGPConfig _KeyPGPConfig = ConfigHelper.GetSection<KeyPGPConfig>(varKey);
            SFTPConfig _SFTPConfig = ConfigHelper.GetSection<SFTPConfig>(sftpConfigName);

            // Aquí ya puedes acceder a las llaves PGP
            string publicKey64 = _KeyPGPConfig.PGP_PUBLIC_KEY_Base64;
            //string privateKey = _pgpConfig.PGP_PRIVATE_KEY_Base64;

            string pgpKey = Encoding.UTF8.GetString(Convert.FromBase64String(publicKey64));
            MemoryStream lectorStream = new MemoryStream(Encoding.UTF8.GetBytes(pgpKey));
            //PgpPublicKey publicKey = clsFuncionalidad_Encriptacion.ReadPublicKey(lectorStream

            string projectRoot = GetProjectRoot();
            string folderPath = Path.Combine(projectRoot, "File");
            Directory.CreateDirectory(folderPath);

            //Definicion del path de csv encripado
            string fileName = string.Concat(name, ".csv");
            string guid = Guid.NewGuid().ToString();
            string tempName = string.Concat(name,"_",guid,".csv");

            //string csvFilePath = Path.Combine(folderPath, $@"{name}.csv");
            //string encryptedFilePath = Path.Combine(folderPath, $@"{name}.csv.pgp");

            string csvTempFilePath = Path.Combine(folderPath, tempName);
            CreateCsvFromDataTable(dt, csvTempFilePath);

            string csvFilePath = Path.Combine(folderPath, tempName);
            string encryptedTempPath = Path.Combine(folderPath, $@"{name}_{guid}.csv.pgp");
            string encryptedFilePath = Path.Combine(folderPath, $@"{name}.csv.pgp");
            EncryptFileWithPgp(csvFilePath, encryptedTempPath, lectorStream);

            //Envio de pgp al SFTP
            bool enviado = SendFileSSH( encryptedTempPath, encryptedFilePath,  _SFTPConfig,  model);

            //Limpieza de csv en el servidor
            if (enviado) {
                bool deleted = DeleteSended(csvFilePath, encryptedFilePath);
            }

            return enviado;
        }

        public static bool SendFile(string localFilePath, string remoteFileName, SFTPConfig _sftpConfig)
        {
            Boolean Value = true;
            try
            {
                string host = _sftpConfig.Host;
                int port = _sftpConfig.Port;
                string username = _sftpConfig.Username;
                string password = _sftpConfig.Password;
                string remoteDirectory = _sftpConfig.RemotePath;
                using (var sftp = new SftpClient(host, port, username, password))
                {
                    sftp.Connect();
                    if (!sftp.IsConnected)
                    {
                        //_logger.LogError("No se pudo conectar al servidor SFTP.");
                        return false;
                    }
                    using (var fileStream = new FileStream(localFilePath, FileMode.Open))
                    {
                        string remoteFilePath = $"{remoteDirectory}/{remoteFileName}";
                        sftp.UploadFile(fileStream, remoteFilePath);
                    }
                    sftp.Disconnect();
                }
                //_logger.LogInformation($"Archivo '{localFilePath}' enviado correctamente a '{remoteDirectory}/{remoteFileName}'");
            }
            catch (Exception ex)
            {
                Value = false;
                //_logger.LogError(ex, "Error al enviar el archivo vía SFTP.");
            }
            return Value;
        }

        private static string GetNameKeySSH(string model)
        {
            string keyName = string.Empty;
            try
            {
                if (model.EndsWith("qa"))
                {
                    keyName = "Key_SSH_qa";
                }
                else if (model.EndsWith("dev"))
                {
                    keyName = "Key_SSH_dev";
                }
                else if (model.EndsWith("prd"))
                {
                    keyName = "Key_SSH_prd";
                }
                return keyName;
            }
            catch
            {
                throw;
            }
        }

        public static bool SendFileSSH_ORIG(string localFilePath, string remoteFileName, SFTPConfig _sftpConfig, string baseName, string model)
        {
            bool Value = true;
            try
            {
                string host = _sftpConfig.Host;
                int port = _sftpConfig.Port;
                string username = _sftpConfig.Username;
                string remoteDirectory = _sftpConfig.RemotePath;

                // Obtener la clave SSH codificada en Base64 desde configuración
                string nameKeySSH = GetNameKeySSH(model);
                KeySSHConfig _KeySSHConfig = ConfigHelper.GetSection<KeySSHConfig>(nameKeySSH);
                string SSHKey64 = _KeySSHConfig.SSH_PEM_Base64;

                // Convertir Base64 a bytes y crear un stream en memoria
                byte[] keyBytes = Convert.FromBase64String(SSHKey64);
                using (var keyStream = new MemoryStream(keyBytes))
                {
                    // Cargar la clave privada desde memoria (sin passphrase)
                    var keyFile = new PrivateKeyFile(keyStream);

                    // Configurar autenticación SSH por clave privada
                    var authMethod = new PrivateKeyAuthenticationMethod(username, keyFile);
                    var connectionInfo = new ConnectionInfo(host, port, username, authMethod);

                    using (var sftp = new SftpClient(connectionInfo))
                    {
                        sftp.Connect();

                        if (!sftp.IsConnected)
                        {
                            Value = false;
                            return Value;
                        }

                        //Eliminacion del csv desencriptado anterior
                        string remotePath = $"{remoteDirectory}/{baseName}.csv";
                        if (sftp.Exists(remotePath)) 
                        {
                            sftp.DeleteFile(remotePath);    
                        }

                        // ✅ Subir el archivo local al servidor remoto
                        using (var fileStream = new FileStream(localFilePath, FileMode.Open))
                        {
                            string remoteFilePath = $"{remoteDirectory}/{remoteFileName}";
                            sftp.UploadFile(fileStream, remoteFilePath);
                        }

                        sftp.Disconnect();
                    }
                }
            }
            catch (Exception ex)
            {
                Value = false;
                // Aquí podrías loguear el error, por ejemplo:
                // _logger.LogError(ex, "Error al enviar el archivo vía SFTP.");
            }

            return Value;
        }

public static bool SendFileSSH(string localTempPath, string localFilePath,  SFTPConfig _sftpConfig, string model)
    {
        bool Value = true;
            string remoteTempName = Path.GetFileName(localTempPath);
            string remoteFileName = Path.GetFileName(localFilePath);
            try
        {
            string host = _sftpConfig.Host;
            int port = _sftpConfig.Port;
            string username = _sftpConfig.Username;
            string remoteDirectory = _sftpConfig.RemotePath;

            string nameKeySSH = GetNameKeySSH(model);
            KeySSHConfig _KeySSHConfig = ConfigHelper.GetSection<KeySSHConfig>(nameKeySSH);
            string SSHKey64 = _KeySSHConfig.SSH_PEM_Base64;

            byte[] keyBytes = Convert.FromBase64String(SSHKey64);

            using (var keyStream = new MemoryStream(keyBytes))
            {
                var keyFile = new PrivateKeyFile(keyStream);
                var authMethod = new PrivateKeyAuthenticationMethod(username, keyFile);
                var connectionInfo = new ConnectionInfo(host, port, username, authMethod);

                using (var sftp = new SftpClient(connectionInfo))
                {
                    sftp.Connect();

                    if (!sftp.IsConnected)
                        return false;

                    // 🔹 Rutas remotas
                    string remoteTempPath = $"{remoteDirectory}/{remoteTempName}.tmp";
                    string remoteFinalPath = $"{remoteDirectory}/{remoteFileName}"; // ej: archivo.csv.pgp

                    // 🔹 Limpieza previa (por si quedó basura)
                    if (sftp.Exists(remoteTempPath))
                        sftp.DeleteFile(remoteTempPath);

                    // 🔹 Subir archivo como .tmp
                    using (var fileStream = new FileStream(localTempPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        sftp.UploadFile(fileStream, remoteTempPath);
                    }

                    // 🔹 Rename atómico (safe publish)
                    if (sftp.Exists(remoteFinalPath))
                    {
                        // Si tu versión soporta overwrite directo, usa esto:
                        // sftp.RenameFile(remoteTempPath, remoteFinalPath, true);

                        // Si no, haz delete + rename
                        sftp.DeleteFile(remoteFinalPath);
                    }

                    sftp.RenameFile(remoteTempPath, remoteFinalPath);

                    sftp.Disconnect();
                }
            }
        }
        catch (Exception ex)
        {
            Value = false;
            // log aquí si quieres
        }

        return Value;
    }

    public static void CreateCsvFromDataTable(DataTable dataTable, string filePath)
        {
            using (var writer = new StreamWriter(filePath))
            using (var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = "," }))
            {
                // Escribir encabezados
                foreach (DataColumn column in dataTable.Columns)
                {
                    csv.WriteField(column.ColumnName);
                }
                csv.NextRecord();

                // Escribir filas dinámicamente
                foreach (DataRow row in dataTable.Rows)
                {
                    foreach (DataColumn column in dataTable.Columns)
                    {
                        csv.WriteField(row[column]); // Escribe el valor de cada celda
                    }
                    csv.NextRecord();
                }
            }
        }


        public static string GetProjectRoot()
        {
            // Obtener ensamblado que inició la aplicación
            var asm = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();

            var projectName = asm.GetName().Name;

            var dir = new DirectoryInfo(AppContext.BaseDirectory);

            // Subir carpetas hasta encontrar .csproj o .vbproj
            while (dir != null &&
                   !File.Exists(Path.Combine(dir.FullName, $"{projectName}.csproj")) &&
                   !File.Exists(Path.Combine(dir.FullName, $"{projectName}.vbproj")))
            {
                dir = dir.Parent;
            }

            // Si no encuentra, devuelve carpeta raíz del proyecto (o bin)
            return dir?.FullName ?? AppContext.BaseDirectory;
        }

        public static void EncryptFileWithPgp(string inputFilePath, string outputFilePath, Stream publicKeyStream)
        {
            using (Stream inputFileStream = File.OpenRead(inputFilePath))
            using (Stream outputFileStream = File.Create(outputFilePath))
            {
                PgpPublicKey publicKey = ReadPublicKey(publicKeyStream);
                EncryptFile(inputFileStream, outputFileStream, publicKey);
            }
        }

        private static PgpPublicKey ReadPublicKey(Stream inputStream)
        {
            PgpPublicKeyRingBundle pgpPub = new PgpPublicKeyRingBundle(PgpUtilities.GetDecoderStream(inputStream));
            foreach (PgpPublicKeyRing keyRing in pgpPub.GetKeyRings())
            {
                foreach (PgpPublicKey key in keyRing.GetPublicKeys())
                {
                    if (key.IsEncryptionKey) return key;
                }
            }
            throw new ArgumentException("No encryption key found in public key ring.");
        }

        private static void EncryptFile(Stream inputStream, Stream outputStream, PgpPublicKey publicKey)
        {
            using (MemoryStream bOut = new MemoryStream())
            {
                PgpCompressedDataGenerator comData = new PgpCompressedDataGenerator(CompressionAlgorithmTag.Zip);
                using (Stream cos = comData.Open(bOut))
                {
                    PgpLiteralDataGenerator lData = new PgpLiteralDataGenerator();
                    using (Stream pOut = lData.Open(cos, PgpLiteralData.Binary, "data.csv", DateTime.UtcNow, new byte[4096]))
                    {
                        inputStream.CopyTo(pOut);
                    }
                }
                comData.Close();

                PgpEncryptedDataGenerator encGen = new PgpEncryptedDataGenerator(SymmetricKeyAlgorithmTag.Aes256, true, new SecureRandom());
                encGen.AddMethod(publicKey);
                using (Stream encOut = encGen.Open(outputStream, bOut.Length))
                {
                    bOut.Position = 0;
                    bOut.CopyTo(encOut);
                }
            }
        }

        private static bool DeleteSended(string csvPath, string pgpPath) {
             try
                 {
                     // Verifica si el archivo CSV existe y, si es así, lo elimina.
                     if (File.Exists(csvPath))
                     {
                         File.Delete(csvPath);
                     }
   
                    // Verifica si el archivo PGP existe y, si es así, lo elimina.
                    if (File.Exists(pgpPath))
                    {
                        File.Delete(pgpPath);
                    }
  
                    return true;
                }
                catch (Exception ex)
                {
                    //Console.WriteLine($"Error al eliminar archivos: {ex.Message}");
                    return false;
                }
        }

    }
}
