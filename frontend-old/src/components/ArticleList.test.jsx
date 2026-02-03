import { describe, test, expect, vi, beforeEach } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'
import { BrowserRouter } from 'react-router-dom'
import '@testing-library/jest-dom'
import ArticleList from './ArticleList'
import * as hooks from '../hooks'

vi.mock('../hooks', () => ({
  useArticlesQuery: vi.fn()
}))

vi.mock('./ArticlePreview', () => ({
  default: ({ article }) => (
    <div data-testid="article-preview" data-slug={article.slug}>
      {article.title}
    </div>
  )
}))

describe('ArticleList Component', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  const mockArticles = [
    { slug: 'article-1', title: 'Article 1' },
    { slug: 'article-2', title: 'Article 2' },
    { slug: 'article-3', title: 'Article 3' }
  ]

  test('displays loading message while fetching', () => {
    vi.mocked(hooks.useArticlesQuery).mockReturnValue({
      data: { articles: [], articlesCount: 0 },
      isFetching: true,
      isError: false,
      isSuccess: false
    })

    render(
      <BrowserRouter>
        <ArticleList />
      </BrowserRouter>
    )

    expect(screen.getByText('Loading articles...')).toBeInTheDocument()
  })

  test('displays error message when fetch fails', () => {
    vi.mocked(hooks.useArticlesQuery).mockReturnValue({
      data: { articles: [], articlesCount: 0 },
      isFetching: false,
      isError: true,
      isSuccess: false
    })

    render(
      <BrowserRouter>
        <ArticleList />
      </BrowserRouter>
    )

    expect(screen.getByText('Loading articles failed :(')).toBeInTheDocument()
  })

  test('displays message when no articles exist', () => {
    vi.mocked(hooks.useArticlesQuery).mockReturnValue({
      data: { articles: [], articlesCount: 0 },
      isFetching: false,
      isError: false,
      isSuccess: true
    })

    render(
      <BrowserRouter>
        <ArticleList />
      </BrowserRouter>
    )

    expect(screen.getByText('No articles are here... yet.')).toBeInTheDocument()
  })

  test('renders article previews', () => {
    vi.mocked(hooks.useArticlesQuery).mockReturnValue({
      data: { articles: mockArticles, articlesCount: 3 },
      isFetching: false,
      isError: false,
      isSuccess: true
    })

    render(
      <BrowserRouter>
        <ArticleList />
      </BrowserRouter>
    )

    expect(screen.getByText('Article 1')).toBeInTheDocument()
    expect(screen.getByText('Article 2')).toBeInTheDocument()
    expect(screen.getByText('Article 3')).toBeInTheDocument()
  })

  test('does not show pagination when articles count is 10 or less', () => {
    vi.mocked(hooks.useArticlesQuery).mockReturnValue({
      data: { articles: mockArticles, articlesCount: 10 },
      isFetching: false,
      isError: false,
      isSuccess: true
    })

    const { container } = render(
      <BrowserRouter>
        <ArticleList />
      </BrowserRouter>
    )

    expect(container.querySelector('.pagination')).not.toBeInTheDocument()
  })

  test('shows pagination when articles count exceeds 10', () => {
    vi.mocked(hooks.useArticlesQuery).mockReturnValue({
      data: { articles: mockArticles, articlesCount: 25 },
      isFetching: false,
      isError: false,
      isSuccess: true
    })

    const { container } = render(
      <BrowserRouter>
        <ArticleList />
      </BrowserRouter>
    )

    expect(container.querySelector('.pagination')).toBeInTheDocument()
  })

  test('calculates correct number of pages', () => {
    vi.mocked(hooks.useArticlesQuery).mockReturnValue({
      data: { articles: mockArticles, articlesCount: 25 },
      isFetching: false,
      isError: false,
      isSuccess: true
    })

    render(
      <BrowserRouter>
        <ArticleList />
      </BrowserRouter>
    )

    // 25 articles / 10 per page = 3 pages
    expect(screen.getByText('1')).toBeInTheDocument()
    expect(screen.getByText('2')).toBeInTheDocument()
    expect(screen.getByText('3')).toBeInTheDocument()
  })

  test('marks first page as active by default', () => {
    vi.mocked(hooks.useArticlesQuery).mockReturnValue({
      data: { articles: mockArticles, articlesCount: 25 },
      isFetching: false,
      isError: false,
      isSuccess: true
    })

    const { container } = render(
      <BrowserRouter>
        <ArticleList />
      </BrowserRouter>
    )

    const activeItem = container.querySelector('.page-item.active')
    expect(activeItem).toBeInTheDocument()
    expect(activeItem.textContent).toBe('1')
  })

  test('changes page when pagination button is clicked', () => {
    const mockQuery = vi.fn().mockReturnValue({
      data: { articles: mockArticles, articlesCount: 25 },
      isFetching: false,
      isError: false,
      isSuccess: true
    })
    vi.mocked(hooks.useArticlesQuery).mockImplementation(mockQuery)

    render(
      <BrowserRouter>
        <ArticleList />
      </BrowserRouter>
    )

    const page2Button = screen.getByText('2')
    fireEvent.click(page2Button)

    // After clicking, the hook should be called with offset: 1
    expect(mockQuery).toHaveBeenCalledWith(
      expect.objectContaining({
        filters: expect.objectContaining({ offset: 1 })
      })
    )
  })

  test('passes filters to useArticlesQuery', () => {
    const mockQuery = vi.fn().mockReturnValue({
      data: { articles: mockArticles, articlesCount: 3 },
      isFetching: false,
      isError: false,
      isSuccess: true
    })
    vi.mocked(hooks.useArticlesQuery).mockImplementation(mockQuery)

    const filters = { tag: 'testing', author: 'johndoe' }

    render(
      <BrowserRouter>
        <ArticleList filters={filters} />
      </BrowserRouter>
    )

    expect(mockQuery).toHaveBeenCalledWith(
      expect.objectContaining({
        filters: expect.objectContaining({
          tag: 'testing',
          author: 'johndoe'
        })
      })
    )
  })

  test('handles exact multiples of 10 articles correctly', () => {
    vi.mocked(hooks.useArticlesQuery).mockReturnValue({
      data: { articles: mockArticles, articlesCount: 20 },
      isFetching: false,
      isError: false,
      isSuccess: true
    })

    render(
      <BrowserRouter>
        <ArticleList />
      </BrowserRouter>
    )

    // 20 articles / 10 per page = exactly 2 pages
    expect(screen.getByText('1')).toBeInTheDocument()
    expect(screen.getByText('2')).toBeInTheDocument()
    expect(screen.queryByText('3')).not.toBeInTheDocument()
  })

  test('applies correct CSS classes to pagination items', () => {
    vi.mocked(hooks.useArticlesQuery).mockReturnValue({
      data: { articles: mockArticles, articlesCount: 25 },
      isFetching: false,
      isError: false,
      isSuccess: true
    })

    const { container } = render(
      <BrowserRouter>
        <ArticleList />
      </BrowserRouter>
    )

    const pageItems = container.querySelectorAll('.page-item')
    expect(pageItems.length).toBe(3)

    const pageLinks = container.querySelectorAll('.page-link')
    expect(pageLinks.length).toBe(3)
  })
})
