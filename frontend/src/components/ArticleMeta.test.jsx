import { describe, test, expect, vi, beforeEach } from 'vitest'
import { render, screen } from '@testing-library/react'
import { BrowserRouter } from 'react-router-dom'
import '@testing-library/jest-dom'
import ArticleMeta from './ArticleMeta'
import * as hooks from '../hooks'

vi.mock('../hooks', () => ({
  useArticleQuery: vi.fn(),
  useAuth: vi.fn()
}))

vi.mock('./DeleteArticleButton', () => ({
  default: () => <button>Delete Article</button>
}))

vi.mock('./FavoriteArticleButton', () => ({
  default: ({ children, slug, favorited }) => (
    <button data-slug={slug} data-favorited={favorited}>
      {children}
    </button>
  )
}))

vi.mock('./FollowAuthorButton', () => ({
  default: () => <button>Follow Author</button>
}))

describe('ArticleMeta Component', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  const mockArticle = {
    slug: 'test-article',
    createdAt: '2024-01-15T10:00:00.000Z',
    favorited: false,
    favoritesCount: 42,
    author: {
      username: 'johndoe',
      image: 'https://example.com/avatar.jpg'
    }
  }

  test('renders author username', () => {
    vi.mocked(hooks.useArticleQuery).mockReturnValue({
      data: { article: mockArticle }
    })
    vi.mocked(hooks.useAuth).mockReturnValue({
      authUser: null
    })

    render(
      <BrowserRouter>
        <ArticleMeta />
      </BrowserRouter>
    )

    expect(screen.getByText('johndoe')).toBeInTheDocument()
  })

  test('renders author image', () => {
    vi.mocked(hooks.useArticleQuery).mockReturnValue({
      data: { article: mockArticle }
    })
    vi.mocked(hooks.useAuth).mockReturnValue({
      authUser: null
    })

    render(
      <BrowserRouter>
        <ArticleMeta />
      </BrowserRouter>
    )

    const img = screen.getAllByRole('img')[0]
    expect(img).toHaveAttribute('src', 'https://example.com/avatar.jpg')
  })

  test('renders article date', () => {
    vi.mocked(hooks.useArticleQuery).mockReturnValue({
      data: { article: mockArticle }
    })
    vi.mocked(hooks.useAuth).mockReturnValue({
      authUser: null
    })

    const { container } = render(
      <BrowserRouter>
        <ArticleMeta />
      </BrowserRouter>
    )

    const date = container.querySelector('.date')
    expect(date.textContent).toMatch(/\w+ \w+ \d+ \d+/)
  })

  test('links to author profile', () => {
    vi.mocked(hooks.useArticleQuery).mockReturnValue({
      data: { article: mockArticle }
    })
    vi.mocked(hooks.useAuth).mockReturnValue({
      authUser: null
    })

    const { container } = render(
      <BrowserRouter>
        <ArticleMeta />
      </BrowserRouter>
    )

    const authorLinks = container.querySelectorAll('a[href="/profile/johndoe"]')
    expect(authorLinks.length).toBeGreaterThan(0)
  })

  test('shows Follow and Favorite buttons for other users articles', () => {
    vi.mocked(hooks.useArticleQuery).mockReturnValue({
      data: { article: mockArticle }
    })
    vi.mocked(hooks.useAuth).mockReturnValue({
      authUser: { username: 'differentuser' }
    })

    render(
      <BrowserRouter>
        <ArticleMeta />
      </BrowserRouter>
    )

    expect(screen.getByText('Follow Author')).toBeInTheDocument()
    expect(screen.getByText(/Favorite Article/)).toBeInTheDocument()
  })

  test('shows Edit and Delete buttons for own articles', () => {
    vi.mocked(hooks.useArticleQuery).mockReturnValue({
      data: { article: mockArticle }
    })
    vi.mocked(hooks.useAuth).mockReturnValue({
      authUser: { username: 'johndoe' }
    })

    render(
      <BrowserRouter>
        <ArticleMeta />
      </BrowserRouter>
    )

    expect(screen.getByText(/Edit Article/)).toBeInTheDocument()
    expect(screen.getByText('Delete Article')).toBeInTheDocument()
  })

  test('does not show Follow button for own articles', () => {
    vi.mocked(hooks.useArticleQuery).mockReturnValue({
      data: { article: mockArticle }
    })
    vi.mocked(hooks.useAuth).mockReturnValue({
      authUser: { username: 'johndoe' }
    })

    render(
      <BrowserRouter>
        <ArticleMeta />
      </BrowserRouter>
    )

    expect(screen.queryByText('Follow Author')).not.toBeInTheDocument()
  })

  test('does not show Delete button for other users articles', () => {
    vi.mocked(hooks.useArticleQuery).mockReturnValue({
      data: { article: mockArticle }
    })
    vi.mocked(hooks.useAuth).mockReturnValue({
      authUser: { username: 'differentuser' }
    })

    render(
      <BrowserRouter>
        <ArticleMeta />
      </BrowserRouter>
    )

    expect(screen.queryByText('Delete Article')).not.toBeInTheDocument()
  })

  test('Edit link points to correct editor route', () => {
    vi.mocked(hooks.useArticleQuery).mockReturnValue({
      data: { article: mockArticle }
    })
    vi.mocked(hooks.useAuth).mockReturnValue({
      authUser: { username: 'johndoe' }
    })

    const { container } = render(
      <BrowserRouter>
        <ArticleMeta />
      </BrowserRouter>
    )

    const editLink = container.querySelector('a[href="/editor/test-article"]')
    expect(editLink).toBeInTheDocument()
  })

  test('displays Favorite text when not favorited', () => {
    vi.mocked(hooks.useArticleQuery).mockReturnValue({
      data: { article: { ...mockArticle, favorited: false } }
    })
    vi.mocked(hooks.useAuth).mockReturnValue({
      authUser: { username: 'differentuser' }
    })

    render(
      <BrowserRouter>
        <ArticleMeta />
      </BrowserRouter>
    )

    expect(screen.getByText(/Favorite Article/)).toBeInTheDocument()
  })

  test('displays Unfavorite text when favorited', () => {
    vi.mocked(hooks.useArticleQuery).mockReturnValue({
      data: { article: { ...mockArticle, favorited: true } }
    })
    vi.mocked(hooks.useAuth).mockReturnValue({
      authUser: { username: 'differentuser' }
    })

    render(
      <BrowserRouter>
        <ArticleMeta />
      </BrowserRouter>
    )

    expect(screen.getByText(/Unfavorite Article/)).toBeInTheDocument()
  })

  test('displays favorites count', () => {
    vi.mocked(hooks.useArticleQuery).mockReturnValue({
      data: { article: mockArticle }
    })
    vi.mocked(hooks.useAuth).mockReturnValue({
      authUser: { username: 'differentuser' }
    })

    render(
      <BrowserRouter>
        <ArticleMeta />
      </BrowserRouter>
    )

    expect(screen.getByText('(42)')).toBeInTheDocument()
  })

  test('renders edit icon', () => {
    vi.mocked(hooks.useArticleQuery).mockReturnValue({
      data: { article: mockArticle }
    })
    vi.mocked(hooks.useAuth).mockReturnValue({
      authUser: { username: 'johndoe' }
    })

    const { container } = render(
      <BrowserRouter>
        <ArticleMeta />
      </BrowserRouter>
    )

    const editIcon = container.querySelector('.ion-edit')
    expect(editIcon).toBeInTheDocument()
  })

  test('handles unauthenticated user', () => {
    vi.mocked(hooks.useArticleQuery).mockReturnValue({
      data: { article: mockArticle }
    })
    vi.mocked(hooks.useAuth).mockReturnValue({
      authUser: null
    })

    render(
      <BrowserRouter>
        <ArticleMeta />
      </BrowserRouter>
    )

    // Should show Follow/Favorite buttons for unauthenticated users
    expect(screen.getByText('Follow Author')).toBeInTheDocument()
    expect(screen.getByText(/Favorite Article/)).toBeInTheDocument()
  })
})
