// Copyright (c) Rapid Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Scada.Comm.Drivers.DrvModbus.Protocol;

namespace Scada.Comm.Drivers.DrvSigModbus.Protocol
{
    /// <summary>
    /// Represents a Modbus element (register).
    /// <para>Представляет элемент (регистр) Modbus.</para>
    /// </summary>
    public class SigElem : Elem
    {
        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public SigElem()
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
