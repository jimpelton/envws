using System;
using System.IO;

namespace EnvwsLib.Util
{
    using Renci.SshNet;

    public class SshAsyncTransfer
    {

        private log4net.ILog 
            logger = log4net.LogManager.GetLogger(typeof(SshAsyncTransfer));

       
        private string username, password;
        private ulong size=1L;


        public Uri RemoteUri 
        { 
            get { return m_remoteUri; } 
            set { m_remoteUri = value; } 
        }
        private Uri m_remoteUri;


        public Uri LocalUri 
        { 
            get { return m_localUri; } 
            set { m_localUri = value; } 
        }
        private Uri m_localUri;

        /// <summary>
        /// Callback for bytes transfered while transfer is active.
        /// </summary>
        public PercentCompleteCallback BytesTransfered
        {
            get { return m_percentCompleteCallback; }
            set { m_percentCompleteCallback = value; }
        }
        private PercentCompleteCallback m_percentCompleteCallback;
        public delegate void PercentCompleteCallback(ulong trans);


        /// <summary>
        /// unused at the moment.
        /// </summary>
        public TransferCompleteCallback TransferCompleted
        {
            get { return m_transferCompleteCallback; }
            set { m_transferCompleteCallback = value; }
        }
        private TransferCompleteCallback m_transferCompleteCallback;
        public delegate void TransferCompleteCallback();


        /// <summary>
        /// Construct an SSHAsyncTransfer for either upload or download.
        /// </summary>
        public SshAsyncTransfer(string localUri, string remoteUri, string username="", string passwd="")
        {
            m_localUri = new Uri(localUri);
            m_remoteUri = new Uri(remoteUri);
            this.username = username;
            password = passwd;
        }


        /// <summary>
        /// Non-asynchronous file transfer from local-uri to remote-uri.
        /// </summary>
        public void UploadFile()
        {
            string host = m_remoteUri.Authority;
            string localFilePath = m_localUri.AbsolutePath;
            string remoteFileName = m_remoteUri.AbsolutePath;
            
            using (SftpClient sftp = new SftpClient(host, username, password))
            {
                sftp.Connect();
                
                using (Stream file = File.OpenRead(localFilePath))
                {
                    sftp.UploadFile(file, remoteFileName, false, makePercentCompleteCallback);
                }

                sftp.Disconnect();
            }
        }


        /// <summary>
        /// Non-asynchronous file transfer from remote-uri to local-uri.
        /// </summary>
        public void DownloadFile()
        {
            string host = m_remoteUri.Authority;
            string localFilePath = m_localUri.AbsolutePath;
            string remoteFileName = m_remoteUri.AbsolutePath;

            using (SftpClient sftp = new SftpClient(host, username, password))
            {
                //sftp.ListDirectory(Path.GetDirectoryName(m_remoteUri))
                    //.First<SftpFile>((f) => {f.FullName.Equals(remoteFileName)});

                sftp.Connect();
                
                using (FileStream file = File.OpenWrite(localFilePath))
                {
                    sftp.DownloadFile(remoteFileName, file, makePercentCompleteCallback);
                }

                sftp.Disconnect();
            }
        }


        private void makePercentCompleteCallback(ulong val)
        {
            BytesTransfered(val);
        }
    }
}
