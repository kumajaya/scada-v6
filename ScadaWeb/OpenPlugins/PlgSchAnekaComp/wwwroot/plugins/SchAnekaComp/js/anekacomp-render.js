/*
 * Aneka components rendering
 *
 * Author   : Ketut Kumajaya
 * Created  : 2022
 *
 * Requires:
 * - jquery
 * - modal.js
 * - scheme-common.js
 * - scheme-render.js
 * - svg.js
 * - gauge.js
 */

/* Created with Inkscape (http://www.inkscape.org/) */
/* Optimized with SVGO (https://jakearchibald.github.io/svgomg/) with removeViewBox disable */
const aneka_basic_svg = `
<svg width="128" height="128" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
  <path opacity=".1" d="M21 12a9 9 0 1 1-18 0 9 9 0 0 1 18 0Z" fill="#323232"/>
  <path d="M21 12a9 9 0 1 1-18 0 9 9 0 0 1 18 0Z" stroke="#323232" stroke-width="2"/>
  <path d="m15 10-4 4-2-2m3 9a9 9 0 1 1 0-18 9 9 0 0 1 0 18Z" stroke="#323232" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
</svg>`;

const aneka_led_svg = `
<svg width="128" height="128" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
  <path opacity=".1" d="M21 12a9 9 0 1 1-18 0 9 9 0 0 1 18 0Z" fill="#323232"/>
  <path d="M21 12a9 9 0 1 1-18 0 9 9 0 0 1 18 0Z" stroke="#323232" stroke-width="2"/>
</svg>`;

const aneka_level_svg = `
<svg width="128" height="128" viewBox="0 0 24 96" xmlns="http://www.w3.org/2000/svg" stroke="#000">
  <path fill="#323232" stroke="none" stroke-width="0" d="M0 0L24 0L24 96L0 96L0 0Z"/>
</svg>`;

const svg_shapes = "path, rect, circle, ellipse, line, polyline, polygon, text, textPath, tspan"

/********** Extra Components Utilities **********/

scada.scheme.anekaCompUtils = {
    // Calculate a proportion of the specified value between minimum and maximum
    // from extracomprender.js
    calcProportion: (val, min, max) => {
        let proportion = min < max ? (val - min) / (max - min) : 0;

        if (proportion < 0) {
            proportion = 0;
        } else if (proportion > 1) {
            proportion = 1;
        }

        return proportion;
    },

    calcOpacity: (opacity) => {
        opacity = opacity / 100;
        if (opacity < 0) {
            opacity = 0;
        } else if (opacity > 1) {
            opacity = 1;
        }

        return opacity;
    }
};

/********** Basic Renderer **********/

scada.scheme.BasicRenderer = function () {
    scada.scheme.ComponentRenderer.call(this);
    this.SVG_IMAGE = aneka_basic_svg;
};

scada.scheme.BasicRenderer.prototype = Object.create(scada.scheme.ComponentRenderer.prototype);
scada.scheme.BasicRenderer.constructor = scada.scheme.BasicRenderer;

scada.scheme.BasicRenderer.prototype.setBackgroundImage = function (jqObj, image) {
    if (image && image.mediaType === "image/svg+xml") {
        jqObj.empty();
        const draw = SVG(jqObj[0]);
        draw.svg(atob(image.data));
    } else {
        jqObj.empty().append(this.SVG_IMAGE);
    }
};

