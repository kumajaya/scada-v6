﻿// Copyright (c) Rapid Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Scada.Web.Plugins.PlgChart.View
{
    /// <summary>
    /// Implements the plugin user interface for the Administrator application.
    /// <para>Реализует пользовательский интерфейс плагина для приложения Администратор.</para>
    /// </summary>
    public class PlgChartView : PluginView
    {
        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public PlgChartView()
        {
            Info = new ChartPluginInfo();
        }
    }
}
