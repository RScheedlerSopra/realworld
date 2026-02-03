import { describe, test, expect } from 'vitest'
import { render, screen } from '@testing-library/react'
import { Formik } from 'formik'
import FormErrors from './FormErrors'

describe('FormErrors Component', () => {
  test('renders nothing when there are no errors', () => {
    const { container } = render(
      <Formik initialValues={{}} onSubmit={() => {}}>
        <FormErrors />
      </Formik>
    )

    expect(container.querySelector('.error-messages')).toBeNull()
  })

  test('renders error messages when errors exist', () => {
    const errors = {
      email: ['is invalid', 'is required'],
      password: ['is too short']
    }

    render(
      <Formik
        initialValues={{}}
        initialErrors={errors}
        onSubmit={() => {}}
      >
        <FormErrors />
      </Formik>
    )

    expect(screen.getByText('email is invalid')).toBeInTheDocument()
    expect(screen.getByText('email is required')).toBeInTheDocument()
    expect(screen.getByText('password is too short')).toBeInTheDocument()
  })

  test('renders errors in a list', () => {
    const errors = {
      username: ['is required'],
      email: ['is invalid']
    }

    const { container } = render(
      <Formik
        initialValues={{}}
        initialErrors={errors}
        onSubmit={() => {}}
      >
        <FormErrors />
      </Formik>
    )

    const errorList = container.querySelector('.error-messages')
    expect(errorList).toBeInTheDocument()
    expect(errorList.tagName).toBe('UL')
    
    const items = errorList.querySelectorAll('li')
    expect(items).toHaveLength(2)
  })

  test('renders multiple error messages for a single field', () => {
    const errors = {
      password: ['is required', 'is too short', 'must contain a number']
    }

    render(
      <Formik
        initialValues={{}}
        initialErrors={errors}
        onSubmit={() => {}}
      >
        <FormErrors />
      </Formik>
    )

    expect(screen.getByText('password is required')).toBeInTheDocument()
    expect(screen.getByText('password is too short')).toBeInTheDocument()
    expect(screen.getByText('password must contain a number')).toBeInTheDocument()
  })

  test('formats error message with field name and message', () => {
    const errors = {
      email: ['is invalid']
    }

    render(
      <Formik
        initialValues={{}}
        initialErrors={errors}
        onSubmit={() => {}}
      >
        <FormErrors />
      </Formik>
    )

    const errorText = screen.getByText('email is invalid')
    expect(errorText.textContent).toBe('email is invalid')
  })

  test('handles empty error arrays', () => {
    const errors = {
      email: []
    }

    render(
      <Formik
        initialValues={{}}
        initialErrors={errors}
        onSubmit={() => {}}
      >
        <FormErrors />
      </Formik>
    )

    // Component still renders the ul element, just with no items
    const errorList = screen.queryByRole('list')
    if (errorList) {
      expect(errorList.children.length).toBe(0)
    }
  })

  test('renders errors for multiple fields', () => {
    const errors = {
      username: ['is required'],
      email: ['is invalid', 'is required'],
      password: ['is too short'],
      bio: ['is too long']
    }

    const { container } = render(
      <Formik
        initialValues={{}}
        initialErrors={errors}
        onSubmit={() => {}}
      >
        <FormErrors />
      </Formik>
    )

    const items = container.querySelectorAll('li')
    expect(items).toHaveLength(5) // 1 + 2 + 1 + 1
  })
})
