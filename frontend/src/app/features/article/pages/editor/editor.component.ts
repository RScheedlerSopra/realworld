import { ChangeDetectionStrategy, Component, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, UntypedFormGroup } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { combineLatest } from 'rxjs';
import { Errors } from '../../../../core/models/errors.model';
import { ArticlesService } from '../../services/articles.service';
import { UserService } from '../../../../core/auth/services/user.service';
import { ListErrorsComponent } from '../../../../shared/components/list-errors.component';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

interface ArticleForm {
  title: FormControl<string>;
  description: FormControl<string>;
  body: FormControl<string>;
}

@Component({
  selector: 'app-editor-page',
  templateUrl: './editor.component.html',
  imports: [ListErrorsComponent, ReactiveFormsModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export default class EditorComponent implements OnInit {
  tagList = signal<string[]>([]);
  articleForm: UntypedFormGroup = new FormGroup<ArticleForm>({
    title: new FormControl('', { nonNullable: true }),
    description: new FormControl('', { nonNullable: true }),
    body: new FormControl('', { nonNullable: true }),
  });
  tagField = new FormControl<string>('', { nonNullable: true });

  errors = signal<Errors | null>(null);
  isSubmitting = signal(false);
  isDraft = signal(false);
  currentSlug = signal<string | null>(null);
  successMessage = signal<string | null>(null);
  destroyRef = inject(DestroyRef);

  constructor(
    private readonly articleService: ArticlesService,
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly userService: UserService,
  ) {}

  ngOnInit() {
    const slug = this.route.snapshot.params['slug'];
    if (slug) {
      this.currentSlug.set(slug);
      combineLatest([this.articleService.get(slug), this.userService.getCurrentUser()])
        .pipe(takeUntilDestroyed(this.destroyRef))
        .subscribe(([article, { user }]) => {
          if (user.username === article.author.username) {
            this.tagList.set(article.tagList);
            this.articleForm.patchValue(article);
            this.isDraft.set(article.isDraft ?? false);
          } else {
            void this.router.navigate(['/']);
          }
        });
    }
  }

  addTag() {
    // retrieve tag control
    const tag = this.tagField.value;
    // only add tag if it does not exist yet
    if (tag != null && tag.trim() !== '' && this.tagList().indexOf(tag) < 0) {
      this.tagList.update(tags => [...tags, tag]);
    }
    // clear the input
    this.tagField.reset('');
  }

  removeTag(tagName: string): void {
    this.tagList.update(tags => tags.filter(tag => tag !== tagName));
  }

  saveDraft(): void {
    this.isSubmitting.set(true);
    this.errors.set(null);
    this.successMessage.set(null);
    // update any single tag
    this.addTag();

    const slug = this.currentSlug();
    const articleData = {
      ...this.articleForm.value,
      tagList: this.tagList(),
      isDraft: true,
    };

    const observable = slug
      ? this.articleService.update({ ...articleData, slug })
      : this.articleService.create(articleData);

    observable.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: article => {
        this.successMessage.set('Draft saved successfully');
        this.isSubmitting.set(false);
        // Update URL if this was a new draft
        if (!slug) {
          this.currentSlug.set(article.slug);
          void this.router.navigate(['/editor', article.slug], { replaceUrl: true });
        }
      },
      error: err => {
        this.errors.set(err);
        this.isSubmitting.set(false);
      },
    });
  }

  publishArticle(): void {
    this.isSubmitting.set(true);
    this.successMessage.set(null);
    // update any single tag
    this.addTag();

    const slug = this.currentSlug();
    
    if (slug && this.isDraft()) {
      // Publishing an existing draft
      this.articleService.publishArticle(slug)
        .pipe(takeUntilDestroyed(this.destroyRef))
        .subscribe({
          next: article => this.router.navigate(['/article/', article.slug]),
          error: err => {
            this.errors.set(err);
            this.isSubmitting.set(false);
          },
        });
    } else {
      // Creating a new published article or updating existing published article
      const articleData = {
        ...this.articleForm.value,
        tagList: this.tagList(),
        isDraft: false,
      };

      const observable = slug
        ? this.articleService.update({ ...articleData, slug })
        : this.articleService.create(articleData);

      observable.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
        next: article => this.router.navigate(['/article/', article.slug]),
        error: err => {
          this.errors.set(err);
          this.isSubmitting.set(false);
        },
      });
    }
  }
}
