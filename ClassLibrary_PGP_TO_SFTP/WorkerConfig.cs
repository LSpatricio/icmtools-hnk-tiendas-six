using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary_PGP_TO_SFTP
{
   
    public class WorkerConfig
    {
        public int IntervalMinutes { get; set; }
    }

    public class KeyPGPConfig
    {
        public string PGP_PUBLIC_KEY_Base64 { get; set; } = string.Empty;
        public string PGP_PRIVATE_KEY_Base64 { get; set; } = string.Empty;
    }
    public class KeySSHConfig
    {
        public string SSH_PEM_Base64 { get; set; } = string.Empty;
    }
    public class SFTPConfig
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string RemotePath { get; set; }
    }


    public enum EnumSFTP
    {
        SFTPConfig_femco,
        SFTPConfig_femcoep,
        SFTPConfig_femcovs,
        SFTPConfig_femcocomp
    }


}
