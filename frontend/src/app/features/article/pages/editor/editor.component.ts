import { ChangeDetectionStrategy, Component, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, UntypedFormGroup } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { combineLatest, switchMap } from 'rxjs';
import { Errors } from '../../../../core/models/errors.model';
import { ArticlesService } from '../../services/articles.service';
import { DraftsService } from '../../services/drafts.service';
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
  draftId = signal<number | null>(null);
  isNewArticle = signal(true);
  destroyRef = inject(DestroyRef);

  constructor(
    private readonly articleService: ArticlesService,
    private readonly draftsService: DraftsService,
    public readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly userService: UserService,
  ) {}

  ngOnInit() {
    const routeParam = this.route.snapshot.params['slug'];
    if (routeParam) {
      this.isNewArticle.set(false);
      // Check if it's a numeric ID (draft) or a slug (published article)
      const idAsNumber = Number(routeParam);
      if (!isNaN(idAsNumber) && Number.isInteger(idAsNumber)) {
        // It's a draft ID
        this.isDraft.set(true);
        this.draftId.set(idAsNumber);
        combineLatest([this.draftsService.get(idAsNumber), this.userService.getCurrentUser()])
          .pipe(takeUntilDestroyed(this.destroyRef))
          .subscribe(([draft, { user }]) => {
            if (user.username === draft.author.username) {
              this.tagList.set(draft.tagList || []);
              this.articleForm.patchValue(draft);
            } else {
              void this.router.navigate(['/']);
            }
          });
      } else {
        // It's a slug for published article
        this.isDraft.set(false);
        combineLatest([this.articleService.get(routeParam), this.userService.getCurrentUser()])
          .pipe(takeUntilDestroyed(this.destroyRef))
          .subscribe(([article, { user }]) => {
            if (user.username === article.author.username) {
              this.tagList.set(article.tagList);
              this.articleForm.patchValue(article);
            } else {
              void this.router.navigate(['/']);
            }
          });
      }
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
    this.addTag();

    const draftData = {
      ...this.articleForm.value,
      tagList: this.tagList(),
    };

    const observable = this.draftId()
      ? this.draftsService.update(this.draftId()!, draftData)
      : this.draftsService.create(draftData);

    observable.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => {
        const currentUser = this.userService.getCurrentUserSync();
        if (currentUser) {
          void this.router.navigate(['/profile', currentUser.username, 'drafts']);
        }
      },
      error: err => {
        this.errors.set(err);
        this.isSubmitting.set(false);
      },
    });
  }

  publishDraft(): void {
    if (!this.draftId()) return;

    this.isSubmitting.set(true);
    this.addTag();

    const draftData = {
      ...this.articleForm.value,
      tagList: this.tagList(),
    };

    // First update the draft with current form data, then publish
    this.draftsService
      .update(this.draftId()!, draftData)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        // After updating, publish the draft
        switchMap(() => this.draftsService.publish(this.draftId()!)),
      )
      .subscribe({
        next: article => this.router.navigate(['/article/', article.slug]),
        error: err => {
          this.errors.set(err);
          this.isSubmitting.set(false);
        },
      });
  }

  submitForm(): void {
    // If editing a draft, publish it
    if (this.isDraft() && this.draftId()) {
      this.publishDraft();
      return;
    }

    this.isSubmitting.set(true);
    this.addTag();

    const slug = this.route.snapshot.params['slug'];
    const articleData = {
      ...this.articleForm.value,
      tagList: this.tagList(),
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
