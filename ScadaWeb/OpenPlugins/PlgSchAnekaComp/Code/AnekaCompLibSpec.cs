// Copyright (c) Rapid Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Scada.Lang;
using Scada.Web.Plugins.PlgScheme;

namespace Scada.Web.Plugins.PlgSchAnekaComp.Code
{
    /// <summary>
    /// Specification of the aneka scheme components library
    /// <para>Спецификация библиотеки основных компонентов схемы</para>
    /// </summary>
    public class AnekaCompLibSpec : CompLibSpec
    {
        /// <summary>
        /// Получить префикс XML-элементов
        /// </summary>
        public override string XmlPrefix
        {
            get
            {
                return "aneka";
            }
        }

        /// <summary>
        /// Получить пространство имён XML-элементов
        /// </summary>
        public override string XmlNs
        {
            get
            {
                return "urn:rapidscada:scheme:aneka";
            }
        }

        /// <summary>
        /// Получить заголовок группы в редакторе
        /// </summary>
        public override string GroupHeader
        {
            get
            {
                return "Aneka";
            }
        }

        /// <summary>
        /// Получить ссылки на файлы CSS, которые необходимы для работы компонентов
        /// </summary>
        public override List<string> Styles
        {
            get
            {
                return new List<string>()
                {
                    "SchAnekaComp/css/anekacomp.min.css"
                };
            }
        }

        /// <summary>
        /// Получить ссылки на файлы JavaScript, которые необходимы для работы компонентов
        /// </summary>
        public override List<string> Scripts
        {
            get
            {
                return new List<string>()
                {
                    "SchAnekaComp/js/anekacomp-render.js",
                    "SchAnekaComp/js/svg.min.js",
                    "SchAnekaComp/js/gauge.min.js"
                };
            }
        }


        /// <summary>
        /// Создать элементы списка компонентов
        /// </summary>
        protected override List<CompItem> CreateCompItems()
        {
            return new List<CompItem>()
            {
                new CompItem(null /*Resources.button*/, typeof(Basic)),
                new CompItem(null /*Resources.led*/, typeof(Led)),
                new CompItem(null /*Resources.link*/, typeof(Level)),
                new CompItem(null /*Resources.link*/, typeof(Gauge))
            };
        }

        /// <summary>
        /// Создать фабрику компонентов
        /// </summary>
        protected override CompFactory CreateCompFactory()
        {
            return new AnekaCompFactory();
        }
    }
}