// Copyright (c) Rapid Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Scada.Comm.Drivers.DrvModbus.Config;

namespace Scada.Comm.Drivers.DrvSigModbus.Config
{
    /// <summary>
    /// Represents a device template.
    /// <para>Представляет шаблон устройства.</para>
    /// </summary>
    public class SigDeviceTemplate : DeviceTemplate
    {
        /// <summary>
        /// Gets the file name for a newly created device template.
        /// </summary>
        public override string NewTemplateFileName => "DrvSigModbus_NewTemplate.xml";

        /// <summary>
        /// Creates a new element group configuration.
        /// </summary>
        public override ElemGroupConfig CreateElemGroupConfig()
        {
            return new SigElemGroupConfig();
        }

        /// <summary>
        /// Creates a new command configuration.
        /// </summary>
        public override CmdConfig CreateCmdConfig()
        {
            return new SigCmdConfig();
        }
    }
}
