(function () {
  /**
   * @imgly/background-removal 仅接受 png/jpeg/webp 等，不支持 image/avif。
   * 用浏览器解码后栅格化为 PNG 再传入。
   */
  function aiInputNeedsPngRasterize(contentType) {
    if (!contentType) {
      return true;
    }
    var ct = contentType.toLowerCase();
    if (ct.indexOf('avif') >= 0 || ct.indexOf('heif') >= 0 || ct.indexOf('heic') >= 0) {
      return true;
    }
    return !/^image\/(png|jpe?g|pjpeg|webp)$/i.test(ct);
  }

  function imageBlobToPngBlob(blob) {
    return new Promise(function (resolve, reject) {
      createImageBitmap(blob)
        .then(function (bmp) {
          var maxSide = 4096;
          var w = bmp.width;
          var h = bmp.height;
          var scale = Math.min(1, maxSide / Math.max(w, h));
          w = Math.floor(w * scale);
          h = Math.floor(h * scale);
          var c = document.createElement('canvas');
          c.width = w;
          c.height = h;
          c.getContext('2d').drawImage(bmp, 0, 0, w, h);
          bmp.close();
          c.toBlob(function (out) {
            if (!out) {
              reject(new Error('无法转为 PNG'));
              return;
            }
            resolve(out);
          }, 'image/png');
        })
        .catch(function () {
          reject(new Error('浏览器无法解码该图片，请换为 JPG、PNG 或 WebP。'));
        });
    });
  }

  function borderAverage(d, w, h) {
    const seen = new Set();
    let r = 0;
    let g = 0;
    let b = 0;
    let c = 0;
    function addPixel(x, y) {
      const k = y * w + x;
      if (seen.has(k)) {
        return;
      }
      seen.add(k);
      const i = k * 4;
      r += d[i];
      g += d[i + 1];
      b += d[i + 2];
      c++;
    }
    for (let x = 0; x < w; x++) {
      addPixel(x, 0);
      addPixel(x, h - 1);
    }
    for (let y = 0; y < h; y++) {
      addPixel(0, y);
      addPixel(w - 1, y);
    }
    return { r: r / c, g: g / c, b: b / c };
  }

  function colorDist(di, d, br, bg, bb) {
    const dr = d[di] - br;
    const dg = d[di + 1] - bg;
    const db = d[di + 2] - bb;
    return Math.sqrt(dr * dr + dg * dg + db * db);
  }

  function parseHex(hex) {
    if (!hex || typeof hex !== 'string') {
      return { r: 255, g: 255, b: 255 };
    }
    let h = hex.trim();
    if (h[0] === '#') {
      h = h.slice(1);
    }
    if (h.length === 3) {
      h = h[0] + h[0] + h[1] + h[1] + h[2] + h[2];
    }
    if (h.length !== 6) {
      return { r: 255, g: 255, b: 255 };
    }
    return {
      r: parseInt(h.slice(0, 2), 16),
      g: parseInt(h.slice(2, 4), 16),
      b: parseInt(h.slice(4, 6), 16)
    };
  }

  function drawImageToCanvasMax4096(canvas, img) {
    const maxSide = 4096;
    let rw = img.naturalWidth;
    let rh = img.naturalHeight;
    const scale = Math.min(1, maxSide / Math.max(rw, rh));
    rw = Math.floor(rw * scale);
    rh = Math.floor(rh * scale);
    canvas.width = rw;
    canvas.height = rh;
    const ctx = canvas.getContext('2d');
    ctx.drawImage(img, 0, 0, rw, rh);
    return { width: rw, height: rh };
  }

  window.imageBackgroundRemove = {
    /**
     * @param {number} [logicalW] 原始逻辑尺寸（展示用）；不传则返回画布栅格宽高
     * @param {number} [logicalH]
     */
    loadDataUrl: function (canvasId, dataUrl, logicalW, logicalH) {
      return new Promise(function (resolve, reject) {
        const canvas = document.getElementById(canvasId);
        if (!canvas) {
          reject(new Error('canvas not found'));
          return;
        }
        const img = new Image();
        img.onload = function () {
          var dim = drawImageToCanvasMax4096(canvas, img);
          var ow =
            typeof logicalW === 'number' && logicalW > 0 ? Math.floor(logicalW) : dim.width;
          var oh =
            typeof logicalH === 'number' && logicalH > 0 ? Math.floor(logicalH) : dim.height;
          resolve({ width: ow, height: oh });
        };
        img.onerror = function () {
          reject(new Error('图片加载失败'));
        };
        img.src = dataUrl;
      });
    },

    /** 完整图替换画布（仍限制长边 4096），减小首帧 Base64 后再加载大图。 */
    upgradeToFullUpload: function (canvasId, fullDataUrl) {
      return new Promise(function (resolve, reject) {
        const canvas = document.getElementById(canvasId);
        if (!canvas) {
          reject(new Error('canvas not found'));
          return;
        }
        const img = new Image();
        img.onload = function () {
          var dim = drawImageToCanvasMax4096(canvas, img);
          resolve(dim);
        };
        img.onerror = function () {
          reject(new Error('图片加载失败'));
        };
        img.src = fullDataUrl;
      });
    },

    removeBackground: function (canvasId, tolerance) {
      const canvas = document.getElementById(canvasId);
      if (!canvas) {
        return { ok: false, error: 'no canvas' };
      }
      const w = canvas.width;
      const h = canvas.height;
      if (w < 2 || h < 2) {
        return { ok: false, error: 'too small' };
      }
      const ctx = canvas.getContext('2d');
      const imageData = ctx.getImageData(0, 0, w, h);
      const d = imageData.data;
      const avg = borderAverage(d, w, h);
      const t = Math.max(1, Math.min(100, tolerance));
      const maxDist = 6 + t * 1.15;

      const visited = new Uint8Array(w * h);
      const queue = [];
      let qh = 0;

      function trySeed(x, y) {
        if (x < 0 || y < 0 || x >= w || y >= h) {
          return;
        }
        const i = y * w + x;
        if (visited[i]) {
          return;
        }
        const di = i * 4;
        if (colorDist(di, d, avg.r, avg.g, avg.b) > maxDist) {
          return;
        }
        visited[i] = 1;
        queue.push(i);
      }

      for (let x = 0; x < w; x++) {
        trySeed(x, 0);
        trySeed(x, h - 1);
      }
      for (let y = 0; y < h; y++) {
        trySeed(0, y);
        trySeed(w - 1, y);
      }

      while (qh < queue.length) {
        const i = queue[qh++];
        const x = i % w;
        const y = (i / w) | 0;
        const di = i * 4;
        d[di + 3] = 0;

        const dirs = [[-1, 0], [1, 0], [0, -1], [0, 1]];
        for (let k = 0; k < 4; k++) {
          const nx = x + dirs[k][0];
          const ny = y + dirs[k][1];
          if (nx < 0 || ny < 0 || nx >= w || ny >= h) {
            continue;
          }
          const ni = ny * w + nx;
          if (visited[ni]) {
            continue;
          }
          const d2 = ni * 4;
          if (colorDist(d2, d, avg.r, avg.g, avg.b) > maxDist) {
            continue;
          }
          visited[ni] = 1;
          queue.push(ni);
        }
      }

      ctx.putImageData(imageData, 0, 0);
      return { ok: true };
    },

    removeByColor: function (canvasId, hexColor, tolerance, softBand) {
      const canvas = document.getElementById(canvasId);
      if (!canvas) {
        return { ok: false, error: 'no canvas' };
      }
      const w = canvas.width;
      const h = canvas.height;
      if (w < 2 || h < 2) {
        return { ok: false, error: 'too small' };
      }
      const bg = parseHex(hexColor);
      const ctx = canvas.getContext('2d');
      const imageData = ctx.getImageData(0, 0, w, h);
      const d = imageData.data;
      const t = Math.max(1, Math.min(100, tolerance));
      const maxDist = 5 + t * 1.2;
      const soft = Math.max(0, Math.min(80, softBand));

      for (let i = 0; i < w * h; i++) {
        const di = i * 4;
        const dist = colorDist(di, d, bg.r, bg.g, bg.b);
        if (dist <= maxDist) {
          d[di + 3] = 0;
        } else if (soft > 0 && dist < maxDist + soft) {
          const u = (dist - maxDist) / soft;
          d[di + 3] = Math.round(Math.min(255, d[di + 3]) * Math.min(1, Math.max(0, u)));
        }
      }

      ctx.putImageData(imageData, 0, 0);
      return { ok: true };
    },

    removeBackgroundAi: async function (canvasId, bytes, contentType) {
      const canvas = document.getElementById(canvasId);
      if (!canvas) {
        return { ok: false, error: 'no canvas' };
      }
      const u8 = bytes instanceof Uint8Array ? bytes : new Uint8Array(bytes);
      const ct = contentType || 'image/png';
      var blob = new Blob([u8], { type: ct });
      var file;
      if (aiInputNeedsPngRasterize(ct)) {
        blob = await imageBlobToPngBlob(blob);
        file = new File([blob], 'src.png', { type: 'image/png' });
      } else {
        const isJpeg = /jpe?g|pjpeg/i.test(ct);
        file = new File([blob], isJpeg ? 'src.jpg' : 'src.png', { type: ct });
      }

      // 须使用 esm.sh 等带依赖解析的入口；直连 jsdelivr 的 dist/index.mjs 会因裸模块名（如 ndarray）在浏览器中报错
      const mod = await import(
        'https://esm.sh/@imgly/background-removal@1.5.5?target=es2022'
      );
      const outBlob = await mod.removeBackground(file, {
        model: 'isnet_quint8',
        device: 'cpu',
        output: { format: 'image/png', quality: 0.92 }
      });

      const bmp = await createImageBitmap(outBlob);
      canvas.width = bmp.width;
      canvas.height = bmp.height;
      const ctx = canvas.getContext('2d');
      ctx.clearRect(0, 0, bmp.width, bmp.height);
      ctx.drawImage(bmp, 0, 0);
      bmp.close();
      return { ok: true };
    },

    reset: function (canvasId) {
      const canvas = document.getElementById(canvasId);
      if (canvas) {
        canvas.width = 0;
        canvas.height = 0;
      }
    },

    download: function (canvasId, fileName) {
      const canvas = document.getElementById(canvasId);
      if (!canvas || canvas.width < 1) {
        return;
      }
      const name = fileName || 'no-bg.png';
      canvas.toBlob(
        function (blob) {
          if (!blob) {
            return;
          }
          const url = URL.createObjectURL(blob);
          const a = document.createElement('a');
          a.href = url;
          a.download = name;
          a.rel = 'noopener';
          a.click();
          URL.revokeObjectURL(url);
        },
        'image/png'
      );
    }
  };
})();
