import { expect, test, describe } from 'vitest'

// Simple utility function to test
function add(a, b) {
  return a + b
}

describe('Utility Functions', () => {
  test('adds 1 + 2 to equal 3', () => {
    expect(add(1, 2)).toBe(3)
  })

  test('adds negative numbers correctly', () => {
    expect(add(-1, -2)).toBe(-3)
  })

  test('adds zero correctly', () => {
    expect(add(0, 5)).toBe(5)
  })
})
