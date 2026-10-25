// Hosts control state via data-z-valky-state. A one-shot celebration returns to idle.
const reducedMotion = matchMedia('(prefers-reduced-motion: reduce)');
const reducedTimers = new WeakMap();

function returnToIdle(companion) {
    if (companion.dataset.zValkyState !== 'celebrating' || companion.dataset.zValkyCelebration !== 'once') return;
    companion.dataset.zValkyState = 'idle';
    companion.dispatchEvent(new CustomEvent('zoorce:valky-state-change', { bubbles: true, detail: { state: 'idle', reason: 'celebration-complete' } }));
}

function updateReducedMotion(companion) {
    clearTimeout(reducedTimers.get(companion));
    if (reducedMotion.matches && companion.dataset.zValkyState === 'celebrating' && companion.dataset.zValkyCelebration === 'once') {
        reducedTimers.set(companion, setTimeout(() => returnToIdle(companion), 1200));
    }
}

document.addEventListener('animationend', event => {
    if (event.animationName !== 'z-valky-celebrate') return;
    const companion = event.target.closest('.z-valky');
    if (companion) returnToIdle(companion);
});

const observer = new MutationObserver(records => {
    for (const record of records) {
        if (record.type === 'attributes') updateReducedMotion(record.target);
        for (const node of record.addedNodes) {
            if (!(node instanceof Element)) continue;
            if (node.matches('.z-valky')) updateReducedMotion(node);
            node.querySelectorAll('.z-valky').forEach(updateReducedMotion);
        }
    }
});
observer.observe(document.documentElement, { subtree: true, childList: true, attributes: true, attributeFilter: ['data-z-valky-state', 'data-z-valky-celebration'] });
document.querySelectorAll('.z-valky').forEach(updateReducedMotion);
reducedMotion.addEventListener('change', () => document.querySelectorAll('.z-valky').forEach(updateReducedMotion));