scada.scheme.BasicRenderer.prototype.createDom = function (component, renderContext) {
    var props = component.props;

    var divComp = $("<div id='comp" + component.id + "' class='aneka'></div>");
    var divContainer = $("<div class='aneka-container'></div>");
    this.prepareComponent(divComp, component, false, true);
    this.setBackColor(divComp, props.backColor);

    var image = renderContext.getImage(props.imageName);
    divContainer.empty();
    const draw = SVG(divContainer[0]);
    draw.svg(image && image.mediaType === "image/svg+xml" ? atob(image.data) : this.SVG_IMAGE);

    draw.find('*').forEach(function (element) {
        if (element instanceof SVG.Element) {
            var color = element.attr("fill");
            if (color !== "none") {
                element.css({
                    fill: props.fillColor || "none",
                    'fill-opacity': scada.scheme.anekaCompUtils.calcOpacity(props.fillOpacity)
                });
            }
            color = element.attr("stroke");
            if (color !== "none") {
                element.css({
                    stroke: props.strokeColor || "none",
                    'stroke-opacity': scada.scheme.anekaCompUtils.calcOpacity(props.strokeOpacity)
                });
            }
        }
    });

    if (props.borderWidth > 0) {
        var divBorder = $("<div class='aneka-border'></div>");
        this.setBorderColor(divBorder, props.borderColor);
        this.setBorderWidth(divBorder, props.borderWidth);
        divBorder.css("opacity", scada.scheme.anekaCompUtils.calcOpacity(props.borderOpacity));
        divBorder.append(divContainer);
        divComp.append(divBorder);
    } else {
        divComp.append(divContainer);
    }

    var ImageStretches = scada.scheme.ImageStretches;
    let elem = divContainer.find("svg");
    switch (props.imageStretch) {
        case ImageStretches.FILL:
            if (elem.get(0)) {
                elem.get(0).setAttribute("preserveAspectRatio", "none");
            }
            elem.attr({ "width": "100%", "height": "100%" });
            break;
        case ImageStretches.ZOOM:
            elem.attr({ "width": "100%", "height": "100%" });
            break;
    }

    component.dom = divComp;
};

scada.scheme.BasicRenderer.prototype.refreshImages = function (component, renderContext, imageNames) {
    var props = component.props;

    if (Array.isArray(imageNames) && imageNames.includes(props.imageName)) {
        var divComp = component.dom;
        var divContainer = divComp.find(".aneka-container");

        var image = renderContext.getImage(props.imageName);
        divContainer.empty();
        const draw = SVG(divContainer[0]);
        draw.svg(image && image.mediaType === "image/svg+xml" ? atob(image.data) : this.SVG_IMAGE);

        draw.find('*').forEach(function (element) {
            if (element instanceof SVG.Element) {
                var color = element.attr("fill");
                if (color !== "none") {
                    element.css({
                        fill: props.fillColor || "none",
                        'fill-opacity': scada.scheme.anekaCompUtils.calcOpacity(props.fillOpacity)
                    });
                }
                color = element.attr("stroke");
                if (color !== "none") {
                    element.css({
                        stroke: props.strokeColor || "none",
                        'stroke-opacity': scada.scheme.anekaCompUtils.calcOpacity(props.strokeOpacity)
                    });
                }
            }
        });

        // force set size
        this.setSize(component, props.size.width, props.size.height);
    }
};

scada.scheme.BasicRenderer.prototype.setSize = function (component, width, height) {
    scada.scheme.ComponentRenderer.prototype.setSize.call(this, component, width, height);

    var props = component.props;
    var divComp = component.dom;
    var elem = divComp.find(".aneka-container svg");
    var ImageStretches = scada.scheme.ImageStretches;

    switch (props.imageStretch) {
        case ImageStretches.FILL:
            if (elem.get(0)) {
                elem.get(0).setAttribute("preserveAspectRatio", "none");
            }
            elem.attr({ "width": "100%", "height": "100%" });
            break;
        case ImageStretches.ZOOM:
            elem.attr({ "width": "100%", "height": "100%" });
            break;
    }
};

/********** Led Renderer **********/

scada.scheme.LedRenderer = function () {
    scada.scheme.BasicRenderer.call(this);
    this.SVG_IMAGE = aneka_led_svg;
};

scada.scheme.LedRenderer.prototype = Object.create(scada.scheme.BasicRenderer.prototype);
scada.scheme.LedRenderer.constructor = scada.scheme.LedRenderer;

