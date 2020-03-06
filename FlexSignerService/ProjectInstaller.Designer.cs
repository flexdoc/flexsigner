namespace FlexSignerService
{
    partial class ProjectInstaller
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.FlexSignerService = new System.ServiceProcess.ServiceProcessInstaller();
            this.FlexSignerServiceInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // FlexSignerService
            // 
            this.FlexSignerService.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.FlexSignerService.Password = null;
            this.FlexSignerService.Username = null;
            // 
            // FlexSignerServiceInstaller
            // 
            this.FlexSignerServiceInstaller.Description = "FlexSignerService";
            this.FlexSignerServiceInstaller.DisplayName = "FlexSignerService";
            this.FlexSignerServiceInstaller.ServiceName = "FlexSignerService";
            this.FlexSignerServiceInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.FlexSignerService,
            this.FlexSignerServiceInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller FlexSignerService;
        private System.ServiceProcess.ServiceInstaller FlexSignerServiceInstaller;
    }
}