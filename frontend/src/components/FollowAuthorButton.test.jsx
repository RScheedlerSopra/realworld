import { describe, test, expect, vi, beforeEach } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'
import { BrowserRouter } from 'react-router-dom'
import { QueryClient, QueryClientProvider } from 'react-query'
import '@testing-library/jest-dom'
import FollowAuthorButton from './FollowAuthorButton'
import * as hooks from '../hooks'

vi.mock('../hooks', () => ({
  useArticleQuery: vi.fn(),
  useAuth: vi.fn(),
  useFollowAuthorMutation: vi.fn()
}))

vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual('react-router-dom')
  return {
    ...actual,
    useParams: vi.fn(),
    useNavigate: vi.fn()
  }
})

vi.mock('./FollowButton', () => ({
  default: ({ disabled, following, onClick, username }) => (
    <button 
      disabled={disabled} 
      onClick={onClick}
      data-following={following}
      data-username={username}
    >
      {following ? 'Unfollow' : 'Follow'} {username}
    </button>
  )
}))

import { useParams, useNavigate } from 'react-router-dom'

describe('FollowAuthorButton Component', () => {
  let queryClient

  beforeEach(() => {
    vi.clearAllMocks()
    queryClient = new QueryClient()
  })

  const mockArticle = {
    slug: 'test-article',
    author: {
      username: 'johndoe',
      following: false
    }
  }

  test('renders FollowButton with correct props', () => {
    vi.mocked(useParams).mockReturnValue({ slug: 'test-article' })
    vi.mocked(useNavigate).mockReturnValue(vi.fn())
    vi.mocked(hooks.useArticleQuery).mockReturnValue({
      data: { article: mockArticle }
    })
    vi.mocked(hooks.useAuth).mockReturnValue({
      isAuth: true
    })
    vi.mocked(hooks.useFollowAuthorMutation).mockReturnValue({
      mutate: vi.fn(),
      isLoading: false
    })

    render(
      <QueryClientProvider client={queryClient}>
        <BrowserRouter>
          <FollowAuthorButton />
        </BrowserRouter>
      </QueryClientProvider>
    )

    expect(screen.getByText(/Follow johndoe/)).toBeInTheDocument()
  })

  test('shows Unfollow when author is followed', () => {
    const followedArticle = {
      ...mockArticle,
      author: { ...mockArticle.author, following: true }
    }

    vi.mocked(useParams).mockReturnValue({ slug: 'test-article' })
    vi.mocked(useNavigate).mockReturnValue(vi.fn())
    vi.mocked(hooks.useArticleQuery).mockReturnValue({
      data: { article: followedArticle }
    })
    vi.mocked(hooks.useAuth).mockReturnValue({
      isAuth: true
    })
    vi.mocked(hooks.useFollowAuthorMutation).mockReturnValue({
      mutate: vi.fn(),
      isLoading: false
    })

    render(
      <QueryClientProvider client={queryClient}>
        <BrowserRouter>
          <FollowAuthorButton />
        </BrowserRouter>
      </QueryClientProvider>
    )

    expect(screen.getByText(/Unfollow johndoe/)).toBeInTheDocument()
  })

  test('calls mutate when button is clicked', () => {
    const mutate = vi.fn()
    vi.mocked(useParams).mockReturnValue({ slug: 'test-article' })
    vi.mocked(useNavigate).mockReturnValue(vi.fn())
    vi.mocked(hooks.useArticleQuery).mockReturnValue({
      data: { article: mockArticle }
    })
    vi.mocked(hooks.useAuth).mockReturnValue({
      isAuth: true
    })
    vi.mocked(hooks.useFollowAuthorMutation).mockReturnValue({
      mutate,
      isLoading: false
    })

    render(
      <QueryClientProvider client={queryClient}>
        <BrowserRouter>
          <FollowAuthorButton />
        </BrowserRouter>
      </QueryClientProvider>
    )

    fireEvent.click(screen.getByRole('button'))

    expect(mutate).toHaveBeenCalledWith({
      following: false,
      username: 'johndoe'
    })
  })

  test('disables button when loading', () => {
    vi.mocked(useParams).mockReturnValue({ slug: 'test-article' })
    vi.mocked(useNavigate).mockReturnValue(vi.fn())
    vi.mocked(hooks.useArticleQuery).mockReturnValue({
      data: { article: mockArticle }
    })
    vi.mocked(hooks.useAuth).mockReturnValue({
      isAuth: true
    })
    vi.mocked(hooks.useFollowAuthorMutation).mockReturnValue({
      mutate: vi.fn(),
      isLoading: true
    })

    render(
      <QueryClientProvider client={queryClient}>
        <BrowserRouter>
          <FollowAuthorButton />
        </BrowserRouter>
      </QueryClientProvider>
    )

    const button = screen.getByRole('button')
    expect(button).toBeDisabled()
  })

  test('does not disable button when not loading', () => {
    vi.mocked(useParams).mockReturnValue({ slug: 'test-article' })
    vi.mocked(useNavigate).mockReturnValue(vi.fn())
    vi.mocked(hooks.useArticleQuery).mockReturnValue({
      data: { article: mockArticle }
    })
    vi.mocked(hooks.useAuth).mockReturnValue({
      isAuth: true
    })
    vi.mocked(hooks.useFollowAuthorMutation).mockReturnValue({
      mutate: vi.fn(),
      isLoading: false
    })

    render(
      <QueryClientProvider client={queryClient}>
        <BrowserRouter>
          <FollowAuthorButton />
        </BrowserRouter>
      </QueryClientProvider>
    )

    const button = screen.getByRole('button')
    expect(button).not.toBeDisabled()
  })
})
