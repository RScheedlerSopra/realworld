import { describe, test, expect, vi, beforeEach } from 'vitest'
import { render, screen } from '@testing-library/react'
import { MemoryRouter, Routes, Route } from 'react-router-dom'
import '@testing-library/jest-dom'
import AuthRoute from './AuthRoute'
import * as hooks from '../hooks'

vi.mock('../hooks', () => ({
  useAuth: vi.fn()
}))

describe('AuthRoute Component', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  test('redirects to home when not authenticated', () => {
    vi.mocked(hooks.useAuth).mockReturnValue({
      isAuth: false
    })

    render(
      <MemoryRouter initialEntries={['/protected']}>
        <Routes>
          <Route path="/" element={<div>Home Page</div>} />
          <AuthRoute path="/protected" element={<div>Protected Content</div>} />
        </Routes>
      </MemoryRouter>
    )

    // Should redirect to home, so protected content should not be visible
    expect(screen.queryByText('Protected Content')).not.toBeInTheDocument()
    expect(screen.getByText('Home Page')).toBeInTheDocument()
  })

  test('renders route content when authenticated', () => {
    vi.mocked(hooks.useAuth).mockReturnValue({
      isAuth: true
    })

    const TestComponent = () => <div>Protected Content</div>

    render(
      <MemoryRouter initialEntries={['/protected']}>
        <Routes>
          <Route path="/" element={<div>Home Page</div>} />
          <AuthRoute path="/protected" element={<TestComponent />} />
        </Routes>
      </MemoryRouter>
    )

    // When authenticated, the protected content should be rendered
    expect(screen.getByText('Protected Content')).toBeInTheDocument()
    expect(hooks.useAuth).toHaveBeenCalled()
  })

  test('calls useAuth hook', () => {
    vi.mocked(hooks.useAuth).mockReturnValue({
      isAuth: true
    })

    render(
      <MemoryRouter initialEntries={['/protected']}>
        <Routes>
          <Route path="/" element={<div>Home</div>} />
          <AuthRoute path="/protected" element={<div>Content</div>} />
        </Routes>
      </MemoryRouter>
    )

    expect(hooks.useAuth).toHaveBeenCalledTimes(1)
  })

  test('passes props to Route component when authenticated', () => {
    vi.mocked(hooks.useAuth).mockReturnValue({
      isAuth: true
    })

    const element = <div>Test Element</div>

    render(
      <MemoryRouter initialEntries={['/test']}>
        <Routes>
          <Route path="/" element={<div>Home</div>} />
          <AuthRoute path="/test" element={element} />
        </Routes>
      </MemoryRouter>
    )

    // Verify that useAuth was called and content is accessible
    expect(hooks.useAuth).toHaveBeenCalled()
    expect(screen.getByText('Test Element')).toBeInTheDocument()
  })
})
