// Hand-rolled, escape-first lightweight markdown renderer (ADR-0006 / 004 FR-004/006).
// Safety by construction: the entire source is HTML-escaped FIRST, so no raw
// HTML or <script> can survive; only the whitelist below re-introduces markup.
// Links are limited to http/https/mailto. No images, tables, or raw HTML.
// Pure function (no DOM) so it is unit-testable without a browser.

const ALLOWED_SCHEME = /^(https?:\/\/|mailto:)/i

function escapeHtml(s: string): string {
  return s
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;')
    .replace(/'/g, '&#39;')
}

// Inline formatting on already-escaped text: links, inline code, bold, italic.
function inline(text: string): string {
  let s = text
  // [label](url) — only safe schemes become anchors; otherwise rendered as text.
  s = s.replace(/\[([^\]]+)\]\(([^)\s]+)\)/g, (_m, label: string, url: string) =>
    ALLOWED_SCHEME.test(url)
      ? `<a href="${url}" rel="noopener noreferrer" target="_blank">${label}</a>`
      : label,
  )
  // `code`
  s = s.replace(/`([^`]+)`/g, '<code>$1</code>')
  // **bold** before *italic*
  s = s.replace(/\*\*([^*]+)\*\*/g, '<strong>$1</strong>')
  s = s.replace(/\*([^*\n]+)\*/g, '<em>$1</em>')
  s = s.replace(/_([^_\n]+)_/g, '<em>$1</em>')
  return s
}

export function renderMarkdown(src: string | null | undefined): string {
  if (!src) return ''
  const lines = escapeHtml(src.replace(/\r\n/g, '\n')).split('\n')
  const out: string[] = []
  let para: string[] = []
  let i = 0

  const flushPara = () => {
    if (para.length) {
      out.push(`<p>${para.map(inline).join('<br>')}</p>`)
      para = []
    }
  }

  while (i < lines.length) {
    const line = lines[i]

    // Fenced code block — content stays verbatim (already escaped), no inline pass.
    if (line.trimStart().startsWith('```')) {
      flushPara()
      const code: string[] = []
      i++
      while (i < lines.length && !lines[i].trimStart().startsWith('```')) {
        code.push(lines[i])
        i++
      }
      i++ // skip closing fence
      out.push(`<pre><code>${code.join('\n')}</code></pre>`)
      continue
    }

    // Unordered list ("- " or "* ").
    if (/^\s*[-*]\s+/.test(line)) {
      flushPara()
      const items: string[] = []
      while (i < lines.length && /^\s*[-*]\s+/.test(lines[i])) {
        items.push(`<li>${inline(lines[i].replace(/^\s*[-*]\s+/, ''))}</li>`)
        i++
      }
      out.push(`<ul>${items.join('')}</ul>`)
      continue
    }

    // Ordered list ("1. ").
    if (/^\s*\d+\.\s+/.test(line)) {
      flushPara()
      const items: string[] = []
      while (i < lines.length && /^\s*\d+\.\s+/.test(lines[i])) {
        items.push(`<li>${inline(lines[i].replace(/^\s*\d+\.\s+/, ''))}</li>`)
        i++
      }
      out.push(`<ol>${items.join('')}</ol>`)
      continue
    }

    // Blank line ends a paragraph.
    if (line.trim() === '') {
      flushPara()
      i++
      continue
    }

    para.push(line)
    i++
  }

  flushPara()
  return out.join('')
}
