using System;
using System.Xml;
using System.IO;

namespace FlexSignerService
{
    public class FlexSigner
    {
        SignX509 signx509;

        private string cnpjCertificate = "";

        private string signInputPath = "";
        private string signOutputPath = "";
        private string signTempPath = "";
     
        private readonly Log _log = new Log();

        private System.Timers.Timer timer;
        public void Init()
        {
            _log.Debug("Init::Begin: " + System.DateTime.Now.ToString());

            string configFile = System.IO.Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory) + "\\CONFIG.XML";

            if (!File.Exists(configFile))
            {
                IniFile.IniWriteValue(configFile, "SIGN", "SignInputPath", @"C:\sign\input\");
                IniFile.IniWriteValue(configFile, "SIGN", "SignOutputPath", @"C:\sign\output\");
                IniFile.IniWriteValue(configFile, "SIGN", "SignTempPath", @"C:\sign\temp\");

                IniFile.IniWriteValue(configFile, "CERTIFICATE", "cnpj", "10583028000152");
            }

            _log.Debug("Init: [1]");

            signInputPath = IniFile.IniReadValue(configFile, "SIGN", "SignInputPath");
            signOutputPath = IniFile.IniReadValue(configFile, "SIGN", "SignOutputPath");
            signTempPath = IniFile.IniReadValue(configFile, "SIGN", "SignTempPath");

            _log.Debug("Init: [2]");

            cnpjCertificate = IniFile.IniReadValue(configFile, "CERTIFICATE", "cnpj");

            _log.Debug("Init: [3]");

            try
            {
                System.IO.Directory.CreateDirectory(signInputPath);
                System.IO.Directory.CreateDirectory(signOutputPath);
                System.IO.Directory.CreateDirectory(signTempPath);
            }
            catch (Exception e)
            {
                _log.Debug("Init: Error CreateDir:" + e.Message);
            }
        
            _log.Debug("SignInputPath:" + signInputPath);
            _log.Debug("SignOutputPath:" + signOutputPath);
            _log.Debug("SignTempPath:" + signTempPath);

            _log.Debug("Checking certificate");
            signx509 = new SignX509(cnpjCertificate);

            if (signx509.NumberOfCertificatesFound == 0)
            {
                _log.Debug("No certificates found: " + cnpjCertificate);
                return;
            }

            _log.Debug("Certificate Ok : [" + cnpjCertificate + "]");

            this.timer = new System.Timers.Timer(30000D);  // 30000 milliseconds = 30 seconds
            this.timer.AutoReset = true;
            this.timer.Elapsed += new System.Timers.ElapsedEventHandler(this.timer1_Tick);
            this.timer.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer.Stop();
            _log.Debug("Timer: " + System.DateTime.Now.ToString());
            ProcessSign();
            timer.Start();
        }

        private void ProcessSign()
        {
            try
            {
                string[] files = System.IO.Directory.GetFiles(signInputPath);

                _log.Debug("ProcessSign: Total Files [IN]: " + files.Length.ToString());

                foreach (string file in files)
                {
                    DateTime fileTime = File.GetLastWriteTime(file);
                    DateTime nowDate = DateTime.Now;

                    _log.Debug("ProcessSign:Processing: " + file);

                    //use timespan to get the number of seconds
                    TimeSpan span = nowDate - fileTime;
                    var seconds = (int)span.TotalSeconds;

                    if (seconds >= 60)    //Delay
                    {
                        string fileout = signOutputPath + "\\" + System.IO.Path.GetFileName(file);
                        string fileTmp = signTempPath + "\\" + System.IO.Path.GetFileName(file);

                        try
                        {
                            if (System.IO.File.Exists(fileout))
                                System.IO.File.Delete(fileout);

                            if (!signx509.CheckIfExistsSignature(file))
                            {
                                if (signx509.SignPDF(file, fileTmp))
                                {
                                    System.IO.File.Delete(file);
                                    System.IO.File.Move(fileTmp, fileout);
                                    _log.Debug("Processed: " + file);
                                }
                            }
                            else
                            {
                                //Check if it is already signed
                                _log.Debug("File already has certificate: " + file);
                                System.IO.File.Move(file, fileout);
                            }

                        }
                        catch (Exception e)
                        {
                            _log.Debug("**ERROR: " + e.Message);
                            _log.Debug("ERROR: " + file);
                            System.IO.File.Delete(file);

                        }
                    }

                }
            }
            catch (Exception e)
            {
                _log.Error("ProcessSIGN::Error:" + e.Message);
            }

            _log.Debug("ProcessSign: End");
        }
                               
        private static string ReadConfigTag(string tag)
        {
            string fileName = AppDomain.CurrentDomain.BaseDirectory + "\\CONFIG\\CONFIG.XML";
            string ret = "";

            if (File.Exists(fileName))
            {
                var xtr = new XmlTextReader(fileName);
                xtr.WhitespaceHandling = WhitespaceHandling.None;
                var xml = new XmlDocument();
                xml.Load(xtr);
                xtr.Close();

                XmlNode element = xml.SelectSingleNode("/ROOT/" + tag);
                if (element != null)
                    ret = element.InnerText;
            }            
            return ret;
        }
    }
}
