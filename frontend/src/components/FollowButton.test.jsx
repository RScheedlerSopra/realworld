import { describe, test, expect, vi } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'
import '@testing-library/jest-dom'
import FollowButton from './FollowButton'

describe('FollowButton Component', () => {
  test('renders button with username', () => {
    render(
      <FollowButton
        disabled={false}
        onClick={vi.fn()}
        following={false}
        username="johndoe"
      />
    )

    expect(screen.getByText(/Follow johndoe/)).toBeInTheDocument()
  })

  test('displays Follow when not following', () => {
    render(
      <FollowButton
        disabled={false}
        onClick={vi.fn()}
        following={false}
        username="johndoe"
      />
    )

    expect(screen.getByText(/Follow johndoe/)).toBeInTheDocument()
  })

  test('displays Unfollow when following', () => {
    render(
      <FollowButton
        disabled={false}
        onClick={vi.fn()}
        following={true}
        username="johndoe"
      />
    )

    expect(screen.getByText(/Unfollow johndoe/)).toBeInTheDocument()
  })

  test('applies outline class when not following', () => {
    render(
      <FollowButton
        disabled={false}
        onClick={vi.fn()}
        following={false}
        username="johndoe"
      />
    )

    const button = screen.getByRole('button')
    expect(button).toHaveClass('btn-outline-secondary')
    expect(button).not.toHaveClass('btn-secondary')
  })

  test('applies solid class when following', () => {
    render(
      <FollowButton
        disabled={false}
        onClick={vi.fn()}
        following={true}
        username="johndoe"
      />
    )

    const button = screen.getByRole('button')
    expect(button).toHaveClass('btn-secondary')
    expect(button).not.toHaveClass('btn-outline-secondary')
  })

  test('calls onClick when button is clicked', () => {
    const onClick = vi.fn()

    render(
      <FollowButton
        disabled={false}
        onClick={onClick}
        following={false}
        username="johndoe"
      />
    )

    fireEvent.click(screen.getByRole('button'))

    expect(onClick).toHaveBeenCalledTimes(1)
  })

  test('disables button when disabled prop is true', () => {
    render(
      <FollowButton
        disabled={true}
        onClick={vi.fn()}
        following={false}
        username="johndoe"
      />
    )

    const button = screen.getByRole('button')
    expect(button).toBeDisabled()
  })

  test('does not disable button when disabled prop is false', () => {
    render(
      <FollowButton
        disabled={false}
        onClick={vi.fn()}
        following={false}
        username="johndoe"
      />
    )

    const button = screen.getByRole('button')
    expect(button).not.toBeDisabled()
  })

  test('renders plus icon', () => {
    const { container } = render(
      <FollowButton
        disabled={false}
        onClick={vi.fn()}
        following={false}
        username="johndoe"
      />
    )

    const icon = container.querySelector('.ion-plus-round')
    expect(icon).toBeInTheDocument()
  })

  test('applies default button classes', () => {
    render(
      <FollowButton
        disabled={false}
        onClick={vi.fn()}
        following={false}
        username="johndoe"
      />
    )

    const button = screen.getByRole('button')
    expect(button).toHaveClass('btn')
    expect(button).toHaveClass('btn-sm')
    expect(button).toHaveClass('action-btn')
  })

  test('has correct button type', () => {
    render(
      <FollowButton
        disabled={false}
        onClick={vi.fn()}
        following={false}
        username="johndoe"
      />
    )

    const button = screen.getByRole('button')
    expect(button).toHaveAttribute('type', 'button')
  })

  test('works with different usernames', () => {
    render(
      <FollowButton
        disabled={false}
        onClick={vi.fn()}
        following={false}
        username="jane_smith_123"
      />
    )

    expect(screen.getByText(/Follow jane_smith_123/)).toBeInTheDocument()
  })
})
