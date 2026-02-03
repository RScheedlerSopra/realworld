import { describe, test, expect, vi, beforeEach } from 'vitest'
import { render, screen } from '@testing-library/react'
import { BrowserRouter } from 'react-router-dom'
import '@testing-library/jest-dom'
import Navbar from './Navbar'
import * as hooks from '../hooks'

vi.mock('../hooks', () => ({
  useAuth: vi.fn()
}))

describe('Navbar Component', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  test('renders conduit brand link', () => {
    vi.mocked(hooks.useAuth).mockReturnValue({
      isAuth: false,
      authUser: null
    })

    render(
      <BrowserRouter>
        <Navbar />
      </BrowserRouter>
    )

    expect(screen.getByText('conduit')).toBeInTheDocument()
  })

  test('renders Home link', () => {
    vi.mocked(hooks.useAuth).mockReturnValue({
      isAuth: false,
      authUser: null
    })

    render(
      <BrowserRouter>
        <Navbar />
      </BrowserRouter>
    )

    expect(screen.getByText('Home')).toBeInTheDocument()
  })

  test('shows Sign up and Sign in links for unauthenticated users', () => {
    vi.mocked(hooks.useAuth).mockReturnValue({
      isAuth: false,
      authUser: null
    })

    render(
      <BrowserRouter>
        <Navbar />
      </BrowserRouter>
    )

    expect(screen.getByText('Sign up')).toBeInTheDocument()
    expect(screen.getByText('Sign in')).toBeInTheDocument()
  })

  test('does not show authenticated links for unauthenticated users', () => {
    vi.mocked(hooks.useAuth).mockReturnValue({
      isAuth: false,
      authUser: null
    })

    render(
      <BrowserRouter>
        <Navbar />
      </BrowserRouter>
    )

    expect(screen.queryByText('New Post')).not.toBeInTheDocument()
    expect(screen.queryByText('Settings')).not.toBeInTheDocument()
  })

  test('shows New Post link for authenticated users', () => {
    vi.mocked(hooks.useAuth).mockReturnValue({
      isAuth: true,
      authUser: { username: 'johndoe', image: 'avatar.jpg' }
    })

    render(
      <BrowserRouter>
        <Navbar />
      </BrowserRouter>
    )

    expect(screen.getByText('New Post')).toBeInTheDocument()
  })

  test('shows Settings link for authenticated users', () => {
    vi.mocked(hooks.useAuth).mockReturnValue({
      isAuth: true,
      authUser: { username: 'johndoe', image: 'avatar.jpg' }
    })

    render(
      <BrowserRouter>
        <Navbar />
      </BrowserRouter>
    )

    expect(screen.getByText('Settings')).toBeInTheDocument()
  })

  test('shows username for authenticated users', () => {
    vi.mocked(hooks.useAuth).mockReturnValue({
      isAuth: true,
      authUser: { username: 'johndoe', image: 'avatar.jpg' }
    })

    render(
      <BrowserRouter>
        <Navbar />
      </BrowserRouter>
    )

    expect(screen.getByText('johndoe')).toBeInTheDocument()
  })

  test('shows user avatar for authenticated users', () => {
    vi.mocked(hooks.useAuth).mockReturnValue({
      isAuth: true,
      authUser: { username: 'johndoe', image: 'https://example.com/avatar.jpg' }
    })

    render(
      <BrowserRouter>
        <Navbar />
      </BrowserRouter>
    )

    const avatar = screen.getByRole('img')
    expect(avatar).toHaveAttribute('src', 'https://example.com/avatar.jpg')
  })

  test('does not show Sign up and Sign in links for authenticated users', () => {
    vi.mocked(hooks.useAuth).mockReturnValue({
      isAuth: true,
      authUser: { username: 'johndoe', image: 'avatar.jpg' }
    })

    render(
      <BrowserRouter>
        <Navbar />
      </BrowserRouter>
    )

    expect(screen.queryByText('Sign up')).not.toBeInTheDocument()
    expect(screen.queryByText('Sign in')).not.toBeInTheDocument()
  })

  test('brand link points to home page', () => {
    vi.mocked(hooks.useAuth).mockReturnValue({
      isAuth: false,
      authUser: null
    })

    const { container } = render(
      <BrowserRouter>
        <Navbar />
      </BrowserRouter>
    )

    const brandLink = container.querySelector('.navbar-brand')
    expect(brandLink).toHaveAttribute('href', '/')
  })

  test('New Post link points to editor', () => {
    vi.mocked(hooks.useAuth).mockReturnValue({
      isAuth: true,
      authUser: { username: 'johndoe', image: 'avatar.jpg' }
    })

    const { container } = render(
      <BrowserRouter>
        <Navbar />
      </BrowserRouter>
    )

    const newPostLink = container.querySelector('a[href="/editor"]')
    expect(newPostLink).toBeInTheDocument()
  })

  test('Settings link points to settings page', () => {
    vi.mocked(hooks.useAuth).mockReturnValue({
      isAuth: true,
      authUser: { username: 'johndoe', image: 'avatar.jpg' }
    })

    const { container } = render(
      <BrowserRouter>
        <Navbar />
      </BrowserRouter>
    )

    const settingsLink = container.querySelector('a[href="/settings"]')
    expect(settingsLink).toBeInTheDocument()
  })

  test('username link points to user profile', () => {
    vi.mocked(hooks.useAuth).mockReturnValue({
      isAuth: true,
      authUser: { username: 'johndoe', image: 'avatar.jpg' }
    })

    const { container } = render(
      <BrowserRouter>
        <Navbar />
      </BrowserRouter>
    )

    const profileLink = container.querySelector('a[href="/@johndoe"]')
    expect(profileLink).toBeInTheDocument()
  })

  test('Sign up link points to register page', () => {
    vi.mocked(hooks.useAuth).mockReturnValue({
      isAuth: false,
      authUser: null
    })

    const { container } = render(
      <BrowserRouter>
        <Navbar />
      </BrowserRouter>
    )

    const signUpLink = container.querySelector('a[href="/register"]')
    expect(signUpLink).toBeInTheDocument()
  })

  test('Sign in link points to login page', () => {
    vi.mocked(hooks.useAuth).mockReturnValue({
      isAuth: false,
      authUser: null
    })

    const { container } = render(
      <BrowserRouter>
        <Navbar />
      </BrowserRouter>
    )

    const signInLink = container.querySelector('a[href="/login"]')
    expect(signInLink).toBeInTheDocument()
  })

  test('renders icons for New Post and Settings', () => {
    vi.mocked(hooks.useAuth).mockReturnValue({
      isAuth: true,
      authUser: { username: 'johndoe', image: 'avatar.jpg' }
    })

    const { container } = render(
      <BrowserRouter>
        <Navbar />
      </BrowserRouter>
    )

    expect(container.querySelector('.ion-compose')).toBeInTheDocument()
    expect(container.querySelector('.ion-gear-a')).toBeInTheDocument()
  })

  test('applies correct CSS classes', () => {
    vi.mocked(hooks.useAuth).mockReturnValue({
      isAuth: false,
      authUser: null
    })

    const { container } = render(
      <BrowserRouter>
        <Navbar />
      </BrowserRouter>
    )

    expect(container.querySelector('.navbar.navbar-light')).toBeInTheDocument()
    expect(container.querySelector('.nav.navbar-nav.pull-xs-right')).toBeInTheDocument()
  })
})
