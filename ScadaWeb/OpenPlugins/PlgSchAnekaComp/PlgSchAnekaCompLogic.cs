// Copyright (c) Rapid Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Scada.Web.Plugins.PlgSchAnekaComp.Code;
using Scada.Web.Plugins.PlgScheme;
using Scada.Web.Services;

namespace Scada.Web.Plugins.PlgSchAnekaComp
{
    /// <summary>
    /// Implements the plugin logic.
    /// <para>Реализует логику плагина.</para>
    /// </summary>
    public class PlgSchAnekaCompLogic : PluginLogic, ISchemeComp
    {
        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public PlgSchAnekaCompLogic(IWebContext webContext)
            : base(webContext)
        {
            Info = new PluginInfo();
        }

        /// <summary>
        /// Gets the specification of the component library.
        /// </summary>
        CompLibSpec ISchemeComp.CompLibSpec
        {
            get
            {
                return new AnekaCompLibSpec();
            }
        }
    }
}
