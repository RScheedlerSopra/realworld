import { ChangeDetectionStrategy, Component, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { DraftsService } from '../../article/services/drafts.service';
import { Article } from '../../article/models/article.model';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { DatePipe } from '@angular/common';

@Component({
  selector: 'app-profile-drafts',
  template: `
    <div>
      @if (drafts().length === 0 && !isLoading()) {
        <div class="article-preview">
          <p>No drafts yet. Start writing to save your work for later.</p>
        </div>
      }
      @for (draft of drafts(); track draft.articleId) {
        <div class="article-preview">
          <div class="article-meta">
            <span class="date">{{ draft.updatedAt | date: 'longDate' }}</span>
          </div>
          <button class="btn btn-sm btn-outline-danger pull-xs-right" (click)="deleteDraft(draft.articleId!)">
            <i class="ion-trash-a"></i> Delete
          </button>
          <a [routerLink]="['/editor', draft.articleId]" class="btn btn-sm btn-outline-secondary pull-xs-right" style="margin-right: 5px;">
            <i class="ion-edit"></i> Edit
          </a>
          <a [routerLink]="['/editor', draft.articleId]" class="preview-link">
            <h1>{{ draft.title }}</h1>
            <p>{{ draft.description }}</p>
            <span>Read more...</span>
            <ul class="tag-list">
              @for (tag of draft.tagList; track tag) {
                <li class="tag-default tag-pill tag-outline">
                  {{ tag }}
                </li>
              }
            </ul>
          </a>
        </div>
      }
    </div>
  `,
  imports: [RouterLink, DatePipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export default class ProfileDraftsComponent implements OnInit {
  drafts = signal<Article[]>([]);
  isLoading = signal(true);
  destroyRef = inject(DestroyRef);

  constructor(
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly draftsService: DraftsService,
  ) {}

  ngOnInit(): void {
    this.loadDrafts();
  }

  loadDrafts(): void {
    this.isLoading.set(true);
    this.draftsService
      .list()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: ({ drafts }) => {
          this.drafts.set(drafts);
          this.isLoading.set(false);
        },
        error: () => {
          this.isLoading.set(false);
        },
      });
  }

  deleteDraft(id: number): void {
    if (confirm('Are you sure you want to delete this draft?')) {
      this.draftsService
        .delete(id)
        .pipe(takeUntilDestroyed(this.destroyRef))
        .subscribe({
          next: () => {
            this.drafts.update(drafts => drafts.filter(d => d.articleId !== id));
          },
        });
    }
  }
}
