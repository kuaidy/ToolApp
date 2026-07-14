(function () {
  'use strict';

  var ICO_MIME = 'image/x-icon';
  var ICON_SIZES = [16, 32, 48, 64, 128, 256];
  var LOSSY_QUALITY = 0.92;
  var MAX_FILE_SIZE = 20 * 1024 * 1024;

  /** @type {Record<string, HTMLImageElement>} */
  var originals = Object.create(null);
  /** @type {Record<string, { blob: Blob, mime: string }>} */
  var converted = Object.create(null);

  function getCanvas(canvasId) {
    var el = document.getElementById(canvasId);
    if (!el || el.tagName !== 'CANVAS') {
      return null;
    }
    return el;
  }

  function isIcoMime(mime) {
    if (!mime) {
      return false;
    }
    var m = mime.toLowerCase();
    return m === ICO_MIME || m === 'image/vnd.microsoft.icon';
  }

  function isLossyMime(mime) {
    if (!mime) {
      return false;
    }
    var m = mime.toLowerCase();
    return m.indexOf('jpeg') >= 0 || m.indexOf('webp') >= 0 || m.indexOf('bmp') >= 0;
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

  function drawSource(canvas, img, fillWhite) {
    var w = img.naturalWidth;
    var h = img.naturalHeight;
    canvas.width = w;
    canvas.height = h;
    var ctx = canvas.getContext('2d');
    ctx.clearRect(0, 0, w, h);
    if (fillWhite) {
      ctx.fillStyle = '#ffffff';
      ctx.fillRect(0, 0, w, h);
    }
    ctx.drawImage(img, 0, 0);
  }

  function createSquareFrame(img, size) {
    var canvas = document.createElement('canvas');
    canvas.width = size;
    canvas.height = size;
    var ctx = canvas.getContext('2d');
    var iw = img.naturalWidth;
    var ih = img.naturalHeight;
    var scale = Math.min(size / iw, size / ih);
    var dw = Math.max(1, Math.round(iw * scale));
    var dh = Math.max(1, Math.round(ih * scale));
    var dx = Math.floor((size - dw) / 2);
    var dy = Math.floor((size - dh) / 2);
    ctx.clearRect(0, 0, size, size);
    ctx.drawImage(img, 0, 0, iw, ih, dx, dy, dw, dh);
    return canvas;
  }

  function canvasToBlob(canvas, mime, quality) {
    return new Promise(function (resolve, reject) {
      canvas.toBlob(
        function (blob) {
          if (blob) {
            resolve(blob);
          } else {
            reject(new Error('encode failed'));
          }
        },
        mime,
        quality
      );
    });
  }

  function encodeBmp24(canvas) {
    var w = canvas.width;
    var h = canvas.height;
    var ctx = canvas.getContext('2d');
    var imageData = ctx.getImageData(0, 0, w, h);
    var src = imageData.data;
    var rowSize = (w * 3 + 3) & ~3;
    var pixelDataSize = rowSize * h;
    var fileSize = 14 + 40 + pixelDataSize;
    var buffer = new ArrayBuffer(fileSize);
    var view = new DataView(buffer);
    var o = 0;

    view.setUint16(o, 0x4d42, true);
    o += 2;
    view.setUint32(o, fileSize, true);
    o += 4;
    view.setUint32(o, 0, true);
    o += 4;
    view.setUint32(o, 54, true);
    o += 4;
    view.setUint32(o, 40, true);
    o += 4;
    view.setInt32(o, w, true);
    o += 4;
    view.setInt32(o, h, true);
    o += 4;
    view.setUint16(o, 1, true);
    o += 2;
    view.setUint16(o, 24, true);
    o += 2;
    view.setUint32(o, 0, true);
    o += 4;
    view.setUint32(o, pixelDataSize, true);
    o += 4;

    var bytes = new Uint8Array(buffer);
    var off = 54;
    for (var y = h - 1; y >= 0; y--) {
      for (var x = 0; x < w; x++) {
        var i = (y * w + x) * 4;
        bytes[off++] = src[i + 2];
        bytes[off++] = src[i + 1];
        bytes[off++] = src[i];
      }
      var pad = rowSize - w * 3;
      for (var p = 0; p < pad; p++) {
        bytes[off++] = 0;
      }
    }

    return new Blob([buffer], { type: 'image/bmp' });
  }

  function writeIcoFile(entries) {
    var headerSize = 6 + 16 * entries.length;
    var totalSize = headerSize;
    for (var i = 0; i < entries.length; i++) {
      totalSize += entries[i].data.byteLength;
    }

    var buffer = new ArrayBuffer(totalSize);
    var view = new DataView(buffer);
    view.setUint16(0, 0, true);
    view.setUint16(2, 1, true);
    view.setUint16(4, entries.length, true);

    var offset = headerSize;
    for (var j = 0; j < entries.length; j++) {
      var entry = entries[j];
      var dirOff = 6 + j * 16;
      var w = entry.width;
      var h = entry.height;
      view.setUint8(dirOff, w >= 256 ? 0 : w);
      view.setUint8(dirOff + 1, h >= 256 ? 0 : h);
      view.setUint8(dirOff + 2, 0);
      view.setUint8(dirOff + 3, 0);
      view.setUint16(dirOff + 4, 1, true);
      view.setUint16(dirOff + 6, 32, true);
      view.setUint32(dirOff + 8, entry.data.byteLength, true);
      view.setUint32(dirOff + 12, offset, true);
      offset += entry.data.byteLength;
    }

    var bytes = new Uint8Array(buffer);
    var writeOff = headerSize;
    for (var k = 0; k < entries.length; k++) {
      bytes.set(new Uint8Array(entries[k].data), writeOff);
      writeOff += entries[k].data.byteLength;
    }

    return new Blob([buffer], { type: ICO_MIME });
  }

  function blobToArrayBuffer(blob) {
    return new Promise(function (resolve, reject) {
      var reader = new FileReader();
      reader.onload = function () {
        resolve(reader.result);
      };
      reader.onerror = function () {
        reject(new Error('read blob failed'));
      };
      reader.readAsArrayBuffer(blob);
    });
  }

  window.imageFormatConvert = {
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
              delete converted[canvasId];
              drawSource(canvas, img, false);
              resolve({
                width: img.naturalWidth,
                height: img.naturalHeight,
                mime: file.type || 'image/png',
                size: file.size
              });
            })
            .catch(reject);
        };
        reader.onerror = function () {
          reject(new Error('read failed'));
        };
        reader.readAsDataURL(file);
      });
    },

    clearInput: function (inputEl) {
      if (inputEl) {
        inputEl.value = '';
      }
    },

    restoreOriginal: function (canvasId) {
      var canvas = getCanvas(canvasId);
      var img = originals[canvasId];
      if (!canvas || !img) {
        return { ok: false };
      }
      delete converted[canvasId];
      drawSource(canvas, img, false);
      return { ok: true, width: img.naturalWidth, height: img.naturalHeight };
    },

    convert: function (canvasId, outputMime, fillWhite) {
      return new Promise(function (resolve, reject) {
        var canvas = getCanvas(canvasId);
        var img = originals[canvasId];
        if (!canvas || !img) {
          resolve({ ok: false, error: 'no source' });
          return;
        }

        var mime = outputMime || 'image/jpeg';
        var useWhite = !!fillWhite && isLossyMime(mime) && !isIcoMime(mime);

        if (isIcoMime(mime)) {
          Promise.all(
            ICON_SIZES.map(function (size) {
              var frame = createSquareFrame(img, size);
              return canvasToBlob(frame, 'image/png').then(function (blob) {
                return blobToArrayBuffer(blob).then(function (data) {
                  return { data: data, width: size, height: size };
                });
              });
            })
          )
            .then(function (entries) {
              var icoBlob = writeIcoFile(entries);
              converted[canvasId] = { blob: icoBlob, mime: ICO_MIME };
              var preview = createSquareFrame(img, 256);
              canvas.width = 256;
              canvas.height = 256;
              var ctx = canvas.getContext('2d');
              ctx.clearRect(0, 0, 256, 256);
              ctx.drawImage(preview, 0, 0);
              resolve({ ok: true, byteLength: icoBlob.size });
            })
            .catch(function (err) {
              resolve({ ok: false, error: err && err.message ? err.message : 'convert failed' });
            });
          return;
        }

        var work = document.createElement('canvas');
        drawSource(work, img, useWhite);

        var blobPromise;
        if (mime.indexOf('png') >= 0) {
          blobPromise = canvasToBlob(work, 'image/png');
        } else if (mime.indexOf('jpeg') >= 0) {
          blobPromise = canvasToBlob(work, 'image/jpeg', LOSSY_QUALITY);
        } else if (mime.indexOf('webp') >= 0) {
          blobPromise = canvasToBlob(work, 'image/webp', LOSSY_QUALITY);
        } else if (mime.indexOf('bmp') >= 0) {
          blobPromise = Promise.resolve(encodeBmp24(work));
        } else {
          resolve({ ok: false, error: 'unsupported format' });
          return;
        }

        blobPromise
          .then(function (blob) {
            converted[canvasId] = { blob: blob, mime: mime };
            canvas.width = work.width;
            canvas.height = work.height;
            var ctx = canvas.getContext('2d');
            ctx.clearRect(0, 0, canvas.width, canvas.height);
            ctx.drawImage(work, 0, 0);
            resolve({ ok: true, byteLength: blob.size });
          })
          .catch(function (err) {
            resolve({ ok: false, error: err && err.message ? err.message : 'convert failed' });
          });
      });
    },

    download: function (canvasId, fileName, mimeType) {
      var item = converted[canvasId];
      if (!item || !item.blob) {
        return { ok: false };
      }
      var url = URL.createObjectURL(item.blob);
      var a = document.createElement('a');
      a.href = url;
      a.download = fileName;
      a.rel = 'noopener';
      a.click();
      setTimeout(function () {
        URL.revokeObjectURL(url);
      }, 1000);
      return { ok: true };
    },

    reset: function (canvasId) {
      delete originals[canvasId];
      delete converted[canvasId];
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
