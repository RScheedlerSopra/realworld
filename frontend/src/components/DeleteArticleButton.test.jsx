import { describe, test, expect, vi, beforeEach } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'
import '@testing-library/jest-dom'
import DeleteArticleButton from './DeleteArticleButton'
import * as hooks from '../hooks'

vi.mock('../hooks', () => ({
  useDeleteArticleMutation: vi.fn()
}))

describe('DeleteArticleButton Component', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  test('renders delete button with text', () => {
    vi.mocked(hooks.useDeleteArticleMutation).mockReturnValue({
      mutate: vi.fn(),
      isLoading: false
    })

    render(<DeleteArticleButton />)

    expect(screen.getByText(/Delete Article/)).toBeInTheDocument()
  })

  test('renders trash icon', () => {
    vi.mocked(hooks.useDeleteArticleMutation).mockReturnValue({
      mutate: vi.fn(),
      isLoading: false
    })

    const { container } = render(<DeleteArticleButton />)

    const icon = container.querySelector('.ion-trash-a')
    expect(icon).toBeInTheDocument()
  })

  test('calls mutate when clicked', () => {
    const mutate = vi.fn()
    vi.mocked(hooks.useDeleteArticleMutation).mockReturnValue({
      mutate,
      isLoading: false
    })

    render(<DeleteArticleButton />)

    fireEvent.click(screen.getByRole('button'))

    expect(mutate).toHaveBeenCalledWith()
    expect(mutate).toHaveBeenCalledTimes(1)
  })

  test('disables button when loading', () => {
    vi.mocked(hooks.useDeleteArticleMutation).mockReturnValue({
      mutate: vi.fn(),
      isLoading: true
    })

    render(<DeleteArticleButton />)

    const button = screen.getByRole('button')
    expect(button).toBeDisabled()
  })

  test('does not disable button when not loading', () => {
    vi.mocked(hooks.useDeleteArticleMutation).mockReturnValue({
      mutate: vi.fn(),
      isLoading: false
    })

    render(<DeleteArticleButton />)

    const button = screen.getByRole('button')
    expect(button).not.toBeDisabled()
  })

  test('has correct button type', () => {
    vi.mocked(hooks.useDeleteArticleMutation).mockReturnValue({
      mutate: vi.fn(),
      isLoading: false
    })

    render(<DeleteArticleButton />)

    const button = screen.getByRole('button')
    expect(button).toHaveAttribute('type', 'button')
  })

  test('applies correct CSS classes', () => {
    vi.mocked(hooks.useDeleteArticleMutation).mockReturnValue({
      mutate: vi.fn(),
      isLoading: false
    })

    render(<DeleteArticleButton />)

    const button = screen.getByRole('button')
    expect(button).toHaveClass('btn')
    expect(button).toHaveClass('btn-outline-danger')
    expect(button).toHaveClass('btn-sm')
  })

  test('calls useDeleteArticleMutation hook', () => {
    vi.mocked(hooks.useDeleteArticleMutation).mockReturnValue({
      mutate: vi.fn(),
      isLoading: false
    })

    render(<DeleteArticleButton />)

    expect(hooks.useDeleteArticleMutation).toHaveBeenCalledTimes(1)
  })
})
