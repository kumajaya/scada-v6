// Copyright (c) Rapid Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Scada.Comm.Config;
using Scada.Comm.Drivers.DrvModbus.View;

namespace Scada.Comm.Drivers.DrvSigModbus.View
{
    /// <summary>
    /// Implements the device user interface.
    /// <para>Реализует пользовательский интерфейс устройства.</para>
    /// </summary>
    public class DevSigModbusView : DevModbusView
    {
        private readonly CustomUi customUi;

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public DevSigModbusView(DriverView parentView, LineConfig lineConfig, DeviceConfig deviceConfig,
            CustomUi customUi) : base(parentView, lineConfig, deviceConfig, customUi)
        {
            this.customUi = customUi ?? throw new ArgumentNullException(nameof(customUi));
        }


        /// <summary>
        /// Shows a modal dialog box for editing device properties.
        /// </summary>
        public override bool ShowProperties()
        {
            if (new Forms.FrmDeviceProps(AppDirs, LineConfig, DeviceConfig, customUi).ShowDialog() == DialogResult.OK)
            {
                LineConfigModified = true;
                DeviceConfigModified = true;
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
