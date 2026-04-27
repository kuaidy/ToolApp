(function () {
  'use strict';

  /** @type {Record<string, HTMLImageElement>} */
  const sources = Object.create(null);

  function getCanvas(canvasId) {
    var el = document.getElementById(canvasId);
    if (!el || el.tagName !== 'CANVAS') {
      return null;
    }
    return el;
  }

  window.imageResize = {
    /**
     * @param {string} canvasId
     * @param {string} dataUrl
     * @returns {Promise<{ width: number, height: number }>}
     */
    loadDataUrl: function (canvasId, dataUrl) {
      return new Promise(function (resolve, reject) {
        var canvas = getCanvas(canvasId);
        if (!canvas) {
          reject(new Error('canvas not found'));
          return;
        }
        var img = new Image();
        img.onload = function () {
          sources[canvasId] = img;
          canvas.width = img.naturalWidth;
          canvas.height = img.naturalHeight;
          var ctx = canvas.getContext('2d');
          ctx.clearRect(0, 0, canvas.width, canvas.height);
          ctx.drawImage(img, 0, 0);
          resolve({ width: img.naturalWidth, height: img.naturalHeight });
        };
        img.onerror = function () {
          reject(new Error('image load failed'));
        };
        img.src = dataUrl;
      });
    },

    /**
     * @param {string} canvasId
     * @param {number} targetWidth
     * @param {number} targetHeight
     * @returns {{ ok: boolean, error?: string }}
     */
    applyResize: function (canvasId, targetWidth, targetHeight) {
      var canvas = getCanvas(canvasId);
      var img = sources[canvasId];
      if (!canvas || !img) {
        return { ok: false, error: 'no source' };
      }
      var w = Math.max(1, Math.floor(Number(targetWidth)));
      var h = Math.max(1, Math.floor(Number(targetHeight)));
      var maxSide = 16384;
      w = Math.min(w, maxSide);
      h = Math.min(h, maxSide);
      canvas.width = w;
      canvas.height = h;
      var ctx = canvas.getContext('2d');
      ctx.imageSmoothingEnabled = true;
      if (typeof ctx.imageSmoothingQuality !== 'undefined') {
        ctx.imageSmoothingQuality = 'high';
      }
      ctx.drawImage(img, 0, 0, w, h);
      return { ok: true };
    },

    /** 将画布恢复为原始像素尺寸（仍基于已加载的源图）。 */
    restoreOriginalSize: function (canvasId) {
      var canvas = getCanvas(canvasId);
      var img = sources[canvasId];
      if (!canvas || !img) {
        return { ok: false, error: 'no source' };
      }
      var w = img.naturalWidth;
      var h = img.naturalHeight;
      canvas.width = w;
      canvas.height = h;
      var ctx = canvas.getContext('2d');
      ctx.drawImage(img, 0, 0);
      return { ok: true, width: w, height: h };
    },

    /**
     * @param {string} canvasId
     * @param {string} fileName
     * @param {string} mimeType image/png | image/jpeg | image/webp
     * @param {number} [quality] 0–1，用于 jpeg/webp
     */
    download: function (canvasId, fileName, mimeType, quality) {
      var canvas = getCanvas(canvasId);
      if (!canvas) {
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
      delete sources[canvasId];
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
