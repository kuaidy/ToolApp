(function () {
  'use strict';

  var MAX_FILE_SIZE = 20 * 1024 * 1024;
  var MIN_BLOCK = 2;
  var MAX_BLOCK = 128;

  /** @type {Record<string, HTMLImageElement>} */
  var originals = Object.create(null);

  function getCanvas(canvasId) {
    var el = document.getElementById(canvasId);
    if (!el || el.tagName !== 'CANVAS') return null;
    return el;
  }

  function loadImage(dataUrl) {
    return new Promise(function (resolve, reject) {
      var img = new Image();
      img.onload = function () { resolve(img); };
      img.onerror = function () { reject(new Error('image load failed')); };
      img.src = dataUrl;
    });
  }

  function clamp(n, min, max) {
    return Math.max(min, Math.min(max, n));
  }

  function drawOriginal(canvas, img) {
    canvas.width = img.naturalWidth;
    canvas.height = img.naturalHeight;
    var ctx = canvas.getContext('2d');
    ctx.clearRect(0, 0, canvas.width, canvas.height);
    ctx.drawImage(img, 0, 0);
  }

  /**
   * Classic mosaic: shrink with averaging (default canvas filter), then enlarge
   * with nearest-neighbor so each cell is a flat block.
   */
  function applyPixelate(canvas, img, blockSize) {
    var w = img.naturalWidth;
    var h = img.naturalHeight;
    blockSize = clamp(Math.round(blockSize) || 12, MIN_BLOCK, MAX_BLOCK);

    var smallW = Math.max(1, Math.round(w / blockSize));
    var smallH = Math.max(1, Math.round(h / blockSize));

    var small = document.createElement('canvas');
    small.width = smallW;
    small.height = smallH;
    var sctx = small.getContext('2d');
    sctx.imageSmoothingEnabled = true;
    sctx.drawImage(img, 0, 0, smallW, smallH);

    canvas.width = w;
    canvas.height = h;
    var ctx = canvas.getContext('2d');
    ctx.clearRect(0, 0, w, h);
    ctx.imageSmoothingEnabled = false;
    ctx.drawImage(small, 0, 0, smallW, smallH, 0, 0, w, h);

    return {
      ok: true,
      width: w,
      height: h,
      blockSize: blockSize,
      gridW: smallW,
      gridH: smallH
    };
  }

  window.imagePixelate = {
    loadFromInput: function (inputEl, canvasId) {
      return new Promise(function (resolve, reject) {
        var canvas = getCanvas(canvasId);
        if (!canvas) {
          reject(new Error('canvas not found'));
          return;
        }
        var file = inputEl && inputEl.files && inputEl.files[0];
        if (!file) {
          reject(new Error('no file'));
          return;
        }
        if (file.size > MAX_FILE_SIZE) {
          reject(new Error('file too large'));
          return;
        }
        var reader = new FileReader();
        reader.onload = function () {
          loadImage(reader.result)
            .then(function (img) {
              originals[canvasId] = img;
              drawOriginal(canvas, img);
              resolve({
                width: img.naturalWidth,
                height: img.naturalHeight,
                mime: file.type || 'image/png',
                size: file.size
              });
            })
            .catch(reject);
        };
        reader.onerror = function () { reject(new Error('read failed')); };
        reader.readAsDataURL(file);
      });
    },

    clearInput: function (inputEl) {
      if (inputEl) inputEl.value = '';
    },

    pixelate: function (canvasId, options) {
      var canvas = getCanvas(canvasId);
      var img = originals[canvasId];
      if (!canvas || !img) {
        return { ok: false, error: 'no image' };
      }
      options = options || {};
      try {
        return applyPixelate(canvas, img, options.blockSize);
      } catch (e) {
        return { ok: false, error: e && e.message ? e.message : 'pixelate failed' };
      }
    },

    restoreOriginal: function (canvasId) {
      var canvas = getCanvas(canvasId);
      var img = originals[canvasId];
      if (!canvas || !img) return { ok: false };
      drawOriginal(canvas, img);
      return { ok: true, width: img.naturalWidth, height: img.naturalHeight };
    },

    download: function (canvasId, fileName) {
      var canvas = getCanvas(canvasId);
      if (!canvas || canvas.width < 1) return { ok: false };
      try {
        var a = document.createElement('a');
        a.href = canvas.toDataURL('image/png');
        a.download = fileName || 'pixelate.png';
        a.rel = 'noopener';
        a.click();
        return { ok: true };
      } catch (e) {
        return { ok: false, error: e && e.message ? e.message : 'download failed' };
      }
    },

    reset: function (canvasId) {
      delete originals[canvasId];
      var canvas = getCanvas(canvasId);
      if (canvas) {
        var ctx = canvas.getContext('2d');
        ctx.clearRect(0, 0, canvas.width, canvas.height);
        canvas.width = 0;
        canvas.height = 0;
      }
    }
  };
})();
