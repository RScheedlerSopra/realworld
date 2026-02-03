import { describe, test, expect, vi, beforeEach } from 'vitest'
import { render, screen, fireEvent, waitFor } from '@testing-library/react'
import { BrowserRouter } from 'react-router-dom'
import '@testing-library/jest-dom'
import ArticleCommentForm from './ArticleCommentForm'
import * as hooks from '../hooks'

vi.mock('../hooks', () => ({
  useArticleQuery: vi.fn(),
  useAddCommentMutation: vi.fn()
}))

describe('ArticleCommentForm Component', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  const mockArticle = {
    author: {
      username: 'johndoe',
      image: 'https://example.com/avatar.jpg'
    }
  }

  test('renders textarea with placeholder', () => {
    vi.mocked(hooks.useArticleQuery).mockReturnValue({
      data: { article: mockArticle }
    })
    vi.mocked(hooks.useAddCommentMutation).mockReturnValue({
      mutateAsync: vi.fn().mockResolvedValue({})
    })

    render(
      <BrowserRouter>
        <ArticleCommentForm />
      </BrowserRouter>
    )

    const textarea = screen.getByPlaceholderText('Write a comment...')
    expect(textarea).toBeInTheDocument()
  })

  test('renders author image', () => {
    vi.mocked(hooks.useArticleQuery).mockReturnValue({
      data: { article: mockArticle }
    })
    vi.mocked(hooks.useAddCommentMutation).mockReturnValue({
      mutateAsync: vi.fn().mockResolvedValue({})
    })

    render(
      <BrowserRouter>
        <ArticleCommentForm />
      </BrowserRouter>
    )

    const img = screen.getByRole('img')
    expect(img).toHaveAttribute('src', 'https://example.com/avatar.jpg')
    expect(img).toHaveClass('comment-author-img')
  })

  test('renders submit button', () => {
    vi.mocked(hooks.useArticleQuery).mockReturnValue({
      data: { article: mockArticle }
    })
    vi.mocked(hooks.useAddCommentMutation).mockReturnValue({
      mutateAsync: vi.fn().mockResolvedValue({})
    })

    render(
      <BrowserRouter>
        <ArticleCommentForm />
      </BrowserRouter>
    )

    expect(screen.getByText('Post Comment')).toBeInTheDocument()
  })

  test('textarea has required attribute', () => {
    vi.mocked(hooks.useArticleQuery).mockReturnValue({
      data: { article: mockArticle }
    })
    vi.mocked(hooks.useAddCommentMutation).mockReturnValue({
      mutateAsync: vi.fn().mockResolvedValue({})
    })

    render(
      <BrowserRouter>
        <ArticleCommentForm />
      </BrowserRouter>
    )

    const textarea = screen.getByPlaceholderText('Write a comment...')
    expect(textarea).toHaveAttribute('required')
  })

  test('textarea has correct rows attribute', () => {
    vi.mocked(hooks.useArticleQuery).mockReturnValue({
      data: { article: mockArticle }
    })
    vi.mocked(hooks.useAddCommentMutation).mockReturnValue({
      mutateAsync: vi.fn().mockResolvedValue({})
    })

    render(
      <BrowserRouter>
        <ArticleCommentForm />
      </BrowserRouter>
    )

    const textarea = screen.getByPlaceholderText('Write a comment...')
    expect(textarea).toHaveAttribute('rows', '3')
  })

  test('submits comment with correct data', async () => {
    const mutateAsync = vi.fn().mockResolvedValue({})
    vi.mocked(hooks.useArticleQuery).mockReturnValue({
      data: { article: mockArticle }
    })
    vi.mocked(hooks.useAddCommentMutation).mockReturnValue({
      mutateAsync
    })

    render(
      <BrowserRouter>
        <ArticleCommentForm />
      </BrowserRouter>
    )

    const textarea = screen.getByPlaceholderText('Write a comment...')
    const submitButton = screen.getByText('Post Comment')

    fireEvent.change(textarea, { target: { value: 'Great article!' } })
    fireEvent.click(submitButton)

    await waitFor(() => {
      expect(mutateAsync).toHaveBeenCalledWith({
        comment: {
          body: 'Great article!'
        }
      })
    })
  })

  test('clears form after successful submission', async () => {
    vi.mocked(hooks.useArticleQuery).mockReturnValue({
      data: { article: mockArticle }
    })
    vi.mocked(hooks.useAddCommentMutation).mockReturnValue({
      mutateAsync: vi.fn().mockResolvedValue({})
    })

    render(
      <BrowserRouter>
        <ArticleCommentForm />
      </BrowserRouter>
    )

    const textarea = screen.getByPlaceholderText('Write a comment...')
    const submitButton = screen.getByText('Post Comment')

    fireEvent.change(textarea, { target: { value: 'Great article!' } })
    fireEvent.click(submitButton)

    await waitFor(() => {
      expect(textarea.value).toBe('')
    })
  })

  test('disables submit button while submitting', async () => {
    vi.mocked(hooks.useArticleQuery).mockReturnValue({
      data: { article: mockArticle }
    })
    vi.mocked(hooks.useAddCommentMutation).mockReturnValue({
      mutateAsync: vi.fn().mockImplementation(() => new Promise(resolve => setTimeout(resolve, 100)))
    })

    render(
      <BrowserRouter>
        <ArticleCommentForm />
      </BrowserRouter>
    )

    const textarea = screen.getByPlaceholderText('Write a comment...')
    const submitButton = screen.getByText('Post Comment')

    fireEvent.change(textarea, { target: { value: 'Great article!' } })
    fireEvent.click(submitButton)

    expect(submitButton).toBeDisabled()
  })

  test('applies correct CSS classes', () => {
    vi.mocked(hooks.useArticleQuery).mockReturnValue({
      data: { article: mockArticle }
    })
    vi.mocked(hooks.useAddCommentMutation).mockReturnValue({
      mutateAsync: vi.fn().mockResolvedValue({})
    })

    const { container } = render(
      <BrowserRouter>
        <ArticleCommentForm />
      </BrowserRouter>
    )

    expect(container.querySelector('.card.comment-form')).toBeInTheDocument()
    expect(container.querySelector('.card-block')).toBeInTheDocument()
    expect(container.querySelector('.card-footer')).toBeInTheDocument()
    expect(container.querySelector('.form-control')).toBeInTheDocument()
  })

  test('submit button has correct CSS classes', () => {
    vi.mocked(hooks.useArticleQuery).mockReturnValue({
      data: { article: mockArticle }
    })
    vi.mocked(hooks.useAddCommentMutation).mockReturnValue({
      mutateAsync: vi.fn().mockResolvedValue({})
    })

    render(
      <BrowserRouter>
        <ArticleCommentForm />
      </BrowserRouter>
    )

    const submitButton = screen.getByText('Post Comment')
    expect(submitButton).toHaveClass('btn')
    expect(submitButton).toHaveClass('btn-sm')
    expect(submitButton).toHaveClass('btn-primary')
  })
})
