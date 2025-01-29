// Copyright (c) Rapid Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Scada.Web.Plugins.PlgScheme;
using Scada.Web.Plugins.PlgScheme.Model;

namespace Scada.Web.Plugins.PlgSchAnekaComp.Code
{
    /// <summary>
    /// Factory for creating aneka scheme components.
    /// <para>Фабрика для создания основных компонентов схемы.</para>
    /// </summary>
    public class AnekaCompFactory : CompFactory
    {
        /// <summary>
        /// Создать компонент схемы.
        /// </summary>
        public override ComponentBase CreateComponent(string typeName, bool nameIsShort)
        {
            if (NameEquals("Basic", typeof(Basic).FullName, typeName, nameIsShort))
                return new Basic();
            else if (NameEquals("Led", typeof(Led).FullName, typeName, nameIsShort))
                return new Led();
            else if (NameEquals("Level", typeof(Level).FullName, typeName, nameIsShort))
                return new Level();
            else if (NameEquals("Gauge", typeof(Gauge).FullName, typeName, nameIsShort))
                return new Gauge();
            else
                return null;
        }
    }
}
