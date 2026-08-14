(function () {
  'use strict';

  /**
   * ToolApp original bamboo-cicada simulator.
   * Physics / synth approach follows publicly documented rope-mass + formant ideas
   * (study only). No third-party source, art, or audio samples are redistributed.
   */

  var TAU = Math.PI * 2;
  var ROPE_K = 2600;
  var ROPE_D = 14;
  var GRAV = 1150;
  var AIR_DRAG = 0.35;
  var PHYS_H = 1 / 240;

  /** @type {Record<string, any>} */
  var instances = Object.create(null);

  function clamp(n, a, b) {
    return Math.max(a, Math.min(b, n));
  }

  function softClipCurve(ctx) {
    var n = 256;
    var curve = new Float32Array(n);
    for (var i = 0; i < n; i++) {
      var x = (i / (n - 1)) * 2 - 1;
      curve[i] = Math.tanh(x * 3.2);
    }
    var shaper = ctx.createWaveShaper();
    shaper.curve = curve;
    shaper.oversample = '2x';
    return shaper;
  }

  function makeNoiseBuffer(ctx) {
    var len = ctx.sampleRate * 2;
    var buf = ctx.createBuffer(1, len, ctx.sampleRate);
    var data = buf.getChannelData(0);
    for (var i = 0; i < len; i++) data[i] = Math.random() * 2 - 1;
    return buf;
  }

  function ensureAudio(inst) {
    if (inst.audio) {
      if (inst.audio.ctx.state === 'suspended') inst.audio.ctx.resume();
      return inst.audio;
    }
    var AC = window.AudioContext || window.webkitAudioContext;
    if (!AC) return null;
    var ctx = new AC();
    var master = ctx.createGain();
    master.gain.value = 0;
    var comp = ctx.createDynamicsCompressor();
    comp.threshold.value = -18;
    comp.knee.value = 18;
    comp.ratio.value = 3;
    comp.attack.value = 0.003;
    comp.release.value = 0.12;
    master.connect(comp);
    comp.connect(ctx.destination);

    var osc = ctx.createOscillator();
    osc.type = 'sawtooth';
    osc.frequency.value = 90;
    var shaper = softClipCurve(ctx);
    osc.connect(shaper);

    var am = ctx.createGain();
    am.gain.value = 0.62;
    var lfo = ctx.createOscillator();
    lfo.type = 'sine';
    lfo.frequency.value = 30;
    var lfoAmt = ctx.createGain();
    lfoAmt.gain.value = 0.34;
    lfo.connect(lfoAmt);
    lfoAmt.connect(am.gain);
    shaper.connect(am);

    var noise = ctx.createBufferSource();
    noise.buffer = makeNoiseBuffer(ctx);
    noise.loop = true;
    var nFil = ctx.createBiquadFilter();
    nFil.type = 'bandpass';
    nFil.frequency.value = 2500;
    nFil.Q.value = 0.7;
    var nGain = ctx.createGain();
    nGain.gain.value = 0;
    noise.connect(nFil);
    nFil.connect(nGain);

    var bus = ctx.createGain();
    bus.gain.value = 0.9;
    am.connect(bus);
    nGain.connect(bus);

    var wah = ctx.createBiquadFilter();
    wah.type = 'bandpass';
    wah.frequency.value = 900;
    wah.Q.value = 2.2;
    bus.connect(wah);

    var sum = ctx.createGain();
    sum.gain.value = 1;
    function formant(freq, q, g) {
      var f = ctx.createBiquadFilter();
      f.type = 'bandpass';
      f.frequency.value = freq;
      f.Q.value = q;
      var fg = ctx.createGain();
      fg.gain.value = g;
      wah.connect(f);
      f.connect(fg);
      fg.connect(sum);
    }
    formant(1050, 9, 0.9);
    formant(2150, 11, 0.6);
    formant(3350, 13, 0.4);
    var bleed = ctx.createGain();
    bleed.gain.value = 0.07;
    wah.connect(bleed);
    bleed.connect(sum);

    var hp = ctx.createBiquadFilter();
    hp.type = 'highpass';
    hp.frequency.value = 360;
    sum.connect(hp);
    hp.connect(master);

    var t0 = ctx.currentTime;
    osc.start(t0);
    lfo.start(t0);
    noise.start(t0);

    inst.audio = { ctx: ctx, master: master, osc: osc, lfo: lfo, nGain: nGain, wah: wah };
    return inst.audio;
  }

  function updateAudio(inst) {
    var a = inst.audio;
    if (!a || a.ctx.state !== 'running') return;
    var t = a.ctx.currentTime;
    var active = inst.active;
    var rps = inst.rps;
    var theta = inst.theta;
    var drive = inst.drive;

    a.master.gain.setTargetAtTime(0.85 * Math.pow(active, 1.3), t, 0.07);
    var f0 = clamp(55 + rps * 17, 50, 195);
    a.osc.frequency.setTargetAtTime(f0, t, 0.06);
    a.osc.detune.setTargetAtTime(46 * Math.sin(theta + 0.9) * clamp(active * 1.6, 0, 1), t, 0.03);
    a.lfo.frequency.setTargetAtTime(24 + rps * 4.5, t, 0.1);
    var wf = 760 + 520 * active + (430 + 330 * active) * Math.sin(theta - 0.7);
    a.wah.frequency.setTargetAtTime(Math.max(320, wf), t, 0.025);
    a.nGain.gain.setTargetAtTime((0.03 + 0.17 * active) * clamp(drive * 4, 0, 1), t, 0.08);
  }

  function physStep(inst, h) {
    var stick = inst.stick;
    var tube = inst.tube;
    var dx = tube.x - stick.x;
    var dy = tube.y - stick.y;
    var d = Math.hypot(dx, dy) || 1e-6;
    var ux = dx / d;
    var uy = dy / d;
    var ax = 0;
    var ay = GRAV;
    if (d > inst.ropeLen) {
      var vrad = tube.vx * ux + tube.vy * uy;
      var f = -ROPE_K * (d - inst.ropeLen) - ROPE_D * vrad;
      ax += f * ux;
      ay += f * uy;
    }
    ax -= AIR_DRAG * tube.vx;
    ay -= AIR_DRAG * tube.vy;
    tube.vx += ax * h;
    tube.vy += ay * h;
    tube.x += tube.vx * h;
    tube.y += tube.vy * h;
  }

  function simStep(inst, dt) {
    var W = inst.cssW;
    var H = inst.cssH;
    var target = inst.target;
    var stick = inst.stick;
    var auto = inst.auto;

    if (auto.on) {
      auto.rps += (3.4 - auto.rps) * Math.min(1, dt * 1.1);
      auto.phase += auto.rps * TAU * dt;
      target.x = auto.cx + inst.autoR * Math.cos(auto.phase);
      target.y = auto.cy + inst.autoR * Math.sin(auto.phase);
    } else {
      auto.rps *= Math.max(0, 1 - dt * 3);
    }

    var k = 1 - Math.exp(-dt * 26);
    stick.x += (target.x - stick.x) * k;
    stick.y += (target.y - stick.y) * k;

    var acc = dt;
    while (acc > 1e-6) {
      var s = Math.min(PHYS_H, acc);
      physStep(inst, s);
      acc -= s;
    }

    var tube = inst.tube;
    var theta = Math.atan2(tube.y - stick.y, tube.x - stick.x);
    var dth = theta - inst.prevTheta;
    while (dth > Math.PI) dth -= TAU;
    while (dth < -Math.PI) dth += TAU;
    if (dt > 1e-6) {
      inst.omega += (dth / dt - inst.omega) * Math.min(1, dt * 9);
    }
    inst.prevTheta = theta;
    inst.theta = theta;
    inst.rps = Math.abs(inst.omega) / TAU;

    // Count full revolutions as "wow" for both manual and auto spin.
    if (inst.active > 0.3) {
      inst.revAccum += Math.abs(dth);
      if (inst.revAccum >= TAU) {
        var n = Math.floor(inst.revAccum / TAU);
        inst.revAccum -= n * TAU;
        inst.wowCount += n;
      }
    } else {
      inst.revAccum = 0;
    }

    inst.ropeDist = Math.hypot(tube.x - stick.x, tube.y - stick.y);
    inst.taut = clamp((inst.ropeDist / inst.ropeLen - 0.88) / 0.12, 0, 1);
    inst.drive = clamp((inst.rps - 1.1) / 2.6, 0, 1);
    var tgt = Math.pow(inst.drive, 1.25) * inst.taut;
    inst.active += (tgt - inst.active) * Math.min(1, dt * (tgt > inst.active ? 10 : 3.2));

    if (inst.active < 0.02 && inst.rps < 0.15 && !inst.dragging && !auto.on) {
      inst.idleTime += dt;
      if (inst.idleTime > 8 && inst.audio && inst.audio.ctx.state === 'running') {
        inst.audio.ctx.suspend().catch(function () {});
      }
    } else {
      inst.idleTime = 0;
    }

    updateAudio(inst);
  }

  function roundRectPath(ctx, x, y, w, h, r) {
    if (ctx.roundRect) {
      ctx.beginPath();
      ctx.roundRect(x, y, w, h, r);
      return;
    }
    ctx.beginPath();
    ctx.moveTo(x + r, y);
    ctx.arcTo(x + w, y, x + w, y + h, r);
    ctx.arcTo(x + w, y + h, x, y + h, r);
    ctx.arcTo(x, y + h, x, y, r);
    ctx.arcTo(x, y, x + w, y, r);
    ctx.closePath();
  }

  function drawScene(inst, now) {
    var ctx = inst.ctx;
    var W = inst.cssW;
    var H = inst.cssH;
    var stick = inst.stick;
    var tube = inst.tube;
    var active = inst.active;

    ctx.clearRect(0, 0, W, H);

    // night-ish playground (toy-like atmosphere, original palette)
    var bg = ctx.createRadialGradient(W * 0.5, H * 0.35, 20, W * 0.5, H * 0.5, Math.max(W, H) * 0.8);
    bg.addColorStop(0, '#1a2744');
    bg.addColorStop(0.55, '#0e1628');
    bg.addColorStop(1, '#070b14');
    ctx.fillStyle = bg;
    ctx.fillRect(0, 0, W, H);

    // soft ground ellipse
    ctx.strokeStyle = 'rgba(120, 180, 140, 0.12)';
    ctx.lineWidth = 2;
    ctx.beginPath();
    ctx.ellipse(W * 0.5, H * 0.78, Math.min(W, H) * 0.32, Math.min(W, H) * 0.06, 0, 0, TAU);
    ctx.stroke();

    // sound ripples
    if (active > 0.08) {
      for (var i = 0; i < 3; i++) {
        var rr = 18 + i * 16 + (now / 40) % 16;
        ctx.strokeStyle = 'rgba(255, 180, 100, ' + (0.18 * active * (1 - i * 0.25)) + ')';
        ctx.lineWidth = 1.5;
        ctx.beginPath();
        ctx.arc(tube.x, tube.y, rr, 0, TAU);
        ctx.stroke();
      }
    }

    drawToy(inst, now);

    // HUD rpm
    var rpmText = (Math.round(inst.rps * 10) / 10).toFixed(1);
    ctx.textAlign = 'center';
    ctx.fillStyle = active > 0.15 ? '#ff8a5c' : 'rgba(200, 220, 210, 0.85)';
    ctx.font = '700 ' + Math.max(30, W * 0.085) + 'px "Segoe UI", system-ui, sans-serif';
    ctx.fillText(rpmText, W * 0.5, H * 0.9);
    ctx.fillStyle = 'rgba(180, 200, 190, 0.7)';
    ctx.font = '500 ' + Math.max(12, W * 0.028) + 'px "Segoe UI", system-ui, sans-serif';
    ctx.fillText('圈 / 秒', W * 0.5, H * 0.9 + Math.max(18, W * 0.04));

    if (!inst.dragging && !inst.auto.on && inst.rps < 0.2) {
      ctx.fillStyle = 'rgba(230, 240, 235, 0.7)';
      ctx.font = '500 ' + Math.max(13, W * 0.032) + 'px "Segoe UI", system-ui, sans-serif';
      ctx.fillText('按住 · 画圈 · 甩起来', W * 0.5, H * 0.12);
    }
  }

  /** Original 2D toy drawing of a traditional bamboo cicada. */
  function drawToy(inst, now) {
    var ctx = inst.ctx;
    var stick = inst.stick;
    var tube = inst.tube;
    var ropeLen = inst.ropeLen;
    var active = inst.active;
    var dx = tube.x - stick.x;
    var dy = tube.y - stick.y;
    var d = Math.hypot(dx, dy) || 1e-6;
    var ux = dx / d;
    var uy = dy / d;

    // rosined string — sag when slack
    ctx.strokeStyle = '#c94a38';
    ctx.lineWidth = 1.8;
    ctx.beginPath();
    ctx.moveTo(stick.x, stick.y);
    if (d < ropeLen * 0.97) {
      var sag = (ropeLen - d) * 0.5;
      ctx.quadraticCurveTo((stick.x + tube.x) * 0.5, (stick.y + tube.y) * 0.5 + sag, tube.x, tube.y);
    } else {
      ctx.lineTo(tube.x, tube.y);
    }
    ctx.stroke();

    // handle stick
    var ang = Math.PI * 0.62;
    var hx = Math.cos(ang);
    var hy = Math.sin(ang);
    ctx.save();
    ctx.translate(stick.x, stick.y);
    var sg = ctx.createLinearGradient(0, 0, hx * 92, hy * 92);
    sg.addColorStop(0, '#f0e0b4');
    sg.addColorStop(1, '#8f7340');
    ctx.strokeStyle = sg;
    ctx.lineWidth = 5.5;
    ctx.lineCap = 'round';
    ctx.beginPath();
    ctx.moveTo(hx * 6, hy * 6);
    ctx.lineTo(hx * 92, hy * 92);
    ctx.stroke();
    // tip beads
    [[hx * 5, hy * 5, 6.2], [hx * 16, hy * 16, 4.4]].forEach(function (b) {
      ctx.fillStyle = '#b52d22';
      ctx.beginPath();
      ctx.arc(b[0], b[1], b[2], 0, TAU);
      ctx.fill();
      ctx.fillStyle = 'rgba(255,210,190,0.5)';
      ctx.beginPath();
      ctx.arc(b[0] - b[2] * 0.25, b[1] - b[2] * 0.3, b[2] * 0.3, 0, TAU);
      ctx.fill();
    });
    ctx.restore();

    // bamboo barrel + cicada features
    ctx.save();
    ctx.translate(tube.x, tube.y);
    ctx.rotate(Math.atan2(uy, ux) - Math.PI / 2);
    var sc = Math.max(1.15, Math.min(inst.cssW, inst.cssH) / 380);
    ctx.scale(sc, sc);

    var body = ctx.createLinearGradient(-14, 0, 14, 0);
    body.addColorStop(0, '#9e8650');
    body.addColorStop(0.28, '#efe4c0');
    body.addColorStop(0.7, '#d8c896');
    body.addColorStop(1, '#8a7340');
    ctx.fillStyle = body;
    roundRectPath(ctx, -13, 2, 26, 50, 7);
    ctx.fill();

    // bamboo rings
    ctx.strokeStyle = 'rgba(90,70,30,0.35)';
    ctx.lineWidth = 1.2;
    [14, 28, 40].forEach(function (yy) {
      ctx.beginPath();
      ctx.moveTo(-11, yy);
      ctx.lineTo(11, yy);
      ctx.stroke();
    });

    ctx.fillStyle = '#3d311c';
    ctx.beginPath();
    ctx.ellipse(0, 51, 12, 4.5, 0, 0, TAU);
    ctx.fill();

    // wings (original curves)
    var open = 0.22 + active * 0.42;
    var flap = active * Math.sin(now * 0.05) * 0.18;
    function drawWing(side) {
      ctx.save();
      ctx.translate(side * 10, 16);
      ctx.rotate(side * (open + flap));
      ctx.beginPath();
      ctx.moveTo(0, 0);
      ctx.bezierCurveTo(side * 16, 6, side * 18, 28, side * 6, 44);
      ctx.bezierCurveTo(side * 1, 34, side * -2, 14, 0, 0);
      ctx.fillStyle = 'rgba(250, 242, 220, 0.9)';
      ctx.fill();
      ctx.strokeStyle = 'rgba(130, 105, 60, 0.5)';
      ctx.lineWidth = 1;
      ctx.stroke();
      ctx.beginPath();
      ctx.moveTo(side * 2, 5);
      ctx.quadraticCurveTo(side * 10, 20, side * 5, 40);
      ctx.stroke();
      ctx.restore();
    }
    drawWing(-1);
    drawWing(1);

    // red head band
    ctx.fillStyle = '#d64535';
    roundRectPath(ctx, -13.5, 0, 27, 11, 4.5);
    ctx.fill();

    // membrane
    ctx.fillStyle = '#f8f1dc';
    ctx.beginPath();
    ctx.ellipse(0, 2.5, 11, 4.5, 0, 0, TAU);
    ctx.fill();
    if (active > 0.05) {
      ctx.save();
      ctx.globalAlpha = clamp(active, 0, 1) * 0.8;
      ctx.shadowColor = '#ffd7a0';
      ctx.shadowBlur = 16 * active;
      ctx.fillStyle = '#ffe8c2';
      ctx.beginPath();
      ctx.ellipse(0, 2.5, 8.8, 3.5, 0, 0, TAU);
      ctx.fill();
      ctx.restore();
    }

    // eyes
    [-1, 1].forEach(function (side) {
      ctx.fillStyle = '#14110c';
      ctx.beginPath();
      ctx.arc(side * 9, 6.5, 2.5, 0, TAU);
      ctx.fill();
      ctx.fillStyle = 'rgba(255,255,255,0.8)';
      ctx.beginPath();
      ctx.arc(side * 9 - 0.6, 5.8, 0.7, 0, TAU);
      ctx.fill();
    });

    // knot
    ctx.fillStyle = '#e25540';
    ctx.beginPath();
    ctx.arc(0, 1.5, 2.2, 0, TAU);
    ctx.fill();

    ctx.restore();
  }

  function layout(inst) {
    var parent = inst.canvas.parentElement;
    var cssW = parent ? parent.clientWidth : 640;
    var cssH = Math.max(360, Math.min(560, Math.round(cssW * 0.78)));
    var dpr = window.devicePixelRatio || 1;
    inst.cssW = cssW;
    inst.cssH = cssH;
    inst.canvas.style.width = cssW + 'px';
    inst.canvas.style.height = cssH + 'px';
    inst.canvas.width = Math.round(cssW * dpr);
    inst.canvas.height = Math.round(cssH * dpr);
    inst.ctx.setTransform(dpr, 0, 0, dpr, 0, 0);

    var minDim = Math.min(cssW, cssH);
    inst.ropeLen = clamp(minDim * 0.28, 90, 150);
    inst.autoR = clamp(minDim * 0.13, 38, 58);

    if (!inst.placed) {
      recenter(inst);
      inst.placed = true;
    }
  }

  function recenter(inst) {
    var W = inst.cssW;
    var H = inst.cssH;
    inst.target.x = inst.stick.x = W * 0.5;
    inst.target.y = inst.stick.y = H * 0.42;
    inst.auto.cx = W * 0.5;
    inst.auto.cy = H * 0.42;
    inst.tube.x = inst.stick.x + 8;
    inst.tube.y = inst.stick.y + inst.ropeLen * 0.92;
    inst.tube.vx = 34;
    inst.tube.vy = 0;
    inst.prevTheta = Math.atan2(inst.tube.y - inst.stick.y, inst.tube.x - inst.stick.x);
    inst.theta = inst.prevTheta;
  }

  function pointerPos(inst, clientX, clientY) {
    var rect = inst.canvas.getBoundingClientRect();
    var x = clientX - rect.left;
    var y = clientY - rect.top - inst.pointerLift;
    return { x: x, y: y };
  }

  function bind(inst) {
    var canvas = inst.canvas;

    function down(e) {
      ensureAudio(inst);
      if (inst.audio && inst.audio.ctx.state === 'suspended') inst.audio.ctx.resume();
      inst.dragging = true;
      inst.auto.on = false;
      var pt = e.touches ? e.touches[0] : e;
      inst.pointerLift = e.touches ? Math.min(110, inst.ropeLen * 0.9) : 0;
      var p = pointerPos(inst, pt.clientX, pt.clientY);
      inst.target.x = p.x;
      inst.target.y = p.y;
      e.preventDefault();
    }

    function move(e) {
      if (!inst.dragging) return;
      var pt = e.touches ? e.touches[0] : e;
      var p = pointerPos(inst, pt.clientX, pt.clientY);
      inst.target.x = clamp(p.x, 8, inst.cssW - 8);
      inst.target.y = clamp(p.y, 8, inst.cssH - 8);
      e.preventDefault();
    }

    function up() {
      inst.dragging = false;
      inst.pointerLift = 0;
    }

    canvas.addEventListener('mousedown', down);
    window.addEventListener('mousemove', move);
    window.addEventListener('mouseup', up);
    canvas.addEventListener('touchstart', down, { passive: false });
    canvas.addEventListener('touchmove', move, { passive: false });
    canvas.addEventListener('touchend', up);
    canvas.addEventListener('touchcancel', up);

    inst._onKey = function (e) {
      if (e.code !== 'Space') return;
      var tag = (e.target && e.target.tagName) || '';
      if (tag === 'INPUT' || tag === 'TEXTAREA' || tag === 'SELECT' || (e.target && e.target.isContentEditable)) return;
      e.preventDefault();
      window.zhuZhiLiao.setAuto(inst.canvas.id, !inst.auto.on);
    };
    window.addEventListener('keydown', inst._onKey);
    inst._onResize = function () {
      layout(inst);
      recenter(inst);
    };
    window.addEventListener('resize', inst._onResize);

    inst._onVisibility = function () {
      if (document.hidden) {
        if (inst.auto.on || inst.active > 0.05) {
          startBackgroundPump(inst);
        }
      } else {
        stopBackgroundPump(inst);
        if (inst.audio && inst.audio.ctx.state === 'suspended') {
          inst.audio.ctx.resume().catch(function () {});
        }
        inst.lastTs = 0;
        if (inst.running && !inst.raf) {
          inst.raf = requestAnimationFrame(function (t) { loop(inst, t); });
        }
      }
    };
    document.addEventListener('visibilitychange', inst._onVisibility);
  }

  function stopBackgroundPump(inst) {
    if (inst.bgTimer) {
      clearInterval(inst.bgTimer);
      inst.bgTimer = 0;
    }
  }

  function startBackgroundPump(inst) {
    if (inst.bgTimer || !inst.running) return;
    if (inst.raf) {
      cancelAnimationFrame(inst.raf);
      inst.raf = 0;
    }
    inst.lastTs = performance.now();
    inst.bgTimer = setInterval(function () {
      if (!inst.running) {
        stopBackgroundPump(inst);
        return;
      }
      if (!document.hidden) {
        stopBackgroundPump(inst);
        if (inst.audio && inst.audio.ctx.state === 'suspended') {
          inst.audio.ctx.resume().catch(function () {});
        }
        inst.lastTs = 0;
        if (!inst.raf) {
          inst.raf = requestAnimationFrame(function (t) { loop(inst, t); });
        }
        return;
      }
      // Keep auto-spin (and leftover sound) alive while tab is hidden.
      if (!inst.auto.on && inst.active < 0.05 && inst.rps < 0.2) {
        return;
      }
      ensureAudio(inst);
      if (inst.audio && inst.audio.ctx.state === 'suspended') {
        inst.audio.ctx.resume().catch(function () {});
      }
      var now = performance.now();
      var dt = clamp((now - (inst.lastTs || now)) / 1000, 0.001, 0.05);
      inst.lastTs = now;
      simStep(inst, dt);
      // Skip canvas draw in background to save CPU; still update stats occasionally.
      if (inst.onStats && (!inst._lastStatsAt || now - inst._lastStatsAt > 200)) {
        inst._lastStatsAt = now;
        inst.onStats({
          rpm: Math.round(inst.rps * 10) / 10,
          wow: inst.wowCount,
          auto: inst.auto.on
        });
      }
    }, 1000 / 30);
  }

  function loop(inst, ts) {
    if (!inst.running) return;
    if (document.hidden) {
      inst.raf = 0;
      if (inst.auto.on || inst.active > 0.05) {
        startBackgroundPump(inst);
      }
      return;
    }
    if (!inst.lastTs) inst.lastTs = ts;
    var dt = clamp((ts - inst.lastTs) / 1000, 0.001, 0.05);
    inst.lastTs = ts;
    simStep(inst, dt);
    drawScene(inst, ts);

    if (inst.onStats && (!inst._lastStatsAt || ts - inst._lastStatsAt > 100)) {
      inst._lastStatsAt = ts;
      inst.onStats({
        rpm: Math.round(inst.rps * 10) / 10,
        wow: inst.wowCount,
        auto: inst.auto.on
      });
    }
    inst.raf = requestAnimationFrame(function (t) { loop(inst, t); });
  }

  window.zhuZhiLiao = {
    mount: function (canvasId) {
      var canvas = document.getElementById(canvasId);
      if (!canvas || canvas.tagName !== 'CANVAS') return { ok: false };
      this.destroy(canvasId);
      var inst = {
        canvas: canvas,
        ctx: canvas.getContext('2d'),
        running: true,
        placed: false,
        cssW: 0,
        cssH: 0,
        ropeLen: 150,
        autoR: 58,
        stick: { x: 0, y: 0 },
        target: { x: 0, y: 0 },
        tube: { x: 0, y: 0, vx: 0, vy: 0 },
        auto: { on: false, rps: 0, phase: 0, cx: 0, cy: 0 },
        dragging: false,
        pointerLift: 0,
        prevTheta: 0,
        theta: 0,
        omega: 0,
        rps: 0,
        ropeDist: 0,
        taut: 0,
        drive: 0,
        active: 0,
        revAccum: 0,
        wowCount: 0,
        idleTime: 0,
        audio: null,
        raf: 0,
        bgTimer: 0,
        lastTs: 0,
        onStats: null
      };
      instances[canvasId] = inst;
      bind(inst);
      layout(inst);
      inst.raf = requestAnimationFrame(function (t) { loop(inst, t); });
      return { ok: true };
    },

    setStatsCallback: function (canvasId, dotNetRef) {
      var inst = instances[canvasId];
      if (!inst) return;
      inst.onStats = function (s) {
        try {
          dotNetRef.invokeMethodAsync('OnZhuStats', s.rpm, s.wow, s.auto);
        } catch (_) { /* disposed */ }
      };
    },

    setAuto: function (canvasId, enabled) {
      var inst = instances[canvasId];
      if (!inst) return { ok: false };
      ensureAudio(inst);
      if (inst.audio && inst.audio.ctx.state === 'suspended') inst.audio.ctx.resume();
      inst.auto.on = !!enabled;
      if (inst.auto.on) {
        inst.auto.cx = inst.cssW * 0.5;
        inst.auto.cy = inst.cssH * 0.42;
        inst.auto.phase = Math.atan2(inst.target.y - inst.auto.cy, inst.target.x - inst.auto.cx);
        if (inst.auto.rps < 1) inst.auto.rps = 2.2;
        if (document.hidden) startBackgroundPump(inst);
      } else if (document.hidden && !inst.dragging) {
        // No need to keep background pump if auto stopped and nearly silent.
        if (inst.active < 0.05) stopBackgroundPump(inst);
      }
      return { ok: true, auto: inst.auto.on };
    },

    toggleAuto: function (canvasId) {
      var inst = instances[canvasId];
      if (!inst) return { ok: false, auto: false };
      return this.setAuto(canvasId, !inst.auto.on);
    },

    unlockAudio: function (canvasId) {
      var inst = instances[canvasId];
      if (!inst) return { ok: false };
      ensureAudio(inst);
      if (inst.audio && inst.audio.ctx.state === 'suspended') inst.audio.ctx.resume();
      return { ok: true };
    },

    resetCount: function (canvasId) {
      var inst = instances[canvasId];
      if (!inst) return;
      inst.wowCount = 0;
      inst.revAccum = 0;
    },

    destroy: function (canvasId) {
      var inst = instances[canvasId];
      if (!inst) return;
      inst.running = false;
      if (inst.raf) cancelAnimationFrame(inst.raf);
      stopBackgroundPump(inst);
      if (inst._onKey) window.removeEventListener('keydown', inst._onKey);
      if (inst._onResize) window.removeEventListener('resize', inst._onResize);
      if (inst._onVisibility) document.removeEventListener('visibilitychange', inst._onVisibility);
      if (inst.audio && inst.audio.ctx) {
        try { inst.audio.ctx.close(); } catch (_) { /* */ }
      }
      delete instances[canvasId];
    }
  };
})();
