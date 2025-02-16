// Copyright (c) Rapid Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Scada.Comm.Drivers.DrvModbus.View;
using Scada.Comm.Drivers.DrvSigModbus.Config;
using Scada.Comm.Drivers.DrvModbus.Config;

namespace Scada.Comm.Drivers.DrvSigModbus.View
{
    /// <summary>
    /// Provides flexibility to the driver user interface.
    /// <para>Обеспечивает гибкость пользовательского интерфейса драйвера.</para>
    /// </summary>
    public class SigModbusUi : CustomUi
    {
        /// <summary>
        /// Creates a new device template.
        /// </summary>
        public override DeviceTemplate CreateDeviceTemplate() => new SigDeviceTemplate();
    }
}
