import 'zone.js';
import 'zone.js/testing';
import { describe, it, expect, beforeEach, afterEach, beforeAll, vi } from 'vitest';
import { TestBed, getTestBed } from '@angular/core/testing';
import { BrowserDynamicTestingModule, platformBrowserDynamicTesting } from '@angular/platform-browser-dynamic/testing';
import { Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import DraftsComponent from './drafts.component';
import { ArticlesService } from '../../services/articles.service';
import { Article } from '../../models/article.model';

describe('DraftsComponent', () => {
  beforeAll(() => {
    getTestBed().initTestEnvironment(BrowserDynamicTestingModule, platformBrowserDynamicTesting());
  });

  let component: DraftsComponent;
  let mockArticlesService: any;
  let mockRouter: any;

  const mockDrafts: Article[] = [
    {
      slug: 'draft-1',
      title: 'Draft Article 1',
      description: 'Draft description 1',
      body: 'Draft body 1',
      tagList: ['draft', 'test'],
      createdAt: '2024-01-01',
      updatedAt: '2024-01-02',
      favorited: false,
      favoritesCount: 0,
      isDraft: true,
      author: {
        username: 'testuser',
        bio: 'Test bio',
        image: 'https://example.com/avatar.jpg',
        following: false,
      },
    },
    {
      slug: 'draft-2',
      title: 'Draft Article 2',
      description: 'Draft description 2',
      body: 'Draft body 2',
      tagList: ['draft'],
      createdAt: '2024-01-03',
      updatedAt: '2024-01-04',
      favorited: false,
      favoritesCount: 0,
      isDraft: true,
      author: {
        username: 'testuser',
        bio: 'Test bio',
        image: 'https://example.com/avatar.jpg',
        following: false,
      },
    },
  ];

  beforeEach(() => {
    mockArticlesService = {
      listDrafts: vi.fn().mockReturnValue(of({ articles: mockDrafts, articlesCount: 2 })),
      delete: vi.fn().mockReturnValue(of(undefined)),
    };

    mockRouter = {
      navigate: vi.fn(),
    };

    TestBed.configureTestingModule({
      providers: [
        DraftsComponent,
        { provide: ArticlesService, useValue: mockArticlesService },
        { provide: Router, useValue: mockRouter },
      ],
    });

    component = TestBed.inject(DraftsComponent);
  });

  afterEach(() => {
    TestBed.resetTestingModule();
  });

  it('should be created', () => {
    expect(component).toBeTruthy();
  });

  it('should load drafts on init', () => {
    component.ngOnInit();
    expect(mockArticlesService.listDrafts).toHaveBeenCalled();
    expect(component.drafts().length).toBe(2);
    expect(component.isLoading()).toBe(false);
  });

  it('should show empty state when no drafts', () => {
    mockArticlesService.listDrafts.mockReturnValue(of({ articles: [], articlesCount: 0 }));
    component.ngOnInit();
    expect(component.drafts().length).toBe(0);
    expect(component.isLoading()).toBe(false);
  });

  it('should handle error when loading drafts fails', () => {
    mockArticlesService.listDrafts.mockReturnValue(throwError(() => new Error('Failed to load')));
    component.ngOnInit();
    expect(component.errorMessage()).toBeTruthy();
    expect(component.isLoading()).toBe(false);
  });

  it('should navigate to editor when editing draft', () => {
    component.editDraft('draft-1');
    expect(mockRouter.navigate).toHaveBeenCalledWith(['/editor', 'draft-1']);
  });

  it('should delete draft and update list', () => {
    component.drafts.set(mockDrafts);
    const event = new Event('click');
    
    component.deleteDraft('draft-1', event);
    
    expect(mockArticlesService.delete).toHaveBeenCalledWith('draft-1');
    expect(component.drafts().length).toBe(1);
    expect(component.drafts()[0].slug).toBe('draft-2');
  });

  it('should handle delete error', () => {
    mockArticlesService.delete.mockReturnValue(throwError(() => new Error('Delete failed')));
    component.drafts.set(mockDrafts);
    const event = new Event('click');
    
    component.deleteDraft('draft-1', event);
    
    expect(component.errorMessage()).toBeTruthy();
  });
});
