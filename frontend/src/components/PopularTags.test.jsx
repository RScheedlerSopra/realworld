import { describe, test, expect, vi, beforeEach } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from 'react-query'
import '@testing-library/jest-dom'
import PopularTags from './PopularTags'

// Mock useQuery from react-query
vi.mock('react-query', async () => {
  const actual = await vi.importActual('react-query')
  return {
    ...actual,
    useQuery: vi.fn()
  }
})

import { useQuery } from 'react-query'

describe('PopularTags Component', () => {
  let queryClient

  beforeEach(() => {
    vi.clearAllMocks()
    queryClient = new QueryClient()
  })

  test('displays loading message while fetching', () => {
    vi.mocked(useQuery).mockReturnValue({
      data: { tags: [] },
      isFetching: true,
      isError: false
    })

    render(
      <QueryClientProvider client={queryClient}>
        <PopularTags onTagClick={vi.fn()} />
      </QueryClientProvider>
    )

    expect(screen.getByText('Loading tags...')).toBeInTheDocument()
  })

  test('displays error message when fetch fails', () => {
    vi.mocked(useQuery).mockReturnValue({
      data: { tags: [] },
      isFetching: false,
      isError: true
    })

    render(
      <QueryClientProvider client={queryClient}>
        <PopularTags onTagClick={vi.fn()} />
      </QueryClientProvider>
    )

    expect(screen.getByText('Loading tags failed :(')).toBeInTheDocument()
  })

  test('renders Popular Tags heading', () => {
    vi.mocked(useQuery).mockReturnValue({
      data: { tags: ['javascript', 'react'] },
      isFetching: false,
      isError: false
    })

    render(
      <QueryClientProvider client={queryClient}>
        <PopularTags onTagClick={vi.fn()} />
      </QueryClientProvider>
    )

    expect(screen.getByText('Popular Tags')).toBeInTheDocument()
  })

  test('renders list of tags', () => {
    vi.mocked(useQuery).mockReturnValue({
      data: { tags: ['javascript', 'react', 'testing'] },
      isFetching: false,
      isError: false
    })

    render(
      <QueryClientProvider client={queryClient}>
        <PopularTags onTagClick={vi.fn()} />
      </QueryClientProvider>
    )

    expect(screen.getByText('javascript')).toBeInTheDocument()
    expect(screen.getByText('react')).toBeInTheDocument()
    expect(screen.getByText('testing')).toBeInTheDocument()
  })

  test('calls onTagClick when tag is clicked', () => {
    const onTagClick = vi.fn()
    vi.mocked(useQuery).mockReturnValue({
      data: { tags: ['javascript', 'react'] },
      isFetching: false,
      isError: false
    })

    render(
      <QueryClientProvider client={queryClient}>
        <PopularTags onTagClick={onTagClick} />
      </QueryClientProvider>
    )

    fireEvent.click(screen.getByText('javascript'))

    expect(onTagClick).toHaveBeenCalledWith('javascript')
  })

  test('prevents default behavior when tag is clicked', () => {
    vi.mocked(useQuery).mockReturnValue({
      data: { tags: ['javascript'] },
      isFetching: false,
      isError: false
    })

    render(
      <QueryClientProvider client={queryClient}>
        <PopularTags onTagClick={vi.fn()} />
      </QueryClientProvider>
    )

    const tagLink = screen.getByText('javascript')
    const event = fireEvent.click(tagLink)

    // Event should be prevented (not follow the href)
    expect(event).toBe(false)
  })

  test('renders tags with correct CSS classes', () => {
    vi.mocked(useQuery).mockReturnValue({
      data: { tags: ['javascript'] },
      isFetching: false,
      isError: false
    })

    const { container } = render(
      <QueryClientProvider client={queryClient}>
        <PopularTags onTagClick={vi.fn()} />
      </QueryClientProvider>
    )

    const tag = container.querySelector('.tag-pill.tag-default')
    expect(tag).toBeInTheDocument()
  })

  test('applies sidebar class to container', () => {
    vi.mocked(useQuery).mockReturnValue({
      data: { tags: [] },
      isFetching: false,
      isError: false
    })

    const { container } = render(
      <QueryClientProvider client={queryClient}>
        <PopularTags onTagClick={vi.fn()} />
      </QueryClientProvider>
    )

    expect(container.querySelector('.sidebar')).toBeInTheDocument()
  })

  test('applies tag-list class to tags container', () => {
    vi.mocked(useQuery).mockReturnValue({
      data: { tags: [] },
      isFetching: false,
      isError: false
    })

    const { container } = render(
      <QueryClientProvider client={queryClient}>
        <PopularTags onTagClick={vi.fn()} />
      </QueryClientProvider>
    )

    expect(container.querySelector('.tag-list')).toBeInTheDocument()
  })

  test('handles empty tags array', () => {
    vi.mocked(useQuery).mockReturnValue({
      data: { tags: [] },
      isFetching: false,
      isError: false
    })

    const { container } = render(
      <QueryClientProvider client={queryClient}>
        <PopularTags onTagClick={vi.fn()} />
      </QueryClientProvider>
    )

    const tagList = container.querySelector('.tag-list')
    expect(tagList.children.length).toBe(0)
  })

  test('calls onTagClick with correct tag for multiple tags', () => {
    const onTagClick = vi.fn()
    vi.mocked(useQuery).mockReturnValue({
      data: { tags: ['javascript', 'react', 'testing'] },
      isFetching: false,
      isError: false
    })

    render(
      <QueryClientProvider client={queryClient}>
        <PopularTags onTagClick={onTagClick} />
      </QueryClientProvider>
    )

    fireEvent.click(screen.getByText('react'))
    expect(onTagClick).toHaveBeenCalledWith('react')

    fireEvent.click(screen.getByText('testing'))
    expect(onTagClick).toHaveBeenCalledWith('testing')
  })
})
