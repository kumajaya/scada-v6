// Copyright (c) Rapid Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel;

namespace Scada.Web.Plugins.PlgSchAnekaComp.Code
{
    /// <summary>
    /// Direction types for gauge component
    /// </summary>
    [TypeConverter(typeof(EnumConverter))]
    public enum DirectionTypes
    {
        /// <summary>
        /// Bottom to top direction
        /// </summary>
        [Description("Bottom to Top")]
        Default,

        /// <summary>
        /// Top to botton direction
        /// </summary>
        [Description("Top to Bottom")]
        One,

        /// <summary>
        /// Left to right direction
        /// </summary>
        [Description("Left to Right")]
        Two,

        /// <summary>
        /// Right to left direction
        /// </summary>
        [Description("Right to Left")]
        Three
    }
}
