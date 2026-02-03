import { describe, test, expect } from 'vitest'
import Article from './Article'
import Author from './Author'

describe('Article Model', () => {
  test('creates an article with default values', () => {
    const article = new Article({})
    
    expect(article.slug).toBe('')
    expect(article.title).toBe('')
    expect(article.description).toBe('')
    expect(article.body).toBe('')
    expect(article.tagList).toEqual([])
    expect(article.favorited).toBe(false)
    expect(article.favoritesCount).toBe(0)
    expect(article.author).toBeInstanceOf(Author)
  })

  test('creates an article with provided values', () => {
    const authorData = {
      username: 'johndoe',
      bio: 'Developer',
      image: 'https://example.com/johndoe.jpg',
      following: true
    }

    const articleData = {
      slug: 'test-article',
      title: 'Test Article',
      description: 'This is a test',
      body: 'Article body content',
      tagList: ['testing', 'javascript'],
      createdAt: '2024-01-15T10:00:00.000Z',
      updatedAt: '2024-01-16T15:30:00.000Z',
      favorited: true,
      favoritesCount: 42,
      author: authorData
    }

    const article = new Article(articleData)

    expect(article.slug).toBe('test-article')
    expect(article.title).toBe('Test Article')
    expect(article.description).toBe('This is a test')
    expect(article.body).toBe('Article body content')
    expect(article.tagList).toEqual(['testing', 'javascript'])
    expect(article.favorited).toBe(true)
    expect(article.favoritesCount).toBe(42)
    expect(article.author).toBeInstanceOf(Author)
    expect(article.author.username).toBe('johndoe')
  })

  test('converts createdAt to date string', () => {
    const article = new Article({
      createdAt: '2024-01-15T10:00:00.000Z'
    })

    expect(typeof article.createdAt).toBe('string')
    expect(article.createdAt).toMatch(/\w+ \w+ \d+ \d+/)
  })

  test('converts updatedAt to date string', () => {
    const article = new Article({
      updatedAt: '2024-01-16T15:30:00.000Z'
    })

    expect(typeof article.updatedAt).toBe('string')
    expect(article.updatedAt).toMatch(/\w+ \w+ \d+ \d+/)
  })

  test('handles empty author object', () => {
    const article = new Article({
      author: {}
    })

    expect(article.author).toBeInstanceOf(Author)
    expect(article.author.username).toBe('')
  })

  test('preserves all article properties', () => {
    const fullArticle = {
      slug: 'complete-article',
      title: 'Complete Article',
      description: 'A complete description',
      body: 'Full body content',
      tagList: ['tag1', 'tag2', 'tag3'],
      createdAt: '2024-01-01T00:00:00.000Z',
      updatedAt: '2024-01-02T00:00:00.000Z',
      favorited: false,
      favoritesCount: 0,
      author: {
        username: 'testuser',
        bio: 'Test bio',
        image: 'test.jpg',
        following: false
      }
    }

    const article = new Article(fullArticle)

    expect(article.slug).toBe(fullArticle.slug)
    expect(article.title).toBe(fullArticle.title)
    expect(article.description).toBe(fullArticle.description)
    expect(article.body).toBe(fullArticle.body)
    expect(article.tagList).toBe(fullArticle.tagList)
    expect(article.favorited).toBe(fullArticle.favorited)
    expect(article.favoritesCount).toBe(fullArticle.favoritesCount)
  })
})
