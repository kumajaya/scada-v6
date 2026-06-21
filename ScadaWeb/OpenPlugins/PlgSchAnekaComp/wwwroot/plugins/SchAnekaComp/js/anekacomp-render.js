/*
 * Aneka components rendering
 *
 * Author   : Ketut Kumajaya
 * Created  : 2022
 * Refactored: 2025
 *
 * Requires:
 * - jquery
 * - modal.js
 * - scheme-common.js
 * - scheme-render.js
 * - svg.js
 * - gauge.js
 */

'use strict';

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

const svg_shapes = "path, rect, circle, ellipse, line, polyline, polygon, text, textPath, tspan";

/********** Gauge Style Map **********/

const GAUGE_STYLES = {
    0: 'gauge-container',
    1: 'gauge-container two',
    2: 'gauge-container three',
    3: 'gauge-container four',
    4: 'gauge-container five',
    5: 'gauge-container six',
    6: 'gauge-container seven'
};

/********** Extra Components Utilities **********/

scada.scheme.anekaCompUtils = {
    // Calculate a proportion of the specified value between minimum and maximum
    calcProportion: (val, min, max) => {
        if (min >= max) return 0;
        return Math.min(1, Math.max(0, (val - min) / (max - min)));
    },

    calcOpacity: (opacity) => {
        return Math.min(1, Math.max(0, opacity / 100));
    }
};

/********** ComponentRenderer Extensions **********/
// Shared helpers added to the base prototype so all renderers can use them.

/**
 * Wrap an inner element with a border div if borderWidth > 0.
 * Returns the wrapper (or the original element if no border needed).
 */
scada.scheme.ComponentRenderer.prototype._buildBorderWrapper = function (inner, props) {
    if (props.borderWidth <= 0) return inner;

    var divBorder = $("<div class='aneka-border'></div>");
    this.setBorderColor(divBorder, props.borderColor);
    this.setBorderWidth(divBorder, props.borderWidth);
    divBorder.css("opacity", scada.scheme.anekaCompUtils.calcOpacity(props.borderOpacity));
    divBorder.append(inner);
    return divBorder;
};

/**
 * Apply width/height stretch attributes to an SVG element.
 */
