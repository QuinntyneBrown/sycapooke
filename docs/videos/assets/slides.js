// Shared runtime for docs/videos/NN-topic/slides.html: shows one slide at a time, adds the header
// (logo and part label from data-parts) and the progress bar. Navigate with the arrow keys, ?slide=N
// or #N (N from 0). The video builder screenshots each slide through ?slide=N.
(function () {
  const slides = [...document.querySelectorAll('body > section')];
  const parts = (document.body.dataset.parts || '').split('|');
  const logo = document.body.dataset.logo;
  slides.forEach((s, i) => {
    if (i > 0) {
      const head = document.createElement('div');
      head.className = 'deck-head';
      const dark = s.matches('.band, .forest') && document.body.dataset.logoDark;
      head.innerHTML = (logo ? `<img src="${dark ? document.body.dataset.logoDark : logo}" alt="">` : '') +
        `<span class="part">${parts[+s.dataset.part || 0] || ''}</span>`;
      s.prepend(head);
    }
    const bar = document.createElement('div');
    bar.className = 'deck-progress';
    bar.style.width = `${(100 * i) / Math.max(1, slides.length - 1)}%`;
    s.append(bar);
  });
  const fromUrl = () => +(new URLSearchParams(location.search).get('slide') ?? location.hash.slice(1)) || 0;
  let current = 0;
  const show = (n) => {
    current = Math.max(0, Math.min(slides.length - 1, n));
    slides.forEach((s, i) => s.classList.toggle('active', i === current));
  };
  addEventListener('keydown', (e) => {
    if (['ArrowRight', 'PageDown', ' '].includes(e.key)) show(current + 1);
    if (['ArrowLeft', 'PageUp'].includes(e.key)) show(current - 1);
  });
  addEventListener('hashchange', () => show(fromUrl()));
  show(fromUrl());
  window.deck = { count: slides.length, show };
})();
