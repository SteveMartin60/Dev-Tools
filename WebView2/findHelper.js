// findHelper.js — Full, unabbreviated drop-in replacement
// Works inside WebView2's isolated world via postMessage
// Highlights text, supports case-sensitive search, next/prev, clear
// Communicates results back via window.chrome.webview.postMessage

(() => {
    // Shared state for active find operation
    let findInfo = null;

    // Inject global styles once
    if (!document.head.querySelector('#wv2-find-style')) {
        const style = document.createElement('style');
        style.id = 'wv2-find-style';
        style.textContent = `
            .wv2-find {
                background-color: yellow !important;
                cursor: pointer;
            }
            .wv2-find-active {
                background-color: orange !important;
                box-shadow: 0 0 2px 1px rgba(0,0,0,0.3);
            }
            .wv2-find:hover {
                background-color: #ffeb3b !important;
            }
        `;
        document.head.appendChild(style);
    }

    // Internal helper: Clear all highlights
    function clearHighlights() {
        if (findInfo && Array.isArray(findInfo.spans)) {
            findInfo.spans.forEach(span => {
                if (span && span.parentNode) {
                    const textNode = document.createTextNode(span.textContent);
                    span.parentNode.replaceChild(textNode, span);
                }
            });
        }
        findInfo = null;
    }

    // Internal helper: Highlight all matches
    function performFind(term, caseSensitive) {
        clearHighlights();

        if (!term) {
            window.chrome.webview.postMessage({
                type: 'findResult',
                matchCount: 0
            });
            return;
        }

        const flags = caseSensitive ? 'g' : 'gi';
        const escaped = term.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
        const regex = new RegExp(escaped, flags);

        const hits = [];

        const walker = document.createTreeWalker(
            document.body,
            NodeFilter.SHOW_TEXT,
            {
                acceptNode: function (node) {
                    const parent = node.parentElement;
                    if (!parent) return NodeFilter.FILTER_REJECT;
                    const tagName = parent.tagName;
                    if (['SCRIPT', 'STYLE', 'NOSCRIPT'].includes(tagName)) {
                        return NodeFilter.FILTER_REJECT;
                    }
                    return NodeFilter.FILTER_ACCEPT;
                }
            }
        );

        let node;
        while ((node = walker.nextNode())) {
            regex.lastIndex = 0;
            let match;
            while ((match = regex.exec(node.textContent))) {
                hits.push({
                    node,
                    start: match.index,
                    end: match.index + match[0].length
                });
            }
        }

        // Highlight backwards to avoid offset corruption
        const spans = [];
        for (let i = hits.length - 1; i >= 0; i--) {
            const { node, start, end } = hits[i];
            const range = document.createRange();
            range.setStart(node, start);
            range.setEnd(node, end);

            const span = document.createElement('span');
            span.className = 'wv2-find';

            const fragment = range.extractContents();
            span.appendChild(fragment);
            range.insertNode(span);

            spans.unshift(span);
        }

        findInfo = {
            term,
            caseSensitive,
            matchCount: hits.length,
            activeIndex: 0,
            spans
        };

        activateSpan(0);

        window.chrome.webview.postMessage({
            type: 'findResult',
            matchCount: hits.length
        });
    }

    // Internal helper: Navigate to next/prev match
    function navigate(direction) {
        if (!findInfo || findInfo.matchCount === 0) return;

        let idx = findInfo.activeIndex + direction;
        if (idx >= findInfo.matchCount) idx = 0;
        if (idx < 0) idx = findInfo.matchCount - 1;

        findInfo.activeIndex = idx;
        activateSpan(idx);
    }

    // Internal helper: Activate a specific span (highlight + scroll)
    function activateSpan(index) {
        if (!findInfo || !findInfo.spans || findInfo.spans.length === 0) return;

        // Remove active class from all
        findInfo.spans.forEach(s => {
            s.classList.remove('wv2-find-active');
            s.classList.add('wv2-find');
        });

        const span = findInfo.spans[index];
        if (span) {
            span.classList.remove('wv2-find');
            span.classList.add('wv2-find-active');
            span.scrollIntoView({ behavior: 'auto', block: 'center' });
        }

        window.chrome.webview.postMessage({
            type: 'findResult',
            matchCount: findInfo.matchCount,
            activeIndex: index
        });
    }

    // Expose findHelper to main world (for debugging)
    window.findHelper = {
        start: performFind,
        next: () => navigate(1),
        prev: () => navigate(-1),
        clear: clearHighlights
    };

    // Listen for commands from C# via postMessage
    window.chrome.webview.addEventListener('message', event => {
        const data = event.data;
        switch (data.command) {
            case 'start':
                performFind(data.term, data.caseSensitive);
                break;
            case 'next':
                navigate(1);
                break;
            case 'prev':
                navigate(-1);
                break;
            case 'clear':
                clearHighlights();
                window.chrome.webview.postMessage({
                    type: 'findResult',
                    matchCount: 0
                });
                break;
        }
    });
})();