scada.scheme.LedRenderer.prototype.createDom = function (component, renderContext) {
    scada.scheme.BasicRenderer.prototype.createDom.call(this, component, renderContext);

    var divComp = component.dom;
    this.bindAction(divComp, component, renderContext);
};

scada.scheme.LedRenderer.prototype.updateData = function (component, renderContext) {
    var props = component.props;

    if (props.inCnlNum > 0) {
        var divComp = component.dom;
        var cnlDataExt = renderContext.getCnlDataExt(props.inCnlNum);
        var fillColor = props.fillColor;

        // define fill color according to the channel status
        if (fillColor === this.STATUS_COLOR) {
            fillColor = this._getStatusColor(cnlDataExt);
        }

        // define fill color according to the led conditions and channel value
        if (cnlDataExt.d.stat > 0 && props.conditions) {
            var cnlVal = cnlDataExt.d.val;

            for (var cond of props.conditions) {
                if (scada.scheme.calc.conditionSatisfied(cond, props.maximum ? 100.0 * cnlVal / props.maximum : cnlVal)) {
                    fillColor = cond.color;
                    break;
                }
            }
        }

        // apply fill and stroke color using SVG.js
        const draw = SVG(divComp.find(".aneka-container svg")[0]);
        draw.find('*').forEach(function (element) {
            if (element instanceof SVG.Element) {
                var color = element.attr("fill");
                if (color !== "none") {
                    element.css({ fill: fillColor });
                }

                // apply stroke color if needed
                if (props.strokeColor === this.STATUS_COLOR) {
                    color = element.attr("stroke");
                    if (color !== "none") {
                        element.css({ stroke: this._getStatusColor(cnlDataExt) });
                    }
                }
            }
        });
    }
};

/********** Level Renderer **********/

scada.scheme.LevelRenderer = function () {
    scada.scheme.BasicRenderer.call(this);
    this.SVG_IMAGE = aneka_level_svg;
};

scada.scheme.LevelRenderer.prototype = Object.create(scada.scheme.BasicRenderer.prototype);
scada.scheme.LevelRenderer.constructor = scada.scheme.LevelRenderer;

scada.scheme.LevelRenderer.prototype.createDom = function (component, renderContext) {
    scada.scheme.BasicRenderer.prototype.createDom.call(this, component, renderContext);

    var divComp = component.dom;
    this.bindAction(divComp, component, renderContext);

    // divComp.find(".aneka-container svg").css({ "clip-path": "inset(50% 0 0 0)" });

    if (!renderContext.editMode) {
        divComp.addClass("undef");
    }
};

scada.scheme.LevelRenderer.prototype.updateData = function (component, renderContext) {
    scada.scheme.LedRenderer.prototype.updateData.call(this, component,renderContext);

    var props = component.props;

    if (props.inCnlNum > 0) {
        var divComp = component.dom;
        var cnlDataExt = renderContext.getCnlDataExt(props.inCnlNum);

        if (cnlDataExt.d.stat > 0) {
            divComp.removeClass("undef");
            var elem = divComp.find(".aneka-container svg");

            var proportion = scada.scheme.anekaCompUtils.calcProportion(cnlDataExt.d.val, props.minimum, props.maximum);
            var isVertical = props.size.width <= props.size.height;

            if (isVertical) {
                elem.css({ "clip-path": "inset(" + (100 - (proportion * 100)) + "% 0 0 0)" });
            } else {
                elem.css({ "clip-path": "inset(0 " + (100 - (proportion * 100)) + "% 0 0)" });
            }
        } else {
            divComp.addClass("undef");
        }
    }
};

/********** Gauge Renderer **********/

scada.scheme.GaugeRenderer = function () {
    scada.scheme.ComponentRenderer.call(this);
};

