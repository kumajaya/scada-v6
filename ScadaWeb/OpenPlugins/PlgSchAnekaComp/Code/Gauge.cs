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
    /// Scheme component that represents a svg gauge.
    /// <para>Компонент схемы, представляющий светодиод.</para>
    /// </summary>
    [Serializable]
    public class Gauge : ComponentBase
    {
        /// <summary>
        /// Размер по умолчанию.
        /// </summary>
        public static readonly Size DefaultSize = new(100, 100);


        /// <summary>
        /// Конструктор.
        /// </summary>
        public Gauge()
            : base()
        {
            //serBinder = PlgUtils.SerializationBinder;

            BackColor = "";
            BorderColor = "Gray";
            BorderWidth = 0;
            DialColor = "#334455";
            DialRadius = 40;
            DialStartAngle = 10;
            DialEndAngle = 60;
            DialWidth = 10;
            Min = 0;
            Max = 100;
            Conditions = new List<ColorCondition>();
            AddDefaultConditions();
            StyleProperty = StyleProperties.Four;
            Size = DefaultSize;
            InCnlNum = 0;
            Size = DefaultSize;
        }

        #region Attributes
        [DisplayName("Dial color"), Category(Categories.Appearance)]
        [Description("The dial color of the gauge.")]
        //[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
        #endregion
        public string DialColor { get; set; }

        #region Attributes
        [DisplayName("Dial radius"), Category(Categories.Appearance)]
        [Description("The radius of the dial gauge.")]
        #endregion
        public int DialRadius { get; set; }

        #region Attributes
        [DisplayName("Dial end angle"), Category(Categories.Appearance)]
        [Description("The end angle of the gauge.")]
        #endregion
        public double DialEndAngle { get; set; }

        #region Attributes
        [DisplayName("Dial start angle"), Category(Categories.Appearance)]
        [Description("The start angle of the gauge.")]
        #endregion
        public double DialStartAngle { get; set; }

        #region Attributes
        [DisplayName("Dial width"), Category(Categories.Appearance)]
        [Description("The width of the dial gauge.")]
        #endregion
        public int DialWidth { get; set; }

        #region Attributes
        [DisplayName("Maximum value"), Category(Categories.Appearance)]
        [Description("The maximum value of the gauge.")]
        #endregion
        public int Max { get; set; }

        #region Attributes
        [DisplayName("Minimum value"), Category(Categories.Appearance)]
        [Description("The minimum value of the gauge.")]
        #endregion
        public int Min { get; set; }

        #region Attributes
        [DisplayName("Conditions"), Category(Categories.Behavior)]
        [Description("The conditions determining the stroke color depending on the value of the input channel.")]
        [DefaultValue(null), TypeConverter(typeof(CollectionConverter))]
        //[Editor(typeof(CollectionEditor), typeof(UITypeEditor))]
        #endregion
        public List<ColorCondition> Conditions { get; protected set; }

        #region Attributes
        [DisplayName("Style property"), Category(Categories.Behavior)]
        [Description("The gauge property that can be used to select the style.")]
        [DefaultValue(StyleProperties.Four)]
        #endregion
        public StyleProperties StyleProperty { get; set; }

        #region Attributes
        [DisplayName("Input channel"), Category(Categories.Data)]
        [Description("The input channel number associated with the component.")]
        [DefaultValue(0)]
        #endregion
        public int InCnlNum { get; set; }

        protected void AddDefaultConditions()
        {
            Conditions.Add(new ColorCondition()
            {
                CompareOperator1 = CompareOperators.LessThan,
                CompareArgument1 = 25.0,
                Color = "#5ee432"
            });

            Conditions.Add(new ColorCondition()
            {
                CompareOperator1 = CompareOperators.LessThan,
                CompareArgument1 = 50.0,
                Color = "#fffa50"
            });

            Conditions.Add(new ColorCondition()
            {
                CompareOperator1 = CompareOperators.LessThan,
                CompareArgument1 = 75.0,
                Color = "#f7aa38"
            });

            Conditions.Add(new ColorCondition()
            {
                CompareOperator1 = CompareOperators.LessThan,
                CompareArgument1 = 100.0,
                Color = "#ef4655"
            });
        }

        /// <summary>
        /// Загрузить конфигурацию компонента из XML-узла.
        /// </summary>
        public override void LoadFromXml(XmlNode xmlNode)
        {
            base.LoadFromXml(xmlNode);

            XmlNode conditionsNode = xmlNode.SelectSingleNode("Conditions");
            if (conditionsNode != null)
            {
                Conditions = new List<ColorCondition>();
                XmlNodeList conditionNodes = conditionsNode.SelectNodes("Condition");
                foreach (XmlNode conditionNode in conditionNodes)
                {
                    ColorCondition condition = new ColorCondition { SchemeView = SchemeView };
                    condition.LoadFromXml(conditionNode);
                    Conditions.Add(condition);
                }
            }

            DialColor = xmlNode.GetChildAsString("DialColor");
            DialRadius = xmlNode.GetChildAsInt("DialRadius");
            DialStartAngle = xmlNode.GetChildAsDouble("DialStartAngle");
            DialEndAngle = xmlNode.GetChildAsDouble("DialEndAngle");
            DialWidth = xmlNode.GetChildAsInt("DialWidth");
            Min = xmlNode.GetChildAsInt("Min");
            Max = xmlNode.GetChildAsInt("Max");
            StyleProperty = xmlNode.GetChildAsEnum<StyleProperties>("StyleProperty");
            InCnlNum = xmlNode.GetChildAsInt("InCnlNum");
        }

        /// <summary>
        /// Сохранить конфигурацию компонента в XML-узле.
        /// </summary>
        public override void SaveToXml(XmlElement xmlElem)
        {
            base.SaveToXml(xmlElem);

            XmlElement conditionsElem = xmlElem.AppendElem("Conditions");
            foreach (Condition condition in Conditions)
            {
                XmlElement conditionElem = conditionsElem.AppendElem("Condition");
                condition.SaveToXml(conditionElem);
            }

            xmlElem.AppendElem("DialColor", DialColor);
            xmlElem.AppendElem("DialRadius", DialRadius);
            xmlElem.AppendElem("DialStartAngle", DialStartAngle);
            xmlElem.AppendElem("DialEndAngle", DialEndAngle);
            xmlElem.AppendElem("DialWidth", DialWidth);
            xmlElem.AppendElem("Min", Min);
            xmlElem.AppendElem("Max", Max);
            xmlElem.AppendElem("StyleProperty", StyleProperty);
            xmlElem.AppendElem("InCnlNum", InCnlNum);
        }
    }
}
