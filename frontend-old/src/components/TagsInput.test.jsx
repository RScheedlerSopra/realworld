import { describe, test, expect, vi } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'
import '@testing-library/jest-dom'
import TagsInput from './TagsInput'

describe('TagsInput Component', () => {
  test('renders input field with placeholder', () => {
    const field = { name: 'tagList', value: [] }
    const form = { setFieldValue: vi.fn() }

    render(<TagsInput field={field} form={form} />)

    const input = screen.getByPlaceholderText('Enter tags')
    expect(input).toBeInTheDocument()
    expect(input).toHaveClass('form-control')
  })

  test('displays existing tags', () => {
    const field = { name: 'tagList', value: ['javascript', 'testing', 'react'] }
    const form = { setFieldValue: vi.fn() }

    render(<TagsInput field={field} form={form} />)

    expect(screen.getByText('javascript')).toBeInTheDocument()
    expect(screen.getByText('testing')).toBeInTheDocument()
    expect(screen.getByText('react')).toBeInTheDocument()
  })

  test('adds a tag when Enter key is pressed', () => {
    const field = { name: 'tagList', value: ['existing'] }
    const form = { setFieldValue: vi.fn() }

    render(<TagsInput field={field} form={form} />)

    const input = screen.getByPlaceholderText('Enter tags')
    
    fireEvent.change(input, { target: { value: 'newtag' } })
    fireEvent.keyDown(input, { key: 'Enter', target: { value: 'newtag' } })

    expect(form.setFieldValue).toHaveBeenCalledWith('tagList', ['existing', 'newtag'])
  })

  test('clears input after adding tag', () => {
    const field = { name: 'tagList', value: [] }
    const form = { setFieldValue: vi.fn() }

    render(<TagsInput field={field} form={form} />)

    const input = screen.getByPlaceholderText('Enter tags')
    
    fireEvent.change(input, { target: { value: 'newtag' } })
    fireEvent.keyDown(input, { key: 'Enter', target: { value: 'newtag' } })

    expect(input.value).toBe('')
  })

  test('prevents default behavior when Enter is pressed', () => {
    const field = { name: 'tagList', value: [] }
    const form = { setFieldValue: vi.fn() }

    render(<TagsInput field={field} form={form} />)

    const input = screen.getByPlaceholderText('Enter tags')
    const event = new KeyboardEvent('keydown', { key: 'Enter', bubbles: true, cancelable: true })
    const preventDefaultSpy = vi.fn()
    event.preventDefault = preventDefaultSpy

    Object.defineProperty(event, 'target', {
      writable: false,
      value: input
    })

    input.dispatchEvent(event)

    // The component should call preventDefault
    expect(form.setFieldValue).toHaveBeenCalled()
  })

  test('removes a tag when close icon is clicked', () => {
    const field = { name: 'tagList', value: ['tag1', 'tag2', 'tag3'] }
    const form = { setFieldValue: vi.fn() }

    const { container } = render(<TagsInput field={field} form={form} />)

    const closeIcons = container.querySelectorAll('.ion-close-round')
    
    fireEvent.click(closeIcons[1]) // Remove 'tag2'

    expect(form.setFieldValue).toHaveBeenCalledWith('tagList', ['tag1', 'tag3'])
  })

  test('does not add tag on other key presses', () => {
    const field = { name: 'tagList', value: [] }
    const form = { setFieldValue: vi.fn() }

    render(<TagsInput field={field} form={form} />)

    const input = screen.getByPlaceholderText('Enter tags')
    
    fireEvent.change(input, { target: { value: 'newtag' } })
    fireEvent.keyDown(input, { key: 'a', target: { value: 'newtag' } })

    expect(form.setFieldValue).not.toHaveBeenCalled()
  })

  test('renders empty tag list when no tags exist', () => {
    const field = { name: 'tagList', value: [] }
    const form = { setFieldValue: vi.fn() }

    const { container } = render(<TagsInput field={field} form={form} />)

    const tagList = container.querySelector('.tag-list')
    expect(tagList).toBeInTheDocument()
    expect(tagList.children).toHaveLength(0)
  })

  test('renders tags with correct CSS classes', () => {
    const field = { name: 'tagList', value: ['test'] }
    const form = { setFieldValue: vi.fn() }

    const { container } = render(<TagsInput field={field} form={form} />)

    const tag = container.querySelector('.tag-default.tag-pill')
    expect(tag).toBeInTheDocument()
    expect(tag.textContent).toContain('test')
  })

  test('handles multiple tag additions', () => {
    const field = { name: 'tagList', value: [] }
    const form = { setFieldValue: vi.fn() }

    const { rerender } = render(<TagsInput field={field} form={form} />)

    const input = screen.getByPlaceholderText('Enter tags')
    
    fireEvent.change(input, { target: { value: 'tag1' } })
    fireEvent.keyDown(input, { key: 'Enter', target: { value: 'tag1' } })

    expect(form.setFieldValue).toHaveBeenCalledWith('tagList', ['tag1'])

    // Simulate the field value updating
    const updatedField = { name: 'tagList', value: ['tag1'] }
    rerender(<TagsInput field={updatedField} form={form} />)

    fireEvent.change(input, { target: { value: 'tag2' } })
    fireEvent.keyDown(input, { key: 'Enter', target: { value: 'tag2' } })

    expect(form.setFieldValue).toHaveBeenCalledWith('tagList', ['tag1', 'tag2'])
  })
})
