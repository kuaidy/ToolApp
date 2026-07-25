(function () {
  'use strict';

  var MAX_FILE_SIZE = 20 * 1024 * 1024;
  var MAX_GRID = 200;
  var MIN_GRID = 8;

  /** @type {Record<string, HTMLImageElement>} */
  var originals = Object.create(null);
  /** @type {Record<string, { cols: number, rows: number, colors: Array<{hex:string,count:number,r:number,g:number,b:number}> }>} */
  var lastStats = Object.create(null);

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

  function rgbToHex(r, g, b) {
    function h(v) {
      var s = Math.round(clamp(v, 0, 255)).toString(16);
      return s.length === 1 ? '0' + s : s;
    }
    return '#' + h(r) + h(g) + h(b);
  }

  function colorDist(a, b) {
    var dr = a[0] - b[0];
    var dg = a[1] - b[1];
    var db = a[2] - b[2];
    return dr * dr + dg * dg + db * db;
  }

  /** Simple k-means quantization for palette colors. */
  function quantizeColors(samples, k, iterations) {
    if (!samples.length || k < 1) return [];
    k = Math.min(k, samples.length);
    iterations = iterations || 8;

    var centroids = [];
    var step = Math.max(1, Math.floor(samples.length / k));
    for (var i = 0; i < k; i++) {
      var s = samples[Math.min(i * step, samples.length - 1)];
      centroids.push([s[0], s[1], s[2]]);
    }

    for (var iter = 0; iter < iterations; iter++) {
      var sums = [];
      var counts = [];
      for (var c = 0; c < k; c++) {
        sums.push([0, 0, 0]);
        counts.push(0);
      }
      for (var si = 0; si < samples.length; si++) {
        var p = samples[si];
        var best = 0;
        var bestD = Infinity;
        for (var ci = 0; ci < k; ci++) {
          var d = colorDist(p, centroids[ci]);
          if (d < bestD) {
            bestD = d;
            best = ci;
          }
        }
        sums[best][0] += p[0];
        sums[best][1] += p[1];
        sums[best][2] += p[2];
        counts[best]++;
      }
      for (var cj = 0; cj < k; cj++) {
        if (counts[cj] > 0) {
          centroids[cj][0] = sums[cj][0] / counts[cj];
          centroids[cj][1] = sums[cj][1] / counts[cj];
          centroids[cj][2] = sums[cj][2] / counts[cj];
        }
      }
    }
    return centroids.map(function (c) {
      return [Math.round(c[0]), Math.round(c[1]), Math.round(c[2])];
    });
  }

  function nearestColor(rgb, palette) {
    var best = palette[0];
    var bestD = Infinity;
    for (var i = 0; i < palette.length; i++) {
      var d = colorDist(rgb, palette[i]);
      if (d < bestD) {
        bestD = d;
        best = palette[i];
      }
    }
    return best;
  }

  function sampleGrid(img, cols, rows, alphaThreshold) {
    var tmp = document.createElement('canvas');
    tmp.width = cols;
    tmp.height = rows;
    var tctx = tmp.getContext('2d');
    tctx.imageSmoothingEnabled = true;
    if (typeof tctx.imageSmoothingQuality !== 'undefined') {
      tctx.imageSmoothingQuality = 'high';
    }
    tctx.clearRect(0, 0, cols, rows);
    tctx.drawImage(img, 0, 0, cols, rows);
    var data = tctx.getImageData(0, 0, cols, rows).data;
    var cells = [];
    var opaqueSamples = [];

    for (var y = 0; y < rows; y++) {
      for (var x = 0; x < cols; x++) {
        var i = (y * cols + x) * 4;
        var a = data[i + 3];
        if (a < alphaThreshold) {
          cells.push(null);
        } else {
          var rgb = [data[i], data[i + 1], data[i + 2]];
          cells.push(rgb);
          opaqueSamples.push(rgb);
        }
      }
    }
    return { cells: cells, opaqueSamples: opaqueSamples, cols: cols, rows: rows };
  }

  function drawBeads(canvas, cells, cols, rows, beadSize, showGrid, showHole) {
    var w = cols * beadSize;
    var h = rows * beadSize;
    canvas.width = w;
    canvas.height = h;
    var ctx = canvas.getContext('2d');
    ctx.clearRect(0, 0, w, h);
    ctx.fillStyle = '#f4f4f4';
    ctx.fillRect(0, 0, w, h);

    var radius = beadSize * 0.42;
    var holeR = beadSize * 0.12;

    for (var y = 0; y < rows; y++) {
      for (var x = 0; x < cols; x++) {
        var cell = cells[y * cols + x];
        var cx = x * beadSize + beadSize / 2;
        var cy = y * beadSize + beadSize / 2;
        if (!cell) {
          if (showGrid) {
            ctx.strokeStyle = '#ddd';
            ctx.lineWidth = 1;
            ctx.strokeRect(x * beadSize + 0.5, y * beadSize + 0.5, beadSize - 1, beadSize - 1);
          }
          continue;
        }
        ctx.beginPath();
        ctx.arc(cx, cy, radius, 0, Math.PI * 2);
        ctx.fillStyle = 'rgb(' + cell[0] + ',' + cell[1] + ',' + cell[2] + ')';
        ctx.fill();
        if (showHole) {
          ctx.beginPath();
          ctx.arc(cx, cy, holeR, 0, Math.PI * 2);
          ctx.fillStyle = 'rgba(255,255,255,0.55)';
          ctx.fill();
        }
        if (showGrid) {
          ctx.strokeStyle = 'rgba(0,0,0,0.12)';
          ctx.lineWidth = 1;
          ctx.beginPath();
          ctx.arc(cx, cy, radius, 0, Math.PI * 2);
          ctx.stroke();
        }
      }
    }
  }

  function buildStats(cells) {
    var map = Object.create(null);
    var total = 0;
    for (var i = 0; i < cells.length; i++) {
      var c = cells[i];
      if (!c) continue;
      var hex = rgbToHex(c[0], c[1], c[2]);
      if (!map[hex]) {
        map[hex] = { hex: hex, count: 0, r: c[0], g: c[1], b: c[2] };
      }
      map[hex].count++;
      total++;
    }
    var colors = Object.keys(map).map(function (k) { return map[k]; });
    colors.sort(function (a, b) { return b.count - a.count; });
    return { colors: colors, beadCount: total };
  }

  window.beadPattern = {
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
              delete lastStats[canvasId];
              canvas.width = img.naturalWidth;
              canvas.height = img.naturalHeight;
              var ctx = canvas.getContext('2d');
              ctx.clearRect(0, 0, canvas.width, canvas.height);
              ctx.drawImage(img, 0, 0);
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

    generate: function (canvasId, options) {
      var canvas = getCanvas(canvasId);
      var img = originals[canvasId];
      if (!canvas || !img) {
        return { ok: false, error: 'no source' };
      }

      options = options || {};
      var gridW = clamp(Math.floor(Number(options.gridWidth) || 50), MIN_GRID, MAX_GRID);
      var aspect = img.naturalHeight / Math.max(1, img.naturalWidth);
      var gridH = clamp(Math.round(gridW * aspect), MIN_GRID, MAX_GRID);
      var beadSize = clamp(Math.floor(Number(options.beadSize) || 14), 6, 40);
      var maxColors = Math.floor(Number(options.maxColors) || 0);
      var alphaThreshold = clamp(Math.floor(Number(options.alphaThreshold) || 40), 0, 255);
      var showGrid = options.showGrid !== false;
      var showHole = options.showHole !== false;

      var sampled = sampleGrid(img, gridW, gridH, alphaThreshold);
      var cells = sampled.cells.slice();

      if (maxColors > 0 && sampled.opaqueSamples.length > 0) {
        var palette = quantizeColors(sampled.opaqueSamples, maxColors, 10);
        for (var i = 0; i < cells.length; i++) {
          if (cells[i]) {
            cells[i] = nearestColor(cells[i], palette);
          }
        }
      }

      drawBeads(canvas, cells, gridW, gridH, beadSize, showGrid, showHole);
      var stats = buildStats(cells);
      lastStats[canvasId] = {
        cols: gridW,
        rows: gridH,
        colors: stats.colors,
        beadCount: stats.beadCount
      };

      return {
        ok: true,
        cols: gridW,
        rows: gridH,
        beadCount: stats.beadCount,
        colorCount: stats.colors.length,
        colors: stats.colors.slice(0, 64)
      };
    },

    restoreOriginal: function (canvasId) {
      var canvas = getCanvas(canvasId);
      var img = originals[canvasId];
      if (!canvas || !img) return { ok: false };
      delete lastStats[canvasId];
      canvas.width = img.naturalWidth;
      canvas.height = img.naturalHeight;
      var ctx = canvas.getContext('2d');
      ctx.clearRect(0, 0, canvas.width, canvas.height);
      ctx.drawImage(img, 0, 0);
      return { ok: true, width: img.naturalWidth, height: img.naturalHeight };
    },

    download: function (canvasId, fileName) {
      var canvas = getCanvas(canvasId);
      if (!canvas || canvas.width < 1) return { ok: false };
      try {
        var a = document.createElement('a');
        a.href = canvas.toDataURL('image/png');
        a.download = fileName || 'bead-pattern.png';
        a.rel = 'noopener';
        a.click();
        return { ok: true };
      } catch (e) {
        return { ok: false, error: e && e.message ? e.message : 'download failed' };
      }
    },

    reset: function (canvasId) {
      delete originals[canvasId];
      delete lastStats[canvasId];
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
