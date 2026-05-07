(function () {
  'use strict';

  /** @type {Record<string, HTMLImageElement>} */
  const sources = Object.create(null);
  /** 用户上传的原始图（首次 loadDataUrl 时固定），用于恢复与预览链。 */
  const originals = Object.create(null);

  function getCanvas(canvasId) {
    var el = document.getElementById(canvasId);
    if (!el || el.tagName !== 'CANVAS') {
      return null;
    }
    return el;
  }

  window.imageCompress = {
    /**
     * @param {string} canvasId
     * @param {string} dataUrl
     * @returns {Promise<{ width: number, height: number }>}
     */
    loadDataUrl: function (canvasId, dataUrl, logicalW, logicalH) {
      return new Promise(function (resolve, reject) {
        var canvas = getCanvas(canvasId);
        if (!canvas) {
          reject(new Error('canvas not found'));
          return;
        }
        var img = new Image();
        img.onload = function () {
          if (!originals[canvasId]) {
            originals[canvasId] = img;
          }
          sources[canvasId] = img;
          var w = img.naturalWidth;
          var h = img.naturalHeight;
          canvas.width = w;
          canvas.height = h;
          var ctx = canvas.getContext('2d');
          ctx.clearRect(0, 0, w, h);
          ctx.drawImage(img, 0, 0);
          var ow =
            typeof logicalW === 'number' && logicalW > 0 ? Math.floor(logicalW) : w;
          var oh =
            typeof logicalH === 'number' && logicalH > 0 ? Math.floor(logicalH) : h;
          resolve({ width: ow, height: oh });
        };
        img.onerror = function () {
          reject(new Error('image load failed'));
        };
        img.src = dataUrl;
      });
    },

    upgradeToFullUpload: function (canvasId, fullDataUrl) {
      return new Promise(function (resolve, reject) {
        var canvas = getCanvas(canvasId);
        if (!canvas) {
          reject(new Error('canvas not found'));
          return;
        }
        var img = new Image();
        img.onload = function () {
          originals[canvasId] = img;
          sources[canvasId] = img;
          var w = img.naturalWidth;
          var h = img.naturalHeight;
          canvas.width = w;
          canvas.height = h;
          var ctx = canvas.getContext('2d');
          ctx.clearRect(0, 0, w, h);
          ctx.drawImage(img, 0, 0);
          resolve({ width: w, height: h });
        };
        img.onerror = function () {
          reject(new Error('image load failed'));
        };
        img.src = fullDataUrl;
      });
    },

    upgradeCanvasPreview: function (canvasId, fullDataUrl) {
      return new Promise(function (resolve, reject) {
        var canvas = getCanvas(canvasId);
        if (!canvas) {
          reject(new Error('canvas not found'));
          return;
        }
        var img = new Image();
        img.onload = function () {
          sources[canvasId] = img;
          var w = img.naturalWidth;
          var h = img.naturalHeight;
          canvas.width = w;
          canvas.height = h;
          var ctx = canvas.getContext('2d');
          ctx.clearRect(0, 0, w, h);
          ctx.drawImage(img, 0, 0);
          resolve({ width: w, height: h });
        };
        img.onerror = function () {
          reject(new Error('image load failed'));
        };
        img.src = fullDataUrl;
      });
    },

    /**
     * 从内存中的原图重绘到画布（预览原图）。
     * @returns {{ ok: boolean, width?: number, height?: number }}
     */
    restoreOriginal: function (canvasId) {
      var canvas = getCanvas(canvasId);
      var img = originals[canvasId] || sources[canvasId];
      if (!canvas || !img) {
        return { ok: false };
      }
      var w = img.naturalWidth;
      var h = img.naturalHeight;
      canvas.width = w;
      canvas.height = h;
      var ctx = canvas.getContext('2d');
      ctx.clearRect(0, 0, w, h);
      ctx.drawImage(img, 0, 0);
      return { ok: true, width: w, height: h };
    },

    downloadBase64: function (base64, mimeType, fileName) {
      var mime = mimeType || 'image/png';
      var a = document.createElement('a');
      a.href = 'data:' + mime + ';base64,' + base64;
      a.download = fileName;
      a.rel = 'noopener';
      a.click();
      return { ok: true };
    },

    reset: function (canvasId) {
      delete sources[canvasId];
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
