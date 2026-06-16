(function () {
  'use strict';

  var MAX_SIDE = 16384;

  function getCanvas(canvasId) {
    var el = document.getElementById(canvasId);
    if (!el || el.tagName !== 'CANVAS') {
      return null;
    }
    return el;
  }

  function loadImage(dataUrl) {
    return new Promise(function (resolve, reject) {
      var img = new Image();
      img.onload = function () {
        resolve(img);
      };
      img.onerror = function () {
        reject(new Error('image load failed'));
      };
      img.src = dataUrl;
    });
  }

  function clampSide(n) {
    return Math.max(1, Math.min(MAX_SIDE, Math.floor(Number(n))));
  }

  function parseBg(bgColor, transparent) {
    if (transparent) {
      return null;
    }
    if (typeof bgColor === 'string' && /^#[0-9a-fA-F]{6}$/.test(bgColor)) {
      return bgColor;
    }
    return '#ffffff';
  }

  function drawContained(ctx, img, x, y, w, h) {
    var iw = img.naturalWidth;
    var ih = img.naturalHeight;
    if (iw <= 0 || ih <= 0 || w <= 0 || h <= 0) {
      return;
    }
    var scale = Math.min(w / iw, h / ih);
    var dw = iw * scale;
    var dh = ih * scale;
    var dx = x + (w - dw) / 2;
    var dy = y + (h - dh) / 2;
    ctx.drawImage(img, dx, dy, dw, dh);
  }

  function drawFilled(ctx, img, x, y, w, h) {
    ctx.drawImage(img, x, y, w, h);
  }

  function layoutHorizontal(images, gap, align) {
    var n = images.length;
    var targetH = 0;
    for (var i = 0; i < n; i++) {
      targetH = Math.max(targetH, images[i].naturalHeight);
    }
    targetH = clampSide(targetH);

    var placements = [];
    var totalW = 0;
    for (var j = 0; j < n; j++) {
      var img = images[j];
      var sw = clampSide((img.naturalWidth * targetH) / img.naturalHeight);
      var sh = targetH;
      placements.push({ img: img, w: sw, h: sh });
      totalW += sw;
    }
    totalW += gap * Math.max(0, n - 1);
    totalW = clampSide(totalW);

    var positions = [];
    var x = 0;
    for (var k = 0; k < n; k++) {
      var p = placements[k];
      var y = 0;
      if (align === 'center') {
        y = (targetH - p.h) / 2;
      } else if (align === 'bottom') {
        y = targetH - p.h;
      }
      positions.push({ img: p.img, x: x, y: y, w: p.w, h: p.h, mode: 'fill' });
      x += p.w + gap;
    }
    return { width: totalW, height: targetH, positions: positions };
  }

  function layoutVertical(images, gap, align) {
    var n = images.length;
    var targetW = 0;
    for (var i = 0; i < n; i++) {
      targetW = Math.max(targetW, images[i].naturalWidth);
    }
    targetW = clampSide(targetW);

    var placements = [];
    var totalH = 0;
    for (var j = 0; j < n; j++) {
      var img = images[j];
      var sw = targetW;
      var sh = clampSide((img.naturalHeight * targetW) / img.naturalWidth);
      placements.push({ img: img, w: sw, h: sh });
      totalH += sh;
    }
    totalH += gap * Math.max(0, n - 1);
    totalH = clampSide(totalH);

    var positions = [];
    var y = 0;
    for (var k = 0; k < n; k++) {
      var p = placements[k];
      var x = 0;
      if (align === 'center') {
        x = (targetW - p.w) / 2;
      } else if (align === 'right') {
        x = targetW - p.w;
      }
      positions.push({ img: p.img, x: x, y: y, w: p.w, h: p.h, mode: 'fill' });
      y += p.h + gap;
    }
    return { width: targetW, height: totalH, positions: positions };
  }

  function layoutGrid(images, cols, rows, gap, cellFit) {
    var n = images.length;
    cols = Math.max(1, Math.min(20, Math.floor(Number(cols)) || 1));
    rows = Math.max(1, Math.min(20, Math.floor(Number(rows)) || Math.ceil(n / cols)));

    var colWidths = new Array(cols).fill(0);
    var rowHeights = new Array(rows).fill(0);

    for (var r = 0; r < rows; r++) {
      for (var c = 0; c < cols; c++) {
        var idx = r * cols + c;
        if (idx >= n) {
          continue;
        }
        var img = images[idx];
        colWidths[c] = Math.max(colWidths[c], img.naturalWidth);
        rowHeights[r] = Math.max(rowHeights[r], img.naturalHeight);
      }
    }

    for (var ci = 0; ci < cols; ci++) {
      colWidths[ci] = clampSide(colWidths[ci]);
    }
    for (var ri = 0; ri < rows; ri++) {
      rowHeights[ri] = clampSide(rowHeights[ri]);
    }

    var totalW = colWidths.reduce(function (a, b) {
      return a + b;
    }, 0);
    totalW += gap * Math.max(0, cols - 1);
    var totalH = rowHeights.reduce(function (a, b) {
      return a + b;
    }, 0);
    totalH += gap * Math.max(0, rows - 1);
    totalW = clampSide(totalW);
    totalH = clampSide(totalH);

    var colX = [];
    var cx = 0;
    for (var c2 = 0; c2 < cols; c2++) {
      colX[c2] = cx;
      cx += colWidths[c2] + gap;
    }
    var rowY = [];
    var cy = 0;
    for (var r2 = 0; r2 < rows; r2++) {
      rowY[r2] = cy;
      cy += rowHeights[r2] + gap;
    }

    var positions = [];
    for (var r3 = 0; r3 < rows; r3++) {
      for (var c3 = 0; c3 < cols; c3++) {
        var idx2 = r3 * cols + c3;
        if (idx2 >= n) {
          continue;
        }
        positions.push({
          img: images[idx2],
          x: colX[c3],
          y: rowY[r3],
          w: colWidths[c3],
          h: rowHeights[r3],
          mode: cellFit ? 'contain' : 'fill'
        });
      }
    }
    return { width: totalW, height: totalH, positions: positions };
  }

  window.imageMerge = {
    /**
     * @param {string} canvasId
     * @param {string[]} dataUrls ordered image data URLs
     * @param {{ mode: string, gap?: number, bgColor?: string, bgTransparent?: boolean, cols?: number, rows?: number, align?: string, cellFit?: boolean }} options
     * @returns {Promise<{ ok: boolean, width?: number, height?: number, error?: string }>}
     */
    merge: function (canvasId, dataUrls, options) {
      options = options || {};
      var canvas = getCanvas(canvasId);
      if (!canvas) {
        return Promise.resolve({ ok: false, error: 'canvas not found' });
      }
      if (!Array.isArray(dataUrls) || dataUrls.length < 1) {
        return Promise.resolve({ ok: false, error: 'no images' });
      }

      var gap = Math.max(0, Math.min(200, Math.floor(Number(options.gap) || 0)));
      var mode = options.mode || 'horizontal';
      var align = options.align || 'center';
      var cellFit = options.cellFit !== false;
      var bg = parseBg(options.bgColor, !!options.bgTransparent);

      return Promise.all(dataUrls.map(loadImage))
        .then(function (images) {
          var layout;
          if (mode === 'vertical') {
            layout = layoutVertical(images, gap, align);
          } else if (mode === 'grid') {
            var cols = options.cols || 2;
            var rows = Math.ceil(images.length / Math.max(1, cols));
            layout = layoutGrid(images, cols, rows, gap, cellFit);
          } else if (mode === 'custom') {
            layout = layoutGrid(
              images,
              options.cols || 2,
              options.rows || 2,
              gap,
              cellFit
            );
          } else {
            layout = layoutHorizontal(images, gap, align);
          }

          var w = clampSide(layout.width);
          var h = clampSide(layout.height);
          canvas.width = w;
          canvas.height = h;
          var ctx = canvas.getContext('2d');
          ctx.clearRect(0, 0, w, h);
          if (bg) {
            ctx.fillStyle = bg;
            ctx.fillRect(0, 0, w, h);
          }

          ctx.imageSmoothingEnabled = true;
          if (typeof ctx.imageSmoothingQuality !== 'undefined') {
            ctx.imageSmoothingQuality = 'high';
          }

          for (var i = 0; i < layout.positions.length; i++) {
            var p = layout.positions[i];
            if (p.mode === 'contain') {
              drawContained(ctx, p.img, p.x, p.y, p.w, p.h);
            } else {
              drawFilled(ctx, p.img, p.x, p.y, p.w, p.h);
            }
          }

          return { ok: true, width: w, height: h };
        })
        .catch(function (err) {
          return { ok: false, error: err && err.message ? err.message : 'merge failed' };
        });
    },

    download: function (canvasId, fileName, mimeType, quality) {
      var canvas = getCanvas(canvasId);
      if (!canvas || canvas.width < 1) {
        return { ok: false };
      }
      mimeType = mimeType || 'image/png';
      var q = typeof quality === 'number' ? quality : 0.92;
      var dataUrl =
        mimeType === 'image/jpeg' || mimeType === 'image/webp'
          ? canvas.toDataURL(mimeType, q)
          : canvas.toDataURL('image/png');
      var a = document.createElement('a');
      a.href = dataUrl;
      a.download = fileName;
      a.rel = 'noopener';
      a.click();
      return { ok: true };
    },

    reset: function (canvasId) {
      var canvas = getCanvas(canvasId);
      if (canvas) {
        var ctx = canvas.getContext('2d');
        ctx.clearRect(0, 0, canvas.width, canvas.height);
        canvas.width = 0;
        canvas.height = 0;
      }
    },

    /**
     * @param {string} canvasId
     * @param {string} dataUrl
     * @returns {Promise<{ ok: boolean, width?: number, height?: number, error?: string }>}
     */
    loadToCanvas: function (canvasId, dataUrl) {
      return new Promise(function (resolve, reject) {
        var canvas = getCanvas(canvasId);
        if (!canvas) {
          resolve({ ok: false, error: 'canvas not found' });
          return;
        }
        var img = new Image();
        img.onload = function () {
          var w = img.naturalWidth;
          var h = img.naturalHeight;
          canvas.width = w;
          canvas.height = h;
          var ctx = canvas.getContext('2d');
          ctx.clearRect(0, 0, w, h);
          ctx.drawImage(img, 0, 0);
          resolve({ ok: true, width: w, height: h });
        };
        img.onerror = function () {
          resolve({ ok: false, error: 'image load failed' });
        };
        img.src = dataUrl;
      });
    },

    hasCanvas: function (canvasId) {
      return getCanvas(canvasId) != null;
    },

    downloadDataUrl: function (fileName, dataUrl) {
      var a = document.createElement('a');
      a.href = dataUrl;
      a.download = fileName;
      a.rel = 'noopener';
      a.click();
      return { ok: true };
    },

    downloadBase64: function (base64, mimeType, fileName) {
      var mime = mimeType || 'image/png';
      var a = document.createElement('a');
      a.href = 'data:' + mime + ';base64,' + base64;
      a.download = fileName;
      a.rel = 'noopener';
      a.click();
      return { ok: true };
    }
  };

  var MIN_PREVIEW_SCALE = 0.05;
  var MAX_PREVIEW_SCALE = 8;
  var WHEEL_ZOOM_FACTOR = 1.12;

  /** @type {Record<string, object>} */
  var previewViewports = Object.create(null);

  function getViewportEl(viewportId) {
    var el = document.getElementById(viewportId);
    if (!el) {
      return null;
    }
    return el;
  }

  function getPreviewState(viewportId) {
    return previewViewports[viewportId] || null;
  }

  function updateZoomLabel(vp) {
    if (vp.zoomLabel) {
      vp.zoomLabel.textContent = Math.round(vp.scale * 100) + '%';
    }
  }

  function applyPreviewTransform(vp) {
    if (!vp.stage) {
      return;
    }
    vp.stage.style.transform =
      'translate(' + vp.panX + 'px,' + vp.panY + 'px) scale(' + vp.scale + ')';
    updateZoomLabel(vp);
  }

  function clampScale(scale) {
    return Math.max(MIN_PREVIEW_SCALE, Math.min(MAX_PREVIEW_SCALE, scale));
  }

  function zoomAtClientPoint(vp, newScale, clientX, clientY) {
    var rect = vp.el.getBoundingClientRect();
    var ax = clientX - rect.left;
    var ay = clientY - rect.top;
    var ratio = newScale / vp.scale;
    vp.panX = ax - (ax - vp.panX) * ratio;
    vp.panY = ay - (ay - vp.panY) * ratio;
    vp.scale = newScale;
    applyPreviewTransform(vp);
  }

  function fitPreview(viewportId, canvasId) {
    var vp = getPreviewState(viewportId);
    var canvas = getCanvas(canvasId);
    if (!vp || !canvas || canvas.width < 1 || canvas.height < 1) {
      return;
    }

    var vw = vp.el.clientWidth;
    var vh = vp.el.clientHeight;
    if (vw < 1 || vh < 1) {
      return;
    }

    var scale = Math.min(vw / canvas.width, vh / canvas.height);
    vp.scale = clampScale(scale);
    vp.panX = (vw - canvas.width * vp.scale) / 2;
    vp.panY = (vh - canvas.height * vp.scale) / 2;
    applyPreviewTransform(vp);
  }

  window.imageMergePreview = {
    init: function (viewportId, canvasId, zoomLabelId) {
      var el = getViewportEl(viewportId);
      if (!el || previewViewports[viewportId]) {
        return;
      }

      var vp = {
        el: el,
        canvasId: canvasId,
        stage: el.querySelector('.tool-merge-stage'),
        zoomLabel: zoomLabelId ? document.getElementById(zoomLabelId) : null,
        scale: 1,
        panX: 0,
        panY: 0,
        panning: false,
        panStartX: 0,
        panStartY: 0,
        panOriginX: 0,
        panOriginY: 0
      };
      previewViewports[viewportId] = vp;

      el.addEventListener(
        'wheel',
        function (e) {
          if (!getCanvas(canvasId) || getCanvas(canvasId).width < 1) {
            return;
          }
          e.preventDefault();
          var factor = e.deltaY > 0 ? 1 / WHEEL_ZOOM_FACTOR : WHEEL_ZOOM_FACTOR;
          var next = clampScale(vp.scale * factor);
          zoomAtClientPoint(vp, next, e.clientX, e.clientY);
        },
        { passive: false }
      );

      el.addEventListener('pointerdown', function (e) {
        var canvas = getCanvas(canvasId);
        if (!canvas || canvas.width < 1 || e.button !== 0) {
          return;
        }
        vp.panning = true;
        vp.panStartX = e.clientX;
        vp.panStartY = e.clientY;
        vp.panOriginX = vp.panX;
        vp.panOriginY = vp.panY;
        el.classList.add('is-panning');
        if (typeof el.setPointerCapture === 'function') {
          el.setPointerCapture(e.pointerId);
        }
      });

      el.addEventListener('pointermove', function (e) {
        if (!vp.panning) {
          return;
        }
        vp.panX = vp.panOriginX + (e.clientX - vp.panStartX);
        vp.panY = vp.panOriginY + (e.clientY - vp.panStartY);
        applyPreviewTransform(vp);
      });

      function endPan(e) {
        if (!vp.panning) {
          return;
        }
        vp.panning = false;
        el.classList.remove('is-panning');
        if (typeof el.releasePointerCapture === 'function' && el.hasPointerCapture(e.pointerId)) {
          el.releasePointerCapture(e.pointerId);
        }
      }

      el.addEventListener('pointerup', endPan);
      el.addEventListener('pointercancel', endPan);
      el.addEventListener('dblclick', function () {
        fitPreview(viewportId, canvasId);
      });
    },

    onCanvasUpdated: function (viewportId, canvasId, autoFit) {
      if (autoFit) {
        fitPreview(viewportId, canvasId);
      } else {
        var vp = getPreviewState(viewportId);
        if (vp) {
          updateZoomLabel(vp);
        }
      }
    },

    fit: function (viewportId) {
      var vp = getPreviewState(viewportId);
      if (!vp) {
        return;
      }
      fitPreview(viewportId, vp.canvasId);
    },

    zoomIn: function (viewportId) {
      var vp = getPreviewState(viewportId);
      if (!vp) {
        return;
      }
      var rect = vp.el.getBoundingClientRect();
      zoomAtClientPoint(
        vp,
        clampScale(vp.scale * WHEEL_ZOOM_FACTOR),
        rect.left + rect.width / 2,
        rect.top + rect.height / 2
      );
    },

    zoomOut: function (viewportId) {
      var vp = getPreviewState(viewportId);
      if (!vp) {
        return;
      }
      var rect = vp.el.getBoundingClientRect();
      zoomAtClientPoint(
        vp,
        clampScale(vp.scale / WHEEL_ZOOM_FACTOR),
        rect.left + rect.width / 2,
        rect.top + rect.height / 2
      );
    },

    resetViewport: function (viewportId) {
      var vp = getPreviewState(viewportId);
      if (!vp) {
        return;
      }
      vp.scale = 1;
      vp.panX = 0;
      vp.panY = 0;
      applyPreviewTransform(vp);
    }
  };
})();
