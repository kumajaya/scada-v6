// Copyright (c) Rapid Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Scada.Web.Plugins.PlgScheme.Model;
using Scada.Web.Plugins.PlgScheme.Model.DataTypes;
using Scada.Web.Plugins.PlgScheme.Model.PropertyGrid;
using System.ComponentModel;
using System.Xml;

namespace Scada.Web.Plugins.PlgSchAnekaComp.Code
{
    /// <summary>
    /// Scheme component that represents a static svg image.
    /// <para>Компонент схемы, представляющий светодиод.</para>
    /// </summary>
    [Serializable]
    public class Basic : ComponentBase
    {
        /// <summary>
        /// Размер по умолчанию.
        /// </summary>
        public static readonly Size DefaultSize = new(32, 32);


        /// <summary>
        /// Конструктор.
        /// </summary>
        public Basic()
            : base()
        {
            //serBinder = PlgUtils.SerializationBinder;

            BackColor = "";
            BorderColor = "Gray";
            BorderWidth = 0;
            FillColor = "Silver";
            FillOpacity = 85;
            StrokeColor = "Gray";
            StrokeOpacity = 100;
            Size = DefaultSize;
            ImageName = "";
            ImageStretch = ImageStretches.Zoom;
        }

        /// <summary>
        /// Получить или установить цвет фона.
        /// </summary>
        #region Attributes
        [DisplayName("Fill color"), Category(Categories.Appearance)]
        [Description("The fill color of the SVG component.")]
        //[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
        #endregion
        public string FillColor { get; set; }

        /// <summary>
        /// Получить или установить непрозрачность границы.
        /// </summary>
        #region Attributes
        [DisplayName("Fill opacity"), Category(Categories.Appearance)]
        [Description("The fill opacity percentage of the SVG component.")]
        #endregion
        public int FillOpacity { get; set; }

        /// <summary>
        /// Получить или установить цвет границы.
        /// </summary>
        #region Attributes
        [DisplayName("Stroke color"), Category(Categories.Appearance)]
        [Description("The stroke color of the SVG component.")]
        //[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
        #endregion
        public string StrokeColor { get; set; }

        /// <summary>
        /// Получить или установить непрозрачность границы.
        /// </summary>
        #region Attributes
        [DisplayName("Stroke opacity"), Category(Categories.Appearance)]
        [Description("The stroke opacity percentage of the SVG component.")]
        #endregion
        public int StrokeOpacity { get; set; }

        /// <summary>
        /// Получить или установить наименование изображения
        /// </summary>
        #region Attributes
        [DisplayName("Image"), Category(Categories.Appearance)]
        [Description("The image from the collection of scheme SVG images.")]
        //[TypeConverter(typeof(ImageConverter)), Editor(typeof(ImageEditor), typeof(UITypeEditor))]
        [DefaultValue("")]
        #endregion
        public string ImageName { get; set; }

        /// <summary>
        /// Получить или установить растяжение изображения
        /// </summary>
        #region Attributes
        [DisplayName("Image stretch"), Category(Categories.Appearance)]
        [Description("Stretch the image.")]
        [DefaultValue(ImageStretches.None)]
        #endregion
        public ImageStretches ImageStretch { get; set; }

        /// <summary>
        /// Загрузить конфигурацию компонента из XML-узла.
        /// </summary>
        public override void LoadFromXml(XmlNode xmlNode)
        {
            base.LoadFromXml(xmlNode);

            FillColor = xmlNode.GetChildAsString("FillColor");
            FillOpacity = xmlNode.GetChildAsInt("FillOpacity");
            StrokeColor = xmlNode.GetChildAsString("StrokeColor");
            StrokeOpacity = xmlNode.GetChildAsInt("StrokeOpacity");
            ImageName = xmlNode.GetChildAsString("ImageName");
            ImageStretch = xmlNode.GetChildAsEnum<ImageStretches>("ImageStretch");
        }

        /// <summary>
        /// Сохранить конфигурацию компонента в XML-узле.
        /// </summary>
        public override void SaveToXml(XmlElement xmlElem)
        {
            base.SaveToXml(xmlElem);

            xmlElem.AppendElem("FillColor", FillColor);
            xmlElem.AppendElem("FillOpacity", FillOpacity);
            xmlElem.AppendElem("StrokeColor", StrokeColor);
            xmlElem.AppendElem("StrokeOpacity", StrokeOpacity);
            xmlElem.AppendElem("ImageName", ImageName);
            xmlElem.AppendElem("ImageStretch", ImageStretch);
        }
    }
}
