import { describe, it, expect } from 'vitest'
import { renderMarkdown } from './markdown'

// ADR-0006 safety invariants + the supported subset.
describe('renderMarkdown', () => {
  it('escapes raw HTML so it cannot execute', () => {
    const html = renderMarkdown('<script>alert(1)</script>')
    expect(html).not.toContain('<script>')
    expect(html).toContain('&lt;script&gt;')
  })

  it('neutralises an img onerror payload', () => {
    const html = renderMarkdown('<img src=x onerror=alert(1)>')
    expect(html).not.toContain('<img')
    expect(html).toContain('&lt;img')
  })

  it('blocks javascript: links (renders the label as text only)', () => {
    const html = renderMarkdown('[click](javascript:alert(1))')
    expect(html).not.toContain('<a ')
    expect(html).not.toContain('javascript:alert')
    expect(html).toContain('click')
  })

  it('allows http/https links', () => {
    const html = renderMarkdown('[dash](https://example.com/x)')
    expect(html).toContain('<a href="https://example.com/x"')
    expect(html).toContain('rel="noopener noreferrer"')
    expect(html).toContain('>dash</a>')
  })

  it('renders bold, italic, and inline code', () => {
    expect(renderMarkdown('**b**')).toContain('<strong>b</strong>')
    expect(renderMarkdown('*i*')).toContain('<em>i</em>')
    expect(renderMarkdown('`x`')).toContain('<code>x</code>')
  })

  it('renders an unordered list', () => {
    expect(renderMarkdown('- a\n- b')).toBe('<ul><li>a</li><li>b</li></ul>')
  })

  it('renders a fenced code block verbatim (no inline formatting)', () => {
    const html = renderMarkdown('```\nkubectl drain *node*\n```')
    expect(html).toContain('<pre><code>kubectl drain *node*</code></pre>')
  })

  it('returns empty string for empty/nullish input', () => {
    expect(renderMarkdown('')).toBe('')
    expect(renderMarkdown(null)).toBe('')
    expect(renderMarkdown(undefined)).toBe('')
  })
})
