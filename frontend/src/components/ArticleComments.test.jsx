import { describe, test, expect, vi, beforeEach } from 'vitest'
import { render, screen } from '@testing-library/react'
import { BrowserRouter } from 'react-router-dom'
import '@testing-library/jest-dom'
import ArticleComments from './ArticleComments'
import * as hooks from '../hooks'

vi.mock('../hooks', () => ({
  useAuth: vi.fn(),
  useArticleCommentsQuery: vi.fn()
}))

vi.mock('./ArticleComment', () => ({
  default: ({ comment }) => (
    <div data-testid="article-comment" data-id={comment.id}>
      {comment.body}
    </div>
  )
}))

vi.mock('./ArticleCommentForm', () => ({
  default: () => <div data-testid="comment-form">Comment Form</div>
}))

describe('ArticleComments Component', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  test('shows sign in message for unauthenticated users', () => {
    vi.mocked(hooks.useAuth).mockReturnValue({
      isAuth: false
    })
    vi.mocked(hooks.useArticleCommentsQuery).mockReturnValue({
      data: { comments: [] }
    })

    render(
      <BrowserRouter>
        <ArticleComments />
      </BrowserRouter>
    )

    expect(screen.getByText(/Sign in/)).toBeInTheDocument()
    expect(screen.getByText(/sign up/)).toBeInTheDocument()
  })

  test('links to login page for unauthenticated users', () => {
    vi.mocked(hooks.useAuth).mockReturnValue({
      isAuth: false
    })
    vi.mocked(hooks.useArticleCommentsQuery).mockReturnValue({
      data: { comments: [] }
    })

    const { container } = render(
      <BrowserRouter>
        <ArticleComments />
      </BrowserRouter>
    )

    const loginLink = container.querySelector('a[href="/login"]')
    expect(loginLink).toBeInTheDocument()
  })

  test('links to register page for unauthenticated users', () => {
    vi.mocked(hooks.useAuth).mockReturnValue({
      isAuth: false
    })
    vi.mocked(hooks.useArticleCommentsQuery).mockReturnValue({
      data: { comments: [] }
    })

    const { container } = render(
      <BrowserRouter>
        <ArticleComments />
      </BrowserRouter>
    )

    const registerLink = container.querySelector('a[href="/register"]')
    expect(registerLink).toBeInTheDocument()
  })

  test('shows comment form for authenticated users', () => {
    vi.mocked(hooks.useAuth).mockReturnValue({
      isAuth: true
    })
    vi.mocked(hooks.useArticleCommentsQuery).mockReturnValue({
      data: { comments: [] }
    })

    render(
      <BrowserRouter>
        <ArticleComments />
      </BrowserRouter>
    )

    expect(screen.getByTestId('comment-form')).toBeInTheDocument()
  })

  test('does not show sign in message for authenticated users', () => {
    vi.mocked(hooks.useAuth).mockReturnValue({
      isAuth: true
    })
    vi.mocked(hooks.useArticleCommentsQuery).mockReturnValue({
      data: { comments: [] }
    })

    render(
      <BrowserRouter>
        <ArticleComments />
      </BrowserRouter>
    )

    expect(screen.queryByText(/Sign in/)).not.toBeInTheDocument()
  })

  test('renders multiple comments', () => {
    const mockComments = [
      { id: 1, body: 'First comment' },
      { id: 2, body: 'Second comment' },
      { id: 3, body: 'Third comment' }
    ]

    vi.mocked(hooks.useAuth).mockReturnValue({
      isAuth: true
    })
    vi.mocked(hooks.useArticleCommentsQuery).mockReturnValue({
      data: { comments: mockComments }
    })

    render(
      <BrowserRouter>
        <ArticleComments />
      </BrowserRouter>
    )

    expect(screen.getByText('First comment')).toBeInTheDocument()
    expect(screen.getByText('Second comment')).toBeInTheDocument()
    expect(screen.getByText('Third comment')).toBeInTheDocument()
  })

  test('renders comments with correct keys', () => {
    const mockComments = [
      { id: 1, body: 'First comment' },
      { id: 2, body: 'Second comment' }
    ]

    vi.mocked(hooks.useAuth).mockReturnValue({
      isAuth: true
    })
    vi.mocked(hooks.useArticleCommentsQuery).mockReturnValue({
      data: { comments: mockComments }
    })

    render(
      <BrowserRouter>
        <ArticleComments />
      </BrowserRouter>
    )

    const comments = screen.getAllByTestId('article-comment')
    expect(comments).toHaveLength(2)
    expect(comments[0]).toHaveAttribute('data-id', '1')
    expect(comments[1]).toHaveAttribute('data-id', '2')
  })

  test('handles empty comments array', () => {
    vi.mocked(hooks.useAuth).mockReturnValue({
      isAuth: true
    })
    vi.mocked(hooks.useArticleCommentsQuery).mockReturnValue({
      data: { comments: [] }
    })

    render(
      <BrowserRouter>
        <ArticleComments />
      </BrowserRouter>
    )

    expect(screen.queryByTestId('article-comment')).not.toBeInTheDocument()
    expect(screen.getByTestId('comment-form')).toBeInTheDocument()
  })

  test('renders comment form before comments list', () => {
    const mockComments = [
      { id: 1, body: 'First comment' }
    ]

    vi.mocked(hooks.useAuth).mockReturnValue({
      isAuth: true
    })
    vi.mocked(hooks.useArticleCommentsQuery).mockReturnValue({
      data: { comments: mockComments }
    })

    const { container } = render(
      <BrowserRouter>
        <ArticleComments />
      </BrowserRouter>
    )

    const form = screen.getByTestId('comment-form')
    const comment = screen.getByTestId('article-comment')
    
    // Check that form appears before comment in the DOM
    expect(container.innerHTML.indexOf('comment-form')).toBeLessThan(
      container.innerHTML.indexOf('article-comment')
    )
  })
})
