import { describe, test, expect, vi, beforeEach } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'
import { BrowserRouter } from 'react-router-dom'
import '@testing-library/jest-dom'
import ArticleComment from './ArticleComment'
import * as hooks from '../hooks'

vi.mock('../hooks', () => ({
  useArticleCommentQuery: vi.fn(),
  useAuth: vi.fn(),
  useDeleteCommentMutation: vi.fn()
}))

describe('ArticleComment Component', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  const mockComment = {
    id: 1,
    body: 'This is a great article!',
    createdAt: '2024-01-15T10:00:00.000Z',
    author: {
      username: 'johndoe',
      image: 'https://example.com/avatar.jpg'
    }
  }

  test('renders comment body', () => {
    vi.mocked(hooks.useArticleCommentQuery).mockReturnValue({
      data: { comment: mockComment }
    })
    vi.mocked(hooks.useAuth).mockReturnValue({
      authUser: null
    })
    vi.mocked(hooks.useDeleteCommentMutation).mockReturnValue({
      mutate: vi.fn()
    })

    render(
      <BrowserRouter>
        <ArticleComment comment={mockComment} />
      </BrowserRouter>
    )

    expect(screen.getByText('This is a great article!')).toBeInTheDocument()
  })

  test('renders author username', () => {
    vi.mocked(hooks.useArticleCommentQuery).mockReturnValue({
      data: { comment: mockComment }
    })
    vi.mocked(hooks.useAuth).mockReturnValue({
      authUser: null
    })
    vi.mocked(hooks.useDeleteCommentMutation).mockReturnValue({
      mutate: vi.fn()
    })

    render(
      <BrowserRouter>
        <ArticleComment comment={mockComment} />
      </BrowserRouter>
    )

    expect(screen.getByText('johndoe')).toBeInTheDocument()
  })

  test('renders author image', () => {
    vi.mocked(hooks.useArticleCommentQuery).mockReturnValue({
      data: { comment: mockComment }
    })
    vi.mocked(hooks.useAuth).mockReturnValue({
      authUser: null
    })
    vi.mocked(hooks.useDeleteCommentMutation).mockReturnValue({
      mutate: vi.fn()
    })

    render(
      <BrowserRouter>
        <ArticleComment comment={mockComment} />
      </BrowserRouter>
    )

    const img = screen.getByRole('img')
    expect(img).toHaveAttribute('src', 'https://example.com/avatar.jpg')
    expect(img).toHaveClass('comment-author-img')
  })

  test('renders comment date', () => {
    vi.mocked(hooks.useArticleCommentQuery).mockReturnValue({
      data: { comment: mockComment }
    })
    vi.mocked(hooks.useAuth).mockReturnValue({
      authUser: null
    })
    vi.mocked(hooks.useDeleteCommentMutation).mockReturnValue({
      mutate: vi.fn()
    })

    const { container } = render(
      <BrowserRouter>
        <ArticleComment comment={mockComment} />
      </BrowserRouter>
    )

    const date = container.querySelector('.date-posted')
    expect(date.textContent).toMatch(/\w+ \w+ \d+ \d+/)
  })

  test('links to author profile', () => {
    vi.mocked(hooks.useArticleCommentQuery).mockReturnValue({
      data: { comment: mockComment }
    })
    vi.mocked(hooks.useAuth).mockReturnValue({
      authUser: null
    })
    vi.mocked(hooks.useDeleteCommentMutation).mockReturnValue({
      mutate: vi.fn()
    })

    const { container } = render(
      <BrowserRouter>
        <ArticleComment comment={mockComment} />
      </BrowserRouter>
    )

    const authorLinks = container.querySelectorAll('a[href="/profile/johndoe"]')
    expect(authorLinks.length).toBeGreaterThan(0)
  })

  test('shows delete button for comment author', () => {
    vi.mocked(hooks.useArticleCommentQuery).mockReturnValue({
      data: { comment: mockComment }
    })
    vi.mocked(hooks.useAuth).mockReturnValue({
      authUser: { username: 'johndoe' }
    })
    vi.mocked(hooks.useDeleteCommentMutation).mockReturnValue({
      mutate: vi.fn()
    })

    const { container } = render(
      <BrowserRouter>
        <ArticleComment comment={mockComment} />
      </BrowserRouter>
    )

    const deleteIcon = container.querySelector('.ion-trash-a')
    expect(deleteIcon).toBeInTheDocument()
  })

  test('does not show delete button for other users', () => {
    vi.mocked(hooks.useArticleCommentQuery).mockReturnValue({
      data: { comment: mockComment }
    })
    vi.mocked(hooks.useAuth).mockReturnValue({
      authUser: { username: 'differentuser' }
    })
    vi.mocked(hooks.useDeleteCommentMutation).mockReturnValue({
      mutate: vi.fn()
    })

    const { container } = render(
      <BrowserRouter>
        <ArticleComment comment={mockComment} />
      </BrowserRouter>
    )

    const deleteIcon = container.querySelector('.ion-trash-a')
    expect(deleteIcon).not.toBeInTheDocument()
  })

  test('calls mutate when delete icon is clicked', () => {
    const mutate = vi.fn()
    vi.mocked(hooks.useArticleCommentQuery).mockReturnValue({
      data: { comment: mockComment }
    })
    vi.mocked(hooks.useAuth).mockReturnValue({
      authUser: { username: 'johndoe' }
    })
    vi.mocked(hooks.useDeleteCommentMutation).mockReturnValue({
      mutate
    })

    const { container } = render(
      <BrowserRouter>
        <ArticleComment comment={mockComment} />
      </BrowserRouter>
    )

    const deleteIcon = container.querySelector('.ion-trash-a')
    fireEvent.click(deleteIcon)

    expect(mutate).toHaveBeenCalledWith({ commentId: 1 })
  })

  test('applies correct CSS classes', () => {
    vi.mocked(hooks.useArticleCommentQuery).mockReturnValue({
      data: { comment: mockComment }
    })
    vi.mocked(hooks.useAuth).mockReturnValue({
      authUser: null
    })
    vi.mocked(hooks.useDeleteCommentMutation).mockReturnValue({
      mutate: vi.fn()
    })

    const { container } = render(
      <BrowserRouter>
        <ArticleComment comment={mockComment} />
      </BrowserRouter>
    )

    expect(container.querySelector('.card')).toBeInTheDocument()
    expect(container.querySelector('.card-block')).toBeInTheDocument()
    expect(container.querySelector('.card-footer')).toBeInTheDocument()
    expect(container.querySelector('.card-text')).toBeInTheDocument()
  })

  test('does not show footer when comment has no id', () => {
    const commentWithoutId = { ...mockComment, id: null }
    vi.mocked(hooks.useArticleCommentQuery).mockReturnValue({
      data: { comment: commentWithoutId }
    })
    vi.mocked(hooks.useAuth).mockReturnValue({
      authUser: null
    })
    vi.mocked(hooks.useDeleteCommentMutation).mockReturnValue({
      mutate: vi.fn()
    })

    const { container } = render(
      <BrowserRouter>
        <ArticleComment comment={commentWithoutId} />
      </BrowserRouter>
    )

    expect(container.querySelector('.card-footer')).not.toBeInTheDocument()
  })
})
