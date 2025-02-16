using Scada.Comm.Config;
using Scada.Comm.Drivers.DrvModbus.View;

namespace Scada.Comm.Drivers.DrvSigModbus.View.Forms
{
    /// <summary>
    /// Represents a form for configuring device and communication line properties.
    /// <para>Представляет форму для настройки свойств устройства и линии связи.</para>
    /// </summary>
    public partial class FrmDeviceProps : DrvModbus.View.Forms.FrmDeviceProps
    {
        private readonly AppDirs appDirs;           // the application directories
        private readonly CustomUi customUi;         // the UI customization object

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public FrmDeviceProps(AppDirs appDirs, LineConfig lineConfig, DeviceConfig deviceConfig, CustomUi customUi)
            : base(appDirs, lineConfig, deviceConfig, customUi)
        {
            this.appDirs = appDirs ?? throw new ArgumentNullException(nameof(appDirs));
            this.customUi = customUi ?? throw new ArgumentNullException(nameof(customUi));
        }

        /// <summary>
        /// Shows a form for editing the device template.
        /// </summary>
        public override void EditDeviceTemplate(string fileName = "")
        {
            FrmDeviceTemplate frmDeviceTemplate = new(appDirs, customUi)
            {
                FileName = fileName
            };

            frmDeviceTemplate.ShowDialog();
            fileName = frmDeviceTemplate.FileName;

            if (string.IsNullOrEmpty(fileName))
                TemplateFileName = "";
            else if (ValidateTemplatePath(fileName, out string shortFileName))
                TemplateFileName = shortFileName;
        }
    }
}
