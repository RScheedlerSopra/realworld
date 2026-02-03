import { describe, test, expect } from 'vitest'
import Author from './Author'

describe('Author Model', () => {
  test('creates an author with default values', () => {
    const author = new Author()
    
    expect(author.username).toBe('')
    expect(author.bio).toBe('')
    expect(author.image).toBe('')
    expect(author.following).toBe(false)
  })

  test('creates an author with provided values', () => {
    const authorData = {
      username: 'johndoe',
      bio: 'Software Developer',
      image: 'https://example.com/avatar.jpg',
      following: true
    }

    const author = new Author(authorData)

    expect(author.username).toBe('johndoe')
    expect(author.bio).toBe('Software Developer')
    expect(author.image).toBe('https://example.com/avatar.jpg')
    expect(author.following).toBe(true)
  })

  test('creates an author with partial values', () => {
    const author = new Author({
      username: 'janedoe',
      bio: 'Writer'
    })

    expect(author.username).toBe('janedoe')
    expect(author.bio).toBe('Writer')
    expect(author.image).toBe('')
    expect(author.following).toBe(false)
  })

  test('handles undefined constructor argument', () => {
    const author = new Author(undefined)
    
    expect(author.username).toBe('')
    expect(author.bio).toBe('')
    expect(author.image).toBe('')
    expect(author.following).toBe(false)
  })

  test('handles empty object constructor argument', () => {
    const author = new Author({})
    
    expect(author.username).toBe('')
    expect(author.bio).toBe('')
    expect(author.image).toBe('')
    expect(author.following).toBe(false)
  })

  test('preserves all author properties', () => {
    const authorData = {
      username: 'testuser',
      bio: 'This is my bio',
      image: 'avatar.png',
      following: true
    }

    const author = new Author(authorData)

    expect(author.username).toBe(authorData.username)
    expect(author.bio).toBe(authorData.bio)
    expect(author.image).toBe(authorData.image)
    expect(author.following).toBe(authorData.following)
  })

  test('handles boolean false for following explicitly', () => {
    const author = new Author({
      username: 'user1',
      following: false
    })

    expect(author.following).toBe(false)
  })
})
