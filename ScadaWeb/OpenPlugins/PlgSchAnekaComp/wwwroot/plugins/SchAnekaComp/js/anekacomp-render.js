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

/********** Basic Renderer **********/

scada.scheme.BasicRenderer = function () {
    scada.scheme.ComponentRenderer.call(this);
    this.SVG_IMAGE = aneka_basic_svg;
};

scada.scheme.BasicRenderer.prototype = Object.create(scada.scheme.ComponentRenderer.prototype);
scada.scheme.BasicRenderer.constructor = scada.scheme.BasicRenderer;

/**
 * Robustly read a style property from an SVG element.
 *
 * SVG from Inkscape uses inline style ("style" attribute), e.g.:
 *   style="fill:#f0f;fill-opacity:1;stroke:none"
 * SVG from other tools may use presentational attributes, e.g.:
 *   fill="#f0f" stroke="none"
 *
 * element.css() reads computed/inline style first.
 * element.attr() reads presentational attribute as fallback.
 * Together they cover both cases robustly.
 */
scada.scheme.BasicRenderer.prototype._getSvgValue = function (element, prop) {
    return element.css(prop) || element.attr(prop);
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
 *
 * Writes via .css() (inline style) to reliably override both:
 * - Inkscape SVGs that use inline style (style="fill:...")
 * - SVGs that use presentational attributes (fill="...")
 *
 * Only targets actual shape elements via svg_shapes selector —
 * excludes <defs>, <title>, gradient nodes, etc.
 */
scada.scheme.BasicRenderer.prototype._applySvgColors = function (draw, props) {
    var calcOpacity = scada.scheme.anekaCompUtils.calcOpacity;
    draw.find(svg_shapes).forEach((element) => {
        if (!(element instanceof SVG.Element)) return;
        if (this._isSvgValueVisible(this._getSvgValue(element, "fill"))) {
            element.css({
                fill: props.fillColor || "none",
                'fill-opacity': calcOpacity(props.fillOpacity)
            });
        }
        if (this._isSvgValueVisible(this._getSvgValue(element, "stroke"))) {
            element.css({
                stroke: props.strokeColor || "none",
                'stroke-opacity': calcOpacity(props.strokeOpacity)
            });
        }
    });
};

/**
 * Render SVG content into a container element, returning the SVG.js draw instance.
 */
scada.scheme.BasicRenderer.prototype._renderSvg = function (container, image) {
    container.empty();
    const draw = SVG(container[0]);
    draw.svg(image && image.mediaType === "image/svg+xml" ? atob(image.data) : this.SVG_IMAGE);
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
    this._applySvgColors(draw, props);

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
    this._applySvgColors(draw, props);

    // Update cached draw instance
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
    var props = component.props;
    if (props.inCnlNum <= 0) return;

    var cnlDataExt = renderContext.getCnlDataExt(props.inCnlNum);
    var fillColor = props.fillColor;

    // Resolve fill color from channel status
    if (fillColor === this.STATUS_COLOR) {
        fillColor = this._getStatusColor(cnlDataExt);
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
    var strokeColor = props.strokeColor === this.STATUS_COLOR
        ? this._getStatusColor(cnlDataExt)
        : null;

    // Use cached draw instance; svg_shapes selector skips defs/title/etc.
    // Writes via .css() (inline style) to override both Inkscape inline style
    // and presentational attribute SVGs. Detects visibility via _getSvgValue()
    // which checks inline style first, then presentational attribute.
    var draw = component._svgDraw;
    if (!draw) return;

    draw.find(svg_shapes).forEach((element) => {
        if (!(element instanceof SVG.Element)) return;
        if (this._isSvgValueVisible(this._getSvgValue(element, "fill"))) {
            element.css({ fill: fillColor });
        }
        if (strokeColor && this._isSvgValueVisible(this._getSvgValue(element, "stroke"))) {
            element.css({ stroke: strokeColor });
        }
    });
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
    scada.scheme.LedRenderer.prototype.updateData.call(this, component, renderContext);

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

        // DirectionTypes enum serialized as string name (consistent with StyleProperties pattern).
        // "Default" = Bottom to Top, "One" = Top to Bottom,
        // "Two" = Left to Right, "Three" = Right to Left.
        // Integer fallback included for safety.
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

    // Gauge.js does not expose dialWidth or dialColor as constructor options,
    // so we patch the rendered SVG directly after initialization.
    // This depends on Gauge.js internal DOM structure (svg.gauge > path.dial/path.value).
    // If Gauge.js is upgraded, verify these selectors still hold.
    //
    // Uses .css() (inline style) — consistent with how gauge.js itself writes color
    // internally via gaugeValuePath.style.stroke in setGaugeColor(). Using .attr()
    // would lose to existing inline style set by gauge.js.
    //
    // Guard against null/undefined: if props are not set, CSS variant rules apply.
    if (props.dialWidth) {
        divContainer.find("svg.gauge > path.dial, svg.gauge > path.value")
            .css("stroke-width", props.dialWidth);
    }
    if (props.dialColor) {
        divContainer.find("svg.gauge > path.dial")
            .css("stroke", props.dialColor);
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
        // Note: GaugeRenderer uses props.max (consistent with Gauge() constructor options above).
        // LedRenderer/LevelRenderer use props.maximum — different component props schema.
        var normalizedVal = 100.0 * cnlVal / props.max;
        // Use .css() (inline style) — consistent with gauge.js internal setGaugeColor()
        // which writes via gaugeValuePath.style.stroke. Using .attr() would lose
        // to the existing inline style already set by gauge.js.
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
