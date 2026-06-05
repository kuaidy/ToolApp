(function () {
  const drawPadRegistry = {};

  window.signatureCanvas = {
  draw: async function (canvasId, text, fontFamily, fontSize, textColor, bgTransparent, bgColor) {
    const canvas = document.getElementById(canvasId);
    if (!canvas) {
      return { ok: false, error: 'canvas not found' };
    }
    const t = (text || '').trim();
    if (!t) {
      return { ok: false, error: 'empty' };
    }

    await document.fonts.ready;
    try {
      await document.fonts.load(`${fontSize}px "${fontFamily}"`);
    } catch (_) { /* ignore */ }

    const padding = 48;
    const measure = document.createElement('canvas').getContext('2d');
    measure.font = `${fontSize}px "${fontFamily}", cursive, serif`;
    const metrics = measure.measureText(t);
    const textWidth = Math.max(metrics.width, fontSize * 2);
    const textHeight = fontSize * 1.5;

    canvas.width = Math.ceil(textWidth + padding * 2);
    canvas.height = Math.ceil(textHeight + padding * 2);

    const ctx = canvas.getContext('2d');
    if (bgTransparent) {
      ctx.clearRect(0, 0, canvas.width, canvas.height);
    } else {
      ctx.fillStyle = bgColor || '#ffffff';
      ctx.fillRect(0, 0, canvas.width, canvas.height);
    }

    ctx.fillStyle = textColor || '#1a1a1a';
    ctx.font = `${fontSize}px "${fontFamily}", cursive, serif`;
    ctx.textBaseline = 'middle';
    ctx.textAlign = 'center';
    ctx.fillText(t, canvas.width / 2, canvas.height / 2);

    return { ok: true, width: canvas.width, height: canvas.height };
  },

  download: function (canvasId, fileName) {
    const canvas = document.getElementById(canvasId);
    if (!canvas) {
      return;
    }
    const name = fileName || 'signature.png';
    const url = canvas.toDataURL('image/png');
    const a = document.createElement('a');
    a.href = url;
    a.download = name;
    a.rel = 'noopener';
    a.click();
  },

  initDrawPad: function (canvasId, width, height, transparent, bgColor) {
    const canvas = document.getElementById(canvasId);
    if (!canvas) {
      return { ok: false };
    }

    function fitCanvasBitmap(refWidth, refHeight) {
      canvas.style.width = '100%';
      canvas.style.display = 'block';
      canvas.style.aspectRatio = refWidth + ' / ' + refHeight;
      const parent = canvas.parentElement;
      const rect = canvas.getBoundingClientRect();
      const cssWidth = rect.width > 0
        ? rect.width
        : (parent && parent.clientWidth > 0 ? parent.clientWidth : refWidth);
      const cssHeight = rect.height > 0
        ? rect.height
        : Math.max(100, Math.round(cssWidth * (refHeight / refWidth)));
      const dpr = window.devicePixelRatio || 1;
      const bitmapW = Math.max(1, Math.floor(cssWidth * dpr));
      const bitmapH = Math.max(1, Math.floor(cssHeight * dpr));
      canvas.width = bitmapW;
      canvas.height = bitmapH;
      return { bitmapW: bitmapW, bitmapH: bitmapH };
    }

    const ctx = canvas.getContext('2d');
    const size = fitCanvasBitmap(width, height);

    if (transparent) {
      ctx.clearRect(0, 0, size.bitmapW, size.bitmapH);
    } else {
      ctx.fillStyle = bgColor || '#ffffff';
      ctx.fillRect(0, 0, size.bitmapW, size.bitmapH);
    }

    if (drawPadRegistry[canvasId]) {
      drawPadRegistry[canvasId].cleanup();
      delete drawPadRegistry[canvasId];
    }

    const state = {
      color: '#1a1a1a',
      lineWidth: 4,
      hasInk: false,
      drawing: false,
      lastX: 0,
      lastY: 0
    };

    function getPos(e) {
      const rect = canvas.getBoundingClientRect();
      const scaleX = canvas.width / rect.width;
      const scaleY = canvas.height / rect.height;
      let clientX;
      let clientY;
      if (e.touches && e.touches.length) {
        clientX = e.touches[0].clientX;
        clientY = e.touches[0].clientY;
      } else {
        clientX = e.clientX;
        clientY = e.clientY;
      }
      return {
        x: (clientX - rect.left) * scaleX,
        y: (clientY - rect.top) * scaleY
      };
    }

    function start(e) {
      if (e.cancelable) {
        e.preventDefault();
      }
      state.drawing = true;
      const p = getPos(e);
      state.lastX = p.x;
      state.lastY = p.y;
      ctx.beginPath();
      ctx.fillStyle = state.color;
      ctx.arc(p.x, p.y, Math.max(state.lineWidth / 2, 1), 0, Math.PI * 2);
      ctx.fill();
      state.hasInk = true;
    }

    function move(e) {
      if (!state.drawing) {
        return;
      }
      if (e.cancelable) {
        e.preventDefault();
      }
      const p = getPos(e);
      ctx.beginPath();
      ctx.moveTo(state.lastX, state.lastY);
      ctx.lineTo(p.x, p.y);
      ctx.strokeStyle = state.color;
      ctx.lineWidth = state.lineWidth;
      ctx.lineCap = 'round';
      ctx.lineJoin = 'round';
      ctx.stroke();
      state.lastX = p.x;
      state.lastY = p.y;
      state.hasInk = true;
    }

    function end() {
      state.drawing = false;
    }

    canvas.style.touchAction = 'none';

    let resizeTimer = null;
    function onResize() {
      if (resizeTimer) {
        clearTimeout(resizeTimer);
      }
      resizeTimer = setTimeout(function () {
        const hadInk = state.hasInk;
        let snapshot = null;
        if (hadInk) {
          try {
            snapshot = ctx.getImageData(0, 0, canvas.width, canvas.height);
          } catch (_) { /* ignore */ }
        }
        const prevW = canvas.width;
        const prevH = canvas.height;
        const next = fitCanvasBitmap(width, height);
        if (transparent) {
          ctx.clearRect(0, 0, next.bitmapW, next.bitmapH);
        } else {
          ctx.fillStyle = bgColor || '#ffffff';
          ctx.fillRect(0, 0, next.bitmapW, next.bitmapH);
        }
        if (snapshot && hadInk) {
          const off = document.createElement('canvas');
          off.width = prevW;
          off.height = prevH;
          off.getContext('2d').putImageData(snapshot, 0, 0);
          ctx.drawImage(off, 0, 0, next.bitmapW, next.bitmapH);
        }
      }, 150);
    }

    window.addEventListener('resize', onResize);

    canvas.addEventListener('mousedown', start);
    canvas.addEventListener('mousemove', move);
    canvas.addEventListener('mouseup', end);
    canvas.addEventListener('mouseleave', end);
    canvas.addEventListener('touchstart', start, { passive: false });
    canvas.addEventListener('touchmove', move, { passive: false });
    canvas.addEventListener('touchend', end);
    canvas.addEventListener('touchcancel', end);

    drawPadRegistry[canvasId] = {
      state: state,
      setStroke: function (color, lineWidth) {
        if (color) {
          state.color = color;
        }
        if (lineWidth != null && lineWidth > 0) {
          state.lineWidth = lineWidth;
        }
      },
      clear: function (transparent, bg) {
        if (transparent) {
          ctx.clearRect(0, 0, canvas.width, canvas.height);
        } else {
          ctx.fillStyle = bg || '#ffffff';
          ctx.fillRect(0, 0, canvas.width, canvas.height);
        }
        state.hasInk = false;
      },
      hasInk: function () {
        return state.hasInk;
      },
      cleanup: function () {
        if (resizeTimer) {
          clearTimeout(resizeTimer);
        }
        window.removeEventListener('resize', onResize);
        canvas.removeEventListener('mousedown', start);
        canvas.removeEventListener('mousemove', move);
        canvas.removeEventListener('mouseup', end);
        canvas.removeEventListener('mouseleave', end);
        canvas.removeEventListener('touchstart', start);
        canvas.removeEventListener('touchmove', move);
        canvas.removeEventListener('touchend', end);
        canvas.removeEventListener('touchcancel', end);
      }
    };

    return { ok: true };
  },

  setDrawStroke: function (canvasId, color, lineWidth) {
    const pad = drawPadRegistry[canvasId];
    if (pad) {
      pad.setStroke(color, lineWidth);
    }
  },

  clearDrawPad: function (canvasId, transparent, bgColor) {
    const pad = drawPadRegistry[canvasId];
    if (pad) {
      pad.clear(transparent, bgColor);
    }
  },

  drawPadHasInk: function (canvasId) {
    const pad = drawPadRegistry[canvasId];
    return pad ? pad.hasInk() : false;
  }
  };
})();
