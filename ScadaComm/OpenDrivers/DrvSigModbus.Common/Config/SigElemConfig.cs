// Copyright (c) Rapid Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Scada.Comm.Drivers.DrvModbus.Config;

namespace Scada.Comm.Drivers.DrvSigModbus.Config
{
    /// <summary>
    /// Represents an element configuration.
    /// <para>Представляет конфигурацию элемента.</para>
    /// </summary>
    public class SigElemConfig : ElemConfig
    {
        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public SigElemConfig()
            : base()
        {
            Scaling = "";
        }


        /// <summary>
        /// Gets or sets the scaling double array.
        /// </summary>
        public virtual string Scaling { get; set; }
    }
}
