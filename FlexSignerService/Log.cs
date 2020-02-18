// ============================================
// 
// FlexImage.Core
// Log.cs
// 26/08/2012
// cflavio
// 
// ============================================

#region

using System;
using System.IO;
using log4net;
using log4net.Config;

#endregion

namespace FlexSignerService
{
    public class Log
    {
        private readonly ILog log;
        
        public Log()
        {
            try
            {
                try
                {
                    string logPath = Directory.GetCurrentDirectory() + "\\LOG";
                    Directory.CreateDirectory(logPath);
                }
                catch
                {
                }

                string aux = Directory.GetCurrentDirectory() + "\\Log4net.config";

                if (!System.IO.File.Exists(aux))
                {
                    CreateLog4NetConfig(aux);
                }


                var fi = new FileInfo(aux);
                XmlConfigurator.ConfigureAndWatch(fi);
                this.log = LogManager.GetLogger("FlexImage");
            }
            catch (Exception e)
            {
                //Alert.Information("Log::" + e);
            }
        }

        private void CreateLog4NetConfig(string filename)
        {

            string s = "";
            s = s + "<log4net>";
            s = s + "    <appender name=\"RollingFileAppender\" type=\"log4net.Appender.RollingFileAppender\">";
            s = s + "    <file value=\"log/log.txt\" />";
            s = s + "    <appendToFile value=\"true\" />";
            s = s + "    <rollingStyle value=\"Size\" />";
            s = s + "     <maxSizeRollBackups value=\"20\" />";
            s = s + "    <maximumFileSize value=\"500KB\" />";
            s = s + "    <staticLogFileName value=\"true\" />";
            s = s + "    <layout type=\"log4net.Layout.PatternLayout\">";
            s = s + "      <conversionPattern value=\"%date [%thread] %-5level %logger [%property{NDC}] - %message%newline\" />";
            s = s + "    </layout>";
            s = s + "  </appender>";

            s = s + "  <appender name=\"ColoredConsoleAppender\" type=\"log4net.Appender.ColoredConsoleAppender\">";
            s = s + "    <mapping>";
            s = s + "      <level value=\"ERROR\" />";
            s = s + "      <foreColor value=\"White\" />";
            s = s + "      <backColor value=\"Red, HighIntensity\" />";
            s = s + "    </mapping>";
            s = s + "    <mapping>";
            s = s + "      <level value=\"DEBUG\" />";
            s = s + "      <backColor value=\"Green\" />";
            s = s + "    </mapping>";
            s = s + "    <layout type=\"log4net.Layout.PatternLayout\">";
            s = s + "      <conversionPattern value=\"%date [%thread] %-5level %logger [%property{NDC}] - %message%newline\" />";
            s = s + "    </layout>";
            s = s + "  </appender>";

            s = s + "  <appender name=\"tnet\" type=\"log4net.Appender.TelnetAppender\">";
            s = s + "      <port value=\"23\" />";
            s = s + "      <layout type=\"log4net.Layout.PatternLayout\">";
            s = s + "          <conversionPattern value=\"%-5p [%t]: %m%n\" />";
            s = s + "      </layout>";
            s = s + "  </appender>";

            s = s + "  <logger name=\"FlexImage\">";
            s = s + "    <level value=\"All\" />";
            s = s + "    <appender-ref ref=\"ColoredConsoleAppender\" />";
            s = s + "    <appender-ref ref=\"RollingFileAppender\" />";
            s = s + "    <appender-ref ref=\"tnet\" />";
            s = s + "  </logger>";
            s = s + "</";
            s = s + "log4net >";

            // create a writer and open the file
            TextWriter tw = new StreamWriter(filename);

            // write a line of text to the file
            tw.WriteLine(s);

            // close the stream
            tw.Close();

        }

        public void Info(string aux)
        {
            try
            {
                this.log.Info(aux);
                Console.WriteLine(aux);
            }
            catch (Exception e)
            {
                //Alert.Information("Log.Info::" + e);
            }
        }

        public void Debug(string aux)
        {
            try
            {
                this.log.Debug(aux);
                //Console.WriteLine(aux);
            }
            catch (Exception e)
            {
                //Alert.Information("Log.Debug::" + e);
            }
        }

        public void Error(string aux)
        {
            try
            {
                this.log.Error(aux);
                Console.WriteLine("ERROR: " + aux);
            }
            catch (Exception e)
            {
                //Alert.Information("Log.Error::" + e);
            }
        }

        public void Fatal(string aux)
        {
            try
            {
                this.log.Fatal(aux);
                Console.WriteLine(aux);
            }
            catch (Exception e)
            {
                //Alert.Information("Log.Fatal::" + e);
            }
        }
    }
}