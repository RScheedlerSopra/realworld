import { describe, test, expect, vi, beforeEach } from 'vitest'
import { render, screen } from '@testing-library/react'
import { MemoryRouter, Routes, Route } from 'react-router-dom'
import '@testing-library/jest-dom'
import GuestRoute from './GuestRoute'
import * as hooks from '../hooks'

vi.mock('../hooks', () => ({
  useAuth: vi.fn()
}))

describe('GuestRoute Component', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  test('redirects to home when authenticated', () => {
    vi.mocked(hooks.useAuth).mockReturnValue({
      isAuth: true
    })

    render(
      <MemoryRouter initialEntries={['/login']}>
        <Routes>
          <Route path="/" element={<div>Home Page</div>} />
          <GuestRoute path="/login" element={<div>Login Form</div>} />
        </Routes>
      </MemoryRouter>
    )

    // Should redirect to home, so login form should not be visible
    expect(screen.queryByText('Login Form')).not.toBeInTheDocument()
    expect(screen.getByText('Home Page')).toBeInTheDocument()
  })

  test('renders route content when not authenticated', () => {
    vi.mocked(hooks.useAuth).mockReturnValue({
      isAuth: false
    })

    const TestComponent = () => <div>Guest Content</div>

    render(
      <MemoryRouter initialEntries={['/login']}>
        <Routes>
          <Route path="/" element={<div>Home Page</div>} />
          <GuestRoute path="/login" element={<TestComponent />} />
        </Routes>
      </MemoryRouter>
    )

    // When not authenticated, the guest content should be rendered
    expect(screen.getByText('Guest Content')).toBeInTheDocument()
    expect(hooks.useAuth).toHaveBeenCalled()
  })

  test('calls useAuth hook', () => {
    vi.mocked(hooks.useAuth).mockReturnValue({
      isAuth: false
    })

    render(
      <MemoryRouter initialEntries={['/login']}>
        <Routes>
          <Route path="/" element={<div>Home</div>} />
          <GuestRoute path="/login" element={<div>Content</div>} />
        </Routes>
      </MemoryRouter>
    )

    expect(hooks.useAuth).toHaveBeenCalledTimes(1)
  })

  test('passes props to Route component when not authenticated', () => {
    vi.mocked(hooks.useAuth).mockReturnValue({
      isAuth: false
    })

    const element = <div>Test Element</div>

    render(
      <MemoryRouter initialEntries={['/test']}>
        <Routes>
          <Route path="/" element={<div>Home</div>} />
          <GuestRoute path="/test" element={element} />
        </Routes>
      </MemoryRouter>
    )

    // Verify that useAuth was called and content is accessible
    expect(hooks.useAuth).toHaveBeenCalled()
    expect(screen.getByText('Test Element')).toBeInTheDocument()
  })
})
