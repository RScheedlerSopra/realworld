import { describe, test, expect, vi, beforeEach } from 'vitest'
import { render, screen } from '@testing-library/react'
import { BrowserRouter } from 'react-router-dom'
import '@testing-library/jest-dom'
import ArticlePreview from './ArticlePreview'
import * as hooks from '../hooks'

vi.mock('../hooks', () => ({
  useArticleQuery: vi.fn(),
  useFavoriteArticleMutation: vi.fn()
}))

vi.mock('./FavoriteArticleButton', () => ({
  default: ({ children, className, favorited, slug }) => (
    <button className={className} data-favorited={favorited} data-slug={slug}>
      {children}
    </button>
  )
}))

describe('ArticlePreview Component', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  const mockArticle = {
    slug: 'test-article',
    title: 'Test Article Title',
    body: 'This is the article body content.',
    tagList: ['testing', 'javascript'],
    createdAt: '2024-01-15T10:00:00.000Z',
    favoritesCount: 42,
    favorited: false,
    author: {
      username: 'johndoe',
      image: 'https://example.com/avatar.jpg'
    }
  }

  test('renders article title', () => {
    vi.mocked(hooks.useArticleQuery).mockReturnValue({
      data: { article: mockArticle }
    })

    render(
      <BrowserRouter>
        <ArticlePreview article={mockArticle} />
      </BrowserRouter>
    )

    expect(screen.getByText('Test Article Title')).toBeInTheDocument()
  })

  test('renders article body', () => {
    vi.mocked(hooks.useArticleQuery).mockReturnValue({
      data: { article: mockArticle }
    })

    render(
      <BrowserRouter>
        <ArticlePreview article={mockArticle} />
      </BrowserRouter>
    )

    expect(screen.getByText('This is the article body content.')).toBeInTheDocument()
  })

  test('renders author username', () => {
    vi.mocked(hooks.useArticleQuery).mockReturnValue({
      data: { article: mockArticle }
    })

    render(
      <BrowserRouter>
        <ArticlePreview article={mockArticle} />
      </BrowserRouter>
    )

    expect(screen.getByText('johndoe')).toBeInTheDocument()
  })

  test('renders author image', () => {
    vi.mocked(hooks.useArticleQuery).mockReturnValue({
      data: { article: mockArticle }
    })

    render(
      <BrowserRouter>
        <ArticlePreview article={mockArticle} />
      </BrowserRouter>
    )

    const img = screen.getAllByRole('img')[0]
    expect(img).toHaveAttribute('src', 'https://example.com/avatar.jpg')
  })

  test('renders favorites count', () => {
    vi.mocked(hooks.useArticleQuery).mockReturnValue({
      data: { article: mockArticle }
    })

    render(
      <BrowserRouter>
        <ArticlePreview article={mockArticle} />
      </BrowserRouter>
    )

    expect(screen.getByText('42')).toBeInTheDocument()
  })

  test('renders all tags', () => {
    vi.mocked(hooks.useArticleQuery).mockReturnValue({
      data: { article: mockArticle }
    })

    render(
      <BrowserRouter>
        <ArticlePreview article={mockArticle} />
      </BrowserRouter>
    )

    expect(screen.getByText('testing')).toBeInTheDocument()
    expect(screen.getByText('javascript')).toBeInTheDocument()
  })

  test('renders Read more link', () => {
    vi.mocked(hooks.useArticleQuery).mockReturnValue({
      data: { article: mockArticle }
    })

    render(
      <BrowserRouter>
        <ArticlePreview article={mockArticle} />
      </BrowserRouter>
    )

    expect(screen.getByText('Read more...')).toBeInTheDocument()
  })

  test('links to article detail page', () => {
    vi.mocked(hooks.useArticleQuery).mockReturnValue({
      data: { article: mockArticle }
    })

    const { container } = render(
      <BrowserRouter>
        <ArticlePreview article={mockArticle} />
      </BrowserRouter>
    )

    const previewLink = container.querySelector('.preview-link')
    expect(previewLink).toHaveAttribute('href', '/article/test-article')
  })

  test('links to author profile', () => {
    vi.mocked(hooks.useArticleQuery).mockReturnValue({
      data: { article: mockArticle }
    })

    const { container } = render(
      <BrowserRouter>
        <ArticlePreview article={mockArticle} />
      </BrowserRouter>
    )

    const authorLinks = container.querySelectorAll('a[href="/profile/johndoe"]')
    expect(authorLinks.length).toBeGreaterThan(0)
  })

  test('renders date in readable format', () => {
    vi.mocked(hooks.useArticleQuery).mockReturnValue({
      data: { article: mockArticle }
    })

    const { container } = render(
      <BrowserRouter>
        <ArticlePreview article={mockArticle} />
      </BrowserRouter>
    )

    const date = container.querySelector('.date')
    expect(date.textContent).toMatch(/\w+ \w+ \d+ \d+/)
  })

  test('applies correct CSS classes to article container', () => {
    vi.mocked(hooks.useArticleQuery).mockReturnValue({
      data: { article: mockArticle }
    })

    const { container } = render(
      <BrowserRouter>
        <ArticlePreview article={mockArticle} />
      </BrowserRouter>
    )

    expect(container.querySelector('.article-preview')).toBeInTheDocument()
  })

  test('renders tags with correct CSS classes', () => {
    vi.mocked(hooks.useArticleQuery).mockReturnValue({
      data: { article: mockArticle }
    })

    const { container } = render(
      <BrowserRouter>
        <ArticlePreview article={mockArticle} />
      </BrowserRouter>
    )

    const tags = container.querySelectorAll('.tag-default.tag-pill.tag-outline')
    expect(tags.length).toBe(2)
  })

  test('handles article with no tags', () => {
    const articleWithNoTags = { ...mockArticle, tagList: [] }
    vi.mocked(hooks.useArticleQuery).mockReturnValue({
      data: { article: articleWithNoTags }
    })

    const { container } = render(
      <BrowserRouter>
        <ArticlePreview article={articleWithNoTags} />
      </BrowserRouter>
    )

    const tagList = container.querySelector('.tag-list')
    expect(tagList.children.length).toBe(0)
  })
})
