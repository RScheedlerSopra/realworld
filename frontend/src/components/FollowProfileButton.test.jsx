import { describe, test, expect, vi, beforeEach } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'
import { BrowserRouter } from 'react-router-dom'
import { QueryClient, QueryClientProvider } from 'react-query'
import '@testing-library/jest-dom'
import FollowProfileButton from './FollowProfileButton'
import * as hooks from '../hooks'

vi.mock('../hooks', () => ({
  useProfileQuery: vi.fn(),
  useAuth: vi.fn(),
  useFollowAuthorMutation: vi.fn()
}))

vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual('react-router-dom')
  return {
    ...actual,
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

import { useNavigate } from 'react-router-dom'

describe('FollowProfileButton Component', () => {
  let queryClient

  beforeEach(() => {
    vi.clearAllMocks()
    queryClient = new QueryClient()
  })

  const mockProfile = {
    username: 'johndoe',
    following: false
  }

  test('renders FollowButton with correct props', () => {
    vi.mocked(useNavigate).mockReturnValue(vi.fn())
    vi.mocked(hooks.useProfileQuery).mockReturnValue({
      data: { profile: mockProfile }
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
          <FollowProfileButton />
        </BrowserRouter>
      </QueryClientProvider>
    )

    expect(screen.getByText(/Follow johndoe/)).toBeInTheDocument()
  })

  test('shows Unfollow when profile is followed', () => {
    const followedProfile = {
      ...mockProfile,
      following: true
    }

    vi.mocked(useNavigate).mockReturnValue(vi.fn())
    vi.mocked(hooks.useProfileQuery).mockReturnValue({
      data: { profile: followedProfile }
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
          <FollowProfileButton />
        </BrowserRouter>
      </QueryClientProvider>
    )

    expect(screen.getByText(/Unfollow johndoe/)).toBeInTheDocument()
  })

  test('calls mutate when button is clicked', () => {
    const mutate = vi.fn()
    vi.mocked(useNavigate).mockReturnValue(vi.fn())
    vi.mocked(hooks.useProfileQuery).mockReturnValue({
      data: { profile: mockProfile }
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
          <FollowProfileButton />
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
    vi.mocked(useNavigate).mockReturnValue(vi.fn())
    vi.mocked(hooks.useProfileQuery).mockReturnValue({
      data: { profile: mockProfile }
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
          <FollowProfileButton />
        </BrowserRouter>
      </QueryClientProvider>
    )

    const button = screen.getByRole('button')
    expect(button).toBeDisabled()
  })

  test('does not disable button when not loading', () => {
    vi.mocked(useNavigate).mockReturnValue(vi.fn())
    vi.mocked(hooks.useProfileQuery).mockReturnValue({
      data: { profile: mockProfile }
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
          <FollowProfileButton />
        </BrowserRouter>
      </QueryClientProvider>
    )

    const button = screen.getByRole('button')
    expect(button).not.toBeDisabled()
  })

  test('passes correct username to FollowButton', () => {
    vi.mocked(useNavigate).mockReturnValue(vi.fn())
    vi.mocked(hooks.useProfileQuery).mockReturnValue({
      data: { profile: { username: 'janedoe', following: false } }
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
          <FollowProfileButton />
        </BrowserRouter>
      </QueryClientProvider>
    )

    expect(screen.getByText(/Follow janedoe/)).toBeInTheDocument()
  })
})
