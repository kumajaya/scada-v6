// Copyright (c) Rapid Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel;

namespace Scada.Web.Plugins.PlgSchAnekaComp.Code
{
    /// <summary>
    /// Component properties that can be bound to an input channel
    /// <para>Свойства компонента, которые могут быть привязаны к входному каналу</para>
    /// </summary>
    [TypeConverter(typeof(EnumConverter))]
    public enum StyleProperties
    {
        /// <summary>
        /// Default style
        /// </summary>
        #region Attributes
        [Description("Default style")]
        #endregion
        Default,

        /// <summary>
        /// Alternative style
        /// </summary>
        #region Attributes
        [Description("Alternative two")]
        #endregion
        Two,

        /// <summary>
        /// Alternative style
        /// </summary>
        #region Attributes
        [Description("Alternative three")]
        #endregion
        Three,

        /// <summary>
        /// Alternative style
        /// </summary>
        #region Attributes
        [Description("Alternative four")]
        #endregion
        Four,

        /// <summary>
        /// Alternative style
        /// </summary>
        #region Attributes
        [Description("Alternative five")]
        #endregion
        Five,

        /// <summary>
        /// Alternative style
        /// </summary>
        #region Attributes
        [Description("Alternative six")]
        #endregion
        Six,

        /// <summary>
        /// Alternative style
        /// </summary>
        #region Attributes
        [Description("Alternative seven")]
        #endregion
        Seven
    }
}
