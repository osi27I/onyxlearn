(function () {
    var d = document, b = d.body, fine = matchMedia('(hover:hover)').matches && !matchMedia('(prefers-reduced-motion:reduce)').matches;
    function el(t, c) { var e = d.createElement(t); e.className = c; return e; }
    function rnd(a, z) { return a + Math.random() * (z - a); }

    var sky = el('div', 'sky'), i, n;
    for (i = 0; i < 4; i++) {
        n = el('i', 'cloud');
        n.style.cssText = '--y:' + rnd(0, 70) + '%;--s:' + rnd(.6, 1.6) + ';--t:' + rnd(70, 140) + 's;--d:-' + rnd(0, 140) + 's;--o:' + rnd(.55, .95);
        sky.appendChild(n);
    }
    for (i = 0; i < 7; i++) {
        var z = rnd(10, 70);
        n = el('b', 'bubble');
        n.style.cssText = '--x:' + rnd(0, 100) + '%;--z:' + z + 'px;--t:' + rnd(16, 34) + 's;--d:-' + rnd(0, 34) + 's';
        sky.appendChild(n);
    }
    b.insertBefore(sky, b.firstChild);

    d.querySelectorAll('.hero').forEach(function (h) {
        [[240, '8%', '50px', 1.2]].forEach(function (o) {
            var orb = el('div', 'orb');
            orb.style.cssText = 'width:' + o[0] + 'px;height:' + o[0] + 'px;right:' + o[1] + ';top:' + o[2];
            h.appendChild(orb);
        });
    });

    var io = 'IntersectionObserver' in window ? new IntersectionObserver(function (es) {
        es.forEach(function (e) {
            if (e.isIntersecting) {
                var t = e.target;
                t.classList.add('in');
                io.unobserve(t);
                setTimeout(function () { t.style.transitionDelay = '0s'; }, 1400);
            }
        });
    }, { threshold: .1 }) : null;
    d.querySelectorAll('.card,.stat,.form,.hero h1,.hero p,h2,.t,.gv,.bar,.lessons li,.comment,.q,.panel,.course-head,.mc,.resume').forEach(function (e, k) {
        if (e.closest('.quiz')) return; // quiz questions appear instantly, one at a time
        e.classList.add('reveal');
        e.style.transitionDelay = (k % 6) * 70 + 'ms';
        if (io) io.observe(e); else e.classList.add('in');
    });

    // Pointer effects. One requestAnimationFrame per frame, and only while the mouse is moving, so the page is
    // completely idle the rest of the time. The ring is set straight to the pointer position (no easing), so it
    // never trails behind the cursor.
    var ring, mx = 0, my = 0, tilt = null, queued = false;
    if (fine) {
        ring = el('div', 'cur-ring');
        b.appendChild(ring);
    }

    function frame() {
        queued = false;
        if (ring) { ring.classList.add('on'); ring.style.transform = 'translate3d(' + mx + 'px,' + my + 'px,0)'; }
        // the whole sky shifts a little against the pointer (one element, GPU-composited)
        sky.style.transform = 'translate3d(' + (-(mx / innerWidth - .5) * 24) + 'px,' + (-(my / innerHeight - .5) * 18) + 'px,0)';
        if (tilt) {
            var r = tilt.getBoundingClientRect(), px = (mx - r.left) / r.width, py = (my - r.top) / r.height;
            tilt.style.transform = 'perspective(900px) rotateX(' + ((.5 - py) * 6) + 'deg) rotateY(' + ((px - .5) * 8) + 'deg) scale(1.02)';
            tilt.style.setProperty('--mx', px * 100 + '%');
            tilt.style.setProperty('--my', py * 100 + '%');
        }
    }

    if (fine) {
        d.addEventListener('mousemove', function (e) {
            mx = e.clientX;
            my = e.clientY;
            tilt = e.target.closest && e.target.closest('.card,.stat');
            if (!queued) { queued = true; requestAnimationFrame(frame); }
        }, { passive: true });
        d.documentElement.addEventListener('mouseleave', function () { ring.classList.remove('on'); });
    }

    d.addEventListener('mouseout', function (e) {
        var c = e.target.closest && e.target.closest('.card,.stat');
        if (c && !c.contains(e.relatedTarget)) { c.style.transform = ''; if (tilt === c) tilt = null; }
        var m = e.target.closest && e.target.closest('.btn,input[type=submit]');
        if (m && !m.contains(e.relatedTarget)) m.style.translate = '';
    });

    d.addEventListener('mouseover', function (e) {
        if (ring) ring.classList.toggle('big', !!(e.target.closest && e.target.closest('a,button,input[type=submit],.card,.stat,select,label')));
    });
    d.addEventListener('mousedown', function () { if (ring) ring.classList.add('press'); });
    d.addEventListener('mouseup', function () { if (ring) ring.classList.remove('press'); });

    d.addEventListener('click', function (e) {
        var r = el('div', 'ripple');
        r.style.left = e.clientX + 'px';
        r.style.top = e.clientY + 'px';
        b.appendChild(r);
        setTimeout(function () { r.remove(); }, 750);

        var a = e.target.closest && e.target.closest('a[href]');
        if (!a || e.ctrlKey || e.metaKey || a.target || /^(javascript|#|mailto)/i.test(a.getAttribute('href')) || a.origin !== location.origin) return;
        e.preventDefault();
        b.classList.add('leaving');
        setTimeout(function () { location.href = a.href; }, 150);
    });

    // On phones the lesson outline starts folded so the lesson itself is the first thing you see.
    d.querySelectorAll('details.outline').forEach(function (o) { if (matchMedia('(max-width:900px)').matches) o.open = false; });

    // ---- Quiz: one question at a time. Without JavaScript every question is simply shown on one page.
    // The server still checks that every question is answered and marks the whole quiz.
    d.querySelectorAll('.quiz').forEach(function (qz) {
        var qs = [].slice.call(qz.querySelectorAll('.q')), cur = 0, k;
        if (qs.length < 2) return;
        var step = el('div', 'qstep'), label = el('span', ''), bar = el('div', 'qbar'), fill = el('i', ''), dots = el('div', 'qdots'), nav = el('div', 'qnav');
        var back = el('button', 'btn ghost'), next = el('button', 'btn'), submit = qz.querySelector('input[type=submit]');
        back.type = next.type = 'button';
        back.textContent = 'Back';
        next.textContent = 'Next';
        label.setAttribute('aria-live', 'polite');
        step.appendChild(label);
        bar.appendChild(fill);
        nav.appendChild(back);
        nav.appendChild(next);
        qz.insertBefore(step, qs[0]);
        qz.insertBefore(bar, qs[0]);
        qz.insertBefore(dots, qs[0]);
        qs[qs.length - 1].parentNode.insertBefore(nav, qs[qs.length - 1].nextSibling);

        function answered(q) { return !!q.querySelector('input:checked'); }
        var btns = qs.map(function (q, i) {
            var bt = el('button', ''); bt.type = 'button'; bt.textContent = i + 1; bt.setAttribute('aria-label', 'Question ' + (i + 1));
            bt.addEventListener('click', function () { show(i); });
            dots.appendChild(bt);
            return bt;
        });
        function sync() {
            btns.forEach(function (bt, n) {
                bt.classList.toggle('on', n === cur);
                bt.classList.toggle('ans', answered(qs[n]));
                if (n === cur) bt.setAttribute('aria-current', 'step'); else bt.removeAttribute('aria-current');
            });
            next.disabled = !answered(qs[cur]);
        }
        function show(i) {
            cur = i;
            qs.forEach(function (q, n) { q.classList.toggle('cur', n === i); });
            label.textContent = 'Question ' + (i + 1) + ' of ' + qs.length;
            fill.style.width = ((i + 1) / qs.length * 100) + '%';
            back.disabled = i === 0;
            var last = i === qs.length - 1;
            next.style.display = last ? 'none' : '';
            if (submit) submit.style.display = last ? '' : 'none';
            sync();
        }
        back.addEventListener('click', function () { if (cur > 0) show(cur - 1); });
        next.addEventListener('click', function () { if (cur < qs.length - 1) show(cur + 1); });
        qz.addEventListener('change', sync);
        qz.classList.add('stepper');
        var start = qs.length - 1;
        for (k = 0; k < qs.length; k++) if (!answered(qs[k])) { start = k; break; }
        show(start);
    });

    // ---- Toasts: messages from the server (saved, deleted, errors) pop up in the corner instead of sitting in the page.
    // Without JavaScript the original message stays where it is. Anything with data-toast or the .alert class is picked up.
    var tbox = el('div', 'toasts');
    tbox.setAttribute('role', 'region');
    tbox.setAttribute('aria-label', 'Notifications');
    b.appendChild(tbox);
    function toast(msg, kind) {
        var t = el('div', 'toast ' + kind), ic = el('span', 'ic'), p = el('p', ''), x = el('button', '');
        t.setAttribute('role', kind === 'err' ? 'alert' : 'status');
        ic.setAttribute('aria-hidden', 'true');
        ic.textContent = kind === 'ok' ? '\u2713' : '!';
        p.textContent = msg;
        x.type = 'button';
        x.setAttribute('aria-label', 'Dismiss message');
        x.innerHTML = '&times;';
        function close() { t.classList.add('out'); setTimeout(function () { if (t.parentNode) t.parentNode.removeChild(t); }, 260); }
        x.addEventListener('click', close);
        t.appendChild(ic); t.appendChild(p); t.appendChild(x);
        tbox.appendChild(t);
        setTimeout(close, kind === 'err' ? 9000 : 5500);
    }
    d.querySelectorAll('[data-toast],.alert').forEach(function (m) {
        var txt = m.textContent.trim();
        if (!txt) return;
        var kind = m.getAttribute('data-toast') || (m.classList.contains('bad') ? 'err' : m.classList.contains('ok') ? 'ok' : 'warn');
        toast(txt, kind);
        m.hidden = true;
    });

    d.querySelectorAll('[data-print]').forEach(function (x) { x.addEventListener('click', function () { window.print(); }); });

    // ---- Client-side form helpers (progressive enhancement; the server still validates everything) ----
    var pw = d.getElementById('txtPw') || d.getElementById('txtNew'), bar = d.getElementById('pwBar'), say = d.getElementById('pwText');
    if (pw && bar) {
        var names = ['Too weak', 'Weak', 'Fair', 'Good', 'Strong'], cols = ['#e03131', '#e8590c', '#e0a800', '#2f80ed', '#2fa84f'];
        pw.addEventListener('input', function () {
            var v = pw.value, s = 0;
            if (v.length >= 8) s++;
            if (/[a-z]/.test(v) && /[A-Z]/.test(v)) s++;
            if (/\d/.test(v)) s++;
            if (/[^A-Za-z0-9]/.test(v) || v.length >= 12) s++;
            if (!v) s = 0;
            bar.style.width = (v ? Math.max(8, s * 25) : 0) + '%';
            bar.style.background = cols[s];
            if (say) say.textContent = v ? 'Password strength: ' + names[s] : '';
        });
    }
    var show = d.getElementById('showPw');
    if (show) {
        show.addEventListener('change', function () {
            ['txtPw', 'txtPw2', 'txtOld', 'txtNew', 'txtNew2'].forEach(function (id) {
                var f = d.getElementById(id);
                if (f) f.type = show.checked ? 'text' : 'password';
            });
        });
    }
    // ASP.NET ignores MaxLength on multi-line boxes, so the limit comes from data-max.
    d.querySelectorAll('textarea[data-max]').forEach(function (t) {
        var c = el('span', 'count'), max = +t.getAttribute('data-max');
        t.maxLength = max;
        t.parentNode.insertBefore(c, t.nextSibling);
        function upd() { var left = max - t.value.length; c.textContent = left + ' characters left'; c.classList.toggle('warn', left < 40); }
        t.addEventListener('input', upd);
        upd();
    });

    addEventListener('pageshow', function () { b.classList.remove('leaving'); });
})();
