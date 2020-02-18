using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace FlexSignerService
{
    public partial class ServiceController : ServiceBase
    {
        private readonly Log _log = new Log();

        FlexSigner w;

        public ServiceController()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            _log.Debug("OnStart::Begin");
            w = new FlexSigner();
            w.Init();
            _log.Debug("OnStart::End");
        }

        protected override void OnStop()
        {
            _log.Debug("OnStop::Begin");
            w = null;
            _log.Debug("OnStop::End");
        }
    }
}
