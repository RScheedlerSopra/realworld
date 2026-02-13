import { ChangeDetectionStrategy, Component, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { ArticlesService } from '../../services/articles.service';
import { Article } from '../../models/article.model';
import { DatePipe } from '@angular/common';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

@Component({
  selector: 'app-drafts-page',
  templateUrl: './drafts.component.html',
  imports: [RouterLink, DatePipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export default class DraftsComponent implements OnInit {
  drafts = signal<Article[]>([]);
  isLoading = signal(true);
  errorMessage = signal<string | null>(null);
  destroyRef = inject(DestroyRef);

  constructor(
    private readonly articlesService: ArticlesService,
    private readonly router: Router,
  ) {}

  ngOnInit(): void {
    this.loadDrafts();
  }

  loadDrafts(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);
    
    this.articlesService
      .listDrafts()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: response => {
          this.drafts.set(response.articles);
          this.isLoading.set(false);
        },
        error: () => {
          this.errorMessage.set('Unable to load drafts. Please refresh the page.');
          this.isLoading.set(false);
        },
      });
  }

  editDraft(slug: string): void {
    void this.router.navigate(['/editor', slug]);
  }

  deleteDraft(slug: string, event: Event): void {
    event.stopPropagation();
    
    this.articlesService
      .delete(slug)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          // Remove the draft from the list
          this.drafts.update(drafts => drafts.filter(d => d.slug !== slug));
        },
        error: () => {
          this.errorMessage.set('Failed to delete draft. Please try again.');
        },
      });
  }
}