scada.scheme.GaugeRenderer.prototype = Object.create(scada.scheme.ComponentRenderer.prototype);
scada.scheme.GaugeRenderer.constructor = scada.scheme.GaugeRenderer;

scada.scheme.GaugeRenderer.prototype._setStyle = function (divContainer, styleProperty) {
    const styles = ['gauge-container', 'gauge-container two', 'gauge-container three',
        'gauge-container four', 'gauge-container five',
        'gauge-container six', 'gauge-container seven'];

    divContainer.addClass(styles[styleProperty] || 'gauge-container four');
};

scada.scheme.GaugeRenderer.prototype.createDom = function (component, renderContext) {
    var props = component.props;

    var divComp = $("<div>", { id: 'comp' + component.id, class: 'aneka' });
    var divContainer = $("<div>", { id: 'gauge' + component.id });
    this._setStyle(divContainer, props.styleProperty);
    this.prepareComponent(divComp, component, false, true);
    this.setBackColor(divComp, props.backColor);

    if (props.borderWidth > 0) {
        var divBorder = $("<div>", { class: 'aneka-border' });
        this.setBorderColor(divBorder, props.borderColor);
        this.setBorderWidth(divBorder, props.borderWidth);
        divBorder.css("opacity", scada.scheme.anekaCompUtils.calcOpacity(props.borderOpacity));
        divBorder.append(divContainer);
        divComp.append(divBorder);
    } else {
        divComp.append(divContainer);
    }

    component.dom = divComp;

    component.gauge = Gauge(divContainer[0], {
        min: props.min,
        max: props.max,
        dialRadius: props.dialRadius ? props.dialRadius : 40,
        dialStartAngle: props.dialStartAngle,
        dialEndAngle: props.dialEndAngle,
        value: props.max * 0.5,
        showValue: false,
        label: value => Math.round(value * 100) / 100
    });

    divContainer.find("svg.gauge > path.dial, svg.gauge > path.value").css({
        "stroke-width": props.dialWidth
    });

    divContainer.find("svg.gauge > path.dial").css({ "stroke": props.dialColor });
};

scada.scheme.GaugeRenderer.prototype.setSize = function (component, width, height) {
    scada.scheme.ComponentRenderer.prototype.setSize.call(this, component, width, height);

    var divComp = component.dom;
    var elem = divComp.find(".gauge-container");
    elem.attr({ "width": "100%", "height": "100%" });
};

scada.scheme.GaugeRenderer.prototype.updateData = function (component, renderContext) {
    scada.scheme.ComponentRenderer.prototype.updateData.call(this, component, renderContext);

    var props = component.props;
    if (props.inCnlNum > 0) {
        var cnlDataExt = renderContext.getCnlDataExt(props.inCnlNum);
        var cnlVal = cnlDataExt.d.stat > 0 ? cnlDataExt.d.val : 0;

        if (props.conditions && cnlDataExt.d.stat > 0) {
            var elem = component.dom.find(".gauge-container svg.gauge path.value");
            for (var cond of props.conditions) {
                if (scada.scheme.calc.conditionSatisfied(cond, 100.0*cnlVal/props.max)) {
                    elem.css({ "stroke": cond.color });
                    break;
                }
            }
        }

        component.gauge.setValueAnimated(cnlVal, 1);
    }
};

/********** Renderer Map **********/

// Add components to the renderer map
scada.scheme.rendererMap.set("Scada.Web.Plugins.PlgSchAnekaComp.Code.Basic", new scada.scheme.BasicRenderer());
scada.scheme.rendererMap.set("Scada.Web.Plugins.PlgSchAnekaComp.Code.Led", new scada.scheme.LedRenderer());
scada.scheme.rendererMap.set("Scada.Web.Plugins.PlgSchAnekaComp.Code.Level", new scada.scheme.LevelRenderer());
scada.scheme.rendererMap.set("Scada.Web.Plugins.PlgSchAnekaComp.Code.Gauge", new scada.scheme.GaugeRenderer());
