import { describe, test, expect, vi, beforeEach } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'
import '@testing-library/jest-dom'
import FavoriteArticleButton from './FavoriteArticleButton'
import * as hooks from '../hooks'

vi.mock('../hooks', () => ({
  useFavoriteArticleMutation: vi.fn()
}))

describe('FavoriteArticleButton Component', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  test('renders button with heart icon', () => {
    vi.mocked(hooks.useFavoriteArticleMutation).mockReturnValue({
      mutate: vi.fn(),
      isLoading: false
    })

    render(
      <FavoriteArticleButton slug="test-article" favorited={false}>
        Favorite Article
      </FavoriteArticleButton>
    )

    const button = screen.getByRole('button')
    expect(button).toBeInTheDocument()
    
    const icon = button.querySelector('.ion-heart')
    expect(icon).toBeInTheDocument()
  })

  test('renders children content', () => {
    vi.mocked(hooks.useFavoriteArticleMutation).mockReturnValue({
      mutate: vi.fn(),
      isLoading: false
    })

    render(
      <FavoriteArticleButton slug="test-article" favorited={false}>
        Favorite Post (42)
      </FavoriteArticleButton>
    )

    expect(screen.getByText(/Favorite Post \(42\)/)).toBeInTheDocument()
  })

  test('applies outline class when not favorited', () => {
    vi.mocked(hooks.useFavoriteArticleMutation).mockReturnValue({
      mutate: vi.fn(),
      isLoading: false
    })

    render(
      <FavoriteArticleButton slug="test-article" favorited={false}>
        Favorite
      </FavoriteArticleButton>
    )

    const button = screen.getByRole('button')
    expect(button).toHaveClass('btn-outline-primary')
    expect(button).not.toHaveClass('btn-primary')
  })

  test('applies solid class when favorited', () => {
    vi.mocked(hooks.useFavoriteArticleMutation).mockReturnValue({
      mutate: vi.fn(),
      isLoading: false
    })

    render(
      <FavoriteArticleButton slug="test-article" favorited={true}>
        Unfavorite
      </FavoriteArticleButton>
    )

    const button = screen.getByRole('button')
    expect(button).toHaveClass('btn-primary')
    expect(button).not.toHaveClass('btn-outline-primary')
  })

  test('calls mutate with favorited status when clicked', () => {
    const mutate = vi.fn()
    vi.mocked(hooks.useFavoriteArticleMutation).mockReturnValue({
      mutate,
      isLoading: false
    })

    render(
      <FavoriteArticleButton slug="test-article" favorited={false}>
        Favorite
      </FavoriteArticleButton>
    )

    fireEvent.click(screen.getByRole('button'))

    expect(mutate).toHaveBeenCalledWith({ favorited: false })
    expect(mutate).toHaveBeenCalledTimes(1)
  })

  test('passes slug to useFavoriteArticleMutation hook', () => {
    vi.mocked(hooks.useFavoriteArticleMutation).mockReturnValue({
      mutate: vi.fn(),
      isLoading: false
    })

    render(
      <FavoriteArticleButton slug="my-test-article" favorited={false}>
        Favorite
      </FavoriteArticleButton>
    )

    expect(hooks.useFavoriteArticleMutation).toHaveBeenCalledWith('my-test-article')
  })

  test('disables button when loading', () => {
    vi.mocked(hooks.useFavoriteArticleMutation).mockReturnValue({
      mutate: vi.fn(),
      isLoading: true
    })

    render(
      <FavoriteArticleButton slug="test-article" favorited={false}>
        Favorite
      </FavoriteArticleButton>
    )

    const button = screen.getByRole('button')
    expect(button).toBeDisabled()
  })

  test('does not disable button when not loading', () => {
    vi.mocked(hooks.useFavoriteArticleMutation).mockReturnValue({
      mutate: vi.fn(),
      isLoading: false
    })

    render(
      <FavoriteArticleButton slug="test-article" favorited={false}>
        Favorite
      </FavoriteArticleButton>
    )

    const button = screen.getByRole('button')
    expect(button).not.toBeDisabled()
  })

  test('applies custom className', () => {
    vi.mocked(hooks.useFavoriteArticleMutation).mockReturnValue({
      mutate: vi.fn(),
      isLoading: false
    })

    render(
      <FavoriteArticleButton 
        slug="test-article" 
        favorited={false}
        className="custom-class"
      >
        Favorite
      </FavoriteArticleButton>
    )

    const button = screen.getByRole('button')
    expect(button).toHaveClass('custom-class')
  })

  test('applies default btn classes', () => {
    vi.mocked(hooks.useFavoriteArticleMutation).mockReturnValue({
      mutate: vi.fn(),
      isLoading: false
    })

    render(
      <FavoriteArticleButton slug="test-article" favorited={false}>
        Favorite
      </FavoriteArticleButton>
    )

    const button = screen.getByRole('button')
    expect(button).toHaveClass('btn')
    expect(button).toHaveClass('btn-sm')
  })
})
