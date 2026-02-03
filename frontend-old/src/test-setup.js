import React from 'react'
import '@testing-library/jest-dom'
import { expect, afterEach } from 'vitest'
import { cleanup } from '@testing-library/react'

// Make React globally available
global.React = React

// Cleans up after each test case (e.g. clearing jsdom)
afterEach(() => {
  cleanup()
})