scada.scheme.ComponentRenderer.prototype._applyImageStretch = function (elem, imageStretch) {
    var ImageStretches = scada.scheme.ImageStretches;
    switch (imageStretch) {
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

/********** Shared LED Color Application **********/

/**
 * Apply LED fill and stroke colors based on channel data and conditions.
 * Uses cached colorable elements if available, otherwise queries and caches.
 */
function applyLedColors(component, renderContext, draw) {
    var props = component.props;
    if (props.inCnlNum <= 0) return;

    var cnlDataExt = renderContext.getCnlDataExt(props.inCnlNum);
    var fillColor = props.fillColor;

    // Resolve fill color from channel status
    if (fillColor === scada.scheme.ComponentRenderer.STATUS_COLOR) {
        fillColor = scada.scheme.ComponentRenderer.prototype._getStatusColor(cnlDataExt);
    }

    // Resolve fill color from conditions
    if (cnlDataExt.d.stat > 0 && props.conditions) {
        var cnlVal = props.maximum ? 100.0 * cnlDataExt.d.val / props.maximum : cnlDataExt.d.val;
        for (var cond of props.conditions) {
            if (scada.scheme.calc.conditionSatisfied(cond, cnlVal)) {
                fillColor = cond.color;
                break;
            }
        }
    }

    // Resolve stroke color
    var strokeColor = props.strokeColor === scada.scheme.ComponentRenderer.STATUS_COLOR
        ? scada.scheme.ComponentRenderer.prototype._getStatusColor(cnlDataExt)
        : null;

    // Use cached or newly cached colorable elements
    var colorableElements = component._colorableElements;
    if (!colorableElements) {
        // Cache the elements that have visible fill or stroke (based on original attributes)
        var allElements = draw.find(svg_shapes);
        colorableElements = [];
        allElements.forEach(function (el) {
            var hasVisibleFill = false;
            var hasVisibleStroke = false;
            // Check fill attribute
            var fillAttr = el.attr('fill');
            if (fillAttr !== undefined && fillAttr !== 'none' && fillAttr !== 'transparent') {
                hasVisibleFill = true;
            }
            var strokeAttr = el.attr('stroke');
            if (strokeAttr !== undefined && strokeAttr !== 'none' && strokeAttr !== 'transparent') {
                hasVisibleStroke = true;
            }
            if (hasVisibleFill || hasVisibleStroke) {
                colorableElements.push(el);
            }
        });
        component._colorableElements = colorableElements;
    }

    // Apply colors via inline style
    colorableElements.forEach(function (el) {
        if (!(el instanceof SVG.Element)) return;
        // Only apply fill if the element originally had a visible fill
        var fillAttr = el.attr('fill');
        if (fillAttr !== undefined && fillAttr !== 'none' && fillAttr !== 'transparent') {
            el.css({ fill: fillColor });
        }
        if (strokeColor) {
            var strokeAttr = el.attr('stroke');
            if (strokeAttr !== undefined && strokeAttr !== 'none' && strokeAttr !== 'transparent') {
                el.css({ stroke: strokeColor });
            }
        }
    });
}

/********** Basic Renderer **********/

scada.scheme.BasicRenderer = function () {
    scada.scheme.ComponentRenderer.call(this);
    this.SVG_IMAGE = aneka_basic_svg;
};

scada.scheme.BasicRenderer.prototype = Object.create(scada.scheme.ComponentRenderer.prototype);
scada.scheme.BasicRenderer.constructor = scada.scheme.BasicRenderer;

/**
 * Robustly read a style property from an SVG element.
 * First checks the presentational attribute (source of truth for "none"/transparency),
 * then falls back to computed style.
 */
scada.scheme.BasicRenderer.prototype._getSvgValue = function (element, prop) {
    var attrVal = element.attr(prop);
    // If attribute exists (including empty string), use it; otherwise use computed style
    return attrVal !== undefined ? attrVal : element.css(prop);
};

/**
 * Check if a fill or stroke value is effectively invisible/absent.
 * Covers: null, undefined, empty string, "none", "transparent".
 */
scada.scheme.BasicRenderer.prototype._isSvgValueVisible = function (val) {
    return val && val !== "none" && val !== "transparent";
};

/**
 * Apply fill and stroke colors from props to all shape elements inside a draw instance.
 * Only targets actual shape elements via svg_shapes selector.
 * Uses cached colorable elements to improve performance on subsequent updates.
 */
scada.scheme.BasicRenderer.prototype._applySvgColors = function (draw, props) {
    var calcOpacity = scada.scheme.anekaCompUtils.calcOpacity;
    var colorableElements = draw.find(svg_shapes).filter(function (el) {
        // Only select elements that have a visible fill or stroke attribute originally
        var fillAttr = el.attr('fill');
        var strokeAttr = el.attr('stroke');
        return (fillAttr !== undefined && fillAttr !== 'none' && fillAttr !== 'transparent') ||
               (strokeAttr !== undefined && strokeAttr !== 'none' && strokeAttr !== 'transparent');
    });

    colorableElements.forEach(function (el) {
        if (!(el instanceof SVG.Element)) return;
        var fillAttr = el.attr('fill');
        if (fillAttr !== undefined && fillAttr !== 'none' && fillAttr !== 'transparent') {
            el.css({
                fill: props.fillColor || "none",
                'fill-opacity': calcOpacity(props.fillOpacity)
            });
        }
        var strokeAttr = el.attr('stroke');
        if (strokeAttr !== undefined && strokeAttr !== 'none' && strokeAttr !== 'transparent') {
            el.css({
                stroke: props.strokeColor || "none",
                'stroke-opacity': calcOpacity(props.strokeOpacity)
            });
        }
    });

    // Store these elements for later updates (LED/Level)
    return colorableElements;
};

/**
 * Render SVG content into a container element, returning the SVG.js draw instance.
 * Handles Base64 decoding errors gracefully.
 */
scada.scheme.BasicRenderer.prototype._renderSvg = function (container, image) {
    container.empty();
    const draw = SVG(container[0]);
    var svgContent = this.SVG_IMAGE;
    if (image && image.mediaType === "image/svg+xml") {
        try {
            svgContent = atob(image.data);
        } catch (e) {
            console.warn('Failed to decode SVG image data, using default SVG:', e);
        }
    }
    draw.svg(svgContent);
    return draw;
};

scada.scheme.BasicRenderer.prototype.createDom = function (component, renderContext) {
    var props = component.props;

    var divComp = $("<div id='comp" + component.id + "' class='aneka'></div>");
    var divContainer = $("<div class='aneka-container'></div>");
    this.prepareComponent(divComp, component, false, true);
    this.setBackColor(divComp, props.backColor);

    var image = renderContext.getImage(props.imageName);
    var draw = this._renderSvg(divContainer, image);
    // Cache colorable elements for later use
    component._colorableElements = this._applySvgColors(draw, props);
    // Cache draw instance to avoid re-constructing on each update
    component._svgDraw = draw;

    divComp.append(this._buildBorderWrapper(divContainer, props));
    this._applyImageStretch(divContainer.find("svg"), props.imageStretch);

    component.dom = divComp;
};

scada.scheme.BasicRenderer.prototype.refreshImages = function (component, renderContext, imageNames) {
    var props = component.props;

    if (!Array.isArray(imageNames) || !imageNames.includes(props.imageName)) return;

    var divContainer = component.dom.find(".aneka-container");
    var image = renderContext.getImage(props.imageName);
    var draw = this._renderSvg(divContainer, image);
    component._colorableElements = this._applySvgColors(draw, props);
    component._svgDraw = draw;

    // Force re-apply size
    this.setSize(component, props.size.width, props.size.height);
};

scada.scheme.BasicRenderer.prototype.setSize = function (component, width, height) {
    scada.scheme.ComponentRenderer.prototype.setSize.call(this, component, width, height);

    var props = component.props;
    var elem = component.dom.find(".aneka-container svg");
    this._applyImageStretch(elem, props.imageStretch);
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
    this.bindAction(component.dom, component, renderContext);
};

scada.scheme.LedRenderer.prototype.updateData = function (component, renderContext) {
    // Use shared function to apply LED colors
    applyLedColors(component, renderContext, component._svgDraw);
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

    if (!renderContext.editMode) {
        divComp.addClass("undef");
    }
};

scada.scheme.LevelRenderer.prototype.updateData = function (component, renderContext) {
    // Apply LED color logic first (shared fill/stroke from conditions)
    applyLedColors(component, renderContext, component._svgDraw);

    var props = component.props;
    if (props.inCnlNum <= 0) return;

    var divComp = component.dom;
    var cnlDataExt = renderContext.getCnlDataExt(props.inCnlNum);

    if (cnlDataExt.d.stat > 0) {
        divComp.removeClass("undef");

        var proportion = scada.scheme.anekaCompUtils.calcProportion(
            cnlDataExt.d.val, props.minimum, props.maximum
        );
        var clipValue = (100 - proportion * 100).toFixed(2) + "%";
        var clipPath;

        // DirectionTypes enum: 0=Default (Bottom to Top), 1=Top to Bottom, 2=Left to Right, 3=Right to Left
        if (props.directionType != null) {
            switch (props.directionType) {
                case 1:  clipPath = "inset(0 0 " + clipValue + " 0)"; break; // Top to Bottom
                case 2:  clipPath = "inset(0 " + clipValue + " 0 0)"; break; // Left to Right
                case 3:  clipPath = "inset(0 0 0 " + clipValue + ")"; break; // Right to Left
                default: clipPath = "inset(" + clipValue + " 0 0 0)";        // Bottom to Top
            }
        } else {
            // Auto-detect from dimensions
            var isVertical = props.size.width <= props.size.height;
            clipPath = isVertical
                ? "inset(" + clipValue + " 0 0 0)"
                : "inset(0 " + clipValue + " 0 0)";
        }

        divComp.find(".aneka-container svg").css({ "clip-path": clipPath });
    } else {
        divComp.addClass("undef");
    }
};

/********** Gauge Renderer **********/

scada.scheme.GaugeRenderer = function () {
    scada.scheme.ComponentRenderer.call(this);
};

scada.scheme.GaugeRenderer.prototype = Object.create(scada.scheme.ComponentRenderer.prototype);
scada.scheme.GaugeRenderer.constructor = scada.scheme.GaugeRenderer;

scada.scheme.GaugeRenderer.prototype._setStyle = function (divContainer, styleProperty) {
    divContainer.addClass(GAUGE_STYLES[styleProperty] || 'gauge-container four');
};

scada.scheme.GaugeRenderer.prototype.createDom = function (component, renderContext) {
    var props = component.props;

    var divComp = $("<div>", { id: 'comp' + component.id, class: 'aneka' });
    var divContainer = $("<div>", { id: 'gauge' + component.id });
    this._setStyle(divContainer, props.styleProperty);
    this.prepareComponent(divComp, component, false, true);
    this.setBackColor(divComp, props.backColor);

    divComp.append(this._buildBorderWrapper(divContainer, props));
    component.dom = divComp;

    component.gauge = Gauge(divContainer[0], {
        min: props.min,
        max: props.max,
        dialRadius: props.dialRadius || 40,
        dialStartAngle: props.dialStartAngle,
        dialEndAngle: props.dialEndAngle,
        value: props.max * 0.5,
        showValue: false,
        label: value => Math.round(value * 100) / 100
    });

    // Apply dialWidth and dialColor if provided.
    // These rely on Gauge.js internal DOM structure.
    // We try to apply them, and if selectors fail, we log a warning but continue.
    if (props.dialWidth) {
        var $paths = divContainer.find("svg.gauge > path.dial, svg.gauge > path.value");
        if ($paths.length) {
            $paths.css("stroke-width", props.dialWidth);
        } else {
            console.warn('Gauge internal structure may have changed; dialWidth might not apply.');
        }
    }
    if (props.dialColor) {
        var $dial = divContainer.find("svg.gauge > path.dial");
        if ($dial.length) {
            $dial.css("stroke", props.dialColor);
        } else {
            console.warn('Gauge internal structure may have changed; dialColor might not apply.');
        }
    }
};

scada.scheme.GaugeRenderer.prototype.setSize = function (component, width, height) {
    scada.scheme.ComponentRenderer.prototype.setSize.call(this, component, width, height);
    component.dom.find(".gauge-container").attr({ "width": "100%", "height": "100%" });
};

scada.scheme.GaugeRenderer.prototype.updateData = function (component, renderContext) {
    scada.scheme.ComponentRenderer.prototype.updateData.call(this, component, renderContext);

    var props = component.props;
    if (props.inCnlNum <= 0) return;

    var cnlDataExt = renderContext.getCnlDataExt(props.inCnlNum);
    var cnlVal = cnlDataExt.d.stat > 0 ? cnlDataExt.d.val : 0;

    if (props.conditions && cnlDataExt.d.stat > 0) {
        var elem = component.dom.find(".gauge-container svg.gauge path.value");
        var normalizedVal = 100.0 * cnlVal / props.max;
        for (var cond of props.conditions) {
            if (scada.scheme.calc.conditionSatisfied(cond, normalizedVal)) {
                elem.css("stroke", cond.color);
                break;
            }
        }
    }

    component.gauge.setValueAnimated(cnlVal, 1);
};

/********** Renderer Map **********/

scada.scheme.rendererMap.set("Scada.Web.Plugins.PlgSchAnekaComp.Code.Basic", new scada.scheme.BasicRenderer());
scada.scheme.rendererMap.set("Scada.Web.Plugins.PlgSchAnekaComp.Code.Led", new scada.scheme.LedRenderer());
scada.scheme.rendererMap.set("Scada.Web.Plugins.PlgSchAnekaComp.Code.Level", new scada.scheme.LevelRenderer());
scada.scheme.rendererMap.set("Scada.Web.Plugins.PlgSchAnekaComp.Code.Gauge", new scada.scheme.GaugeRenderer());
