import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { Article } from '../models/article.model';

@Injectable({ providedIn: 'root' })
export class DraftsService {
  constructor(private readonly http: HttpClient) {}

  create(draft: Partial<Article>): Observable<Article> {
    return this.http.post<{ article: Article }>('/drafts/', { draft: draft }).pipe(map(data => data.article));
  }

  list(limit?: number, offset?: number): Observable<{ drafts: Article[]; draftsCount: number }> {
    let params = new HttpParams();
    if (limit) params = params.set('limit', limit.toString());
    if (offset) params = params.set('offset', offset.toString());

    return this.http.get<{ drafts: Article[]; draftsCount: number }>('/drafts', { params });
  }

  get(id: number): Observable<Article> {
    return this.http.get<{ article: Article }>(`/drafts/${id}`).pipe(map(data => data.article));
  }

  update(id: number, draft: Partial<Article>): Observable<Article> {
    return this.http.put<{ article: Article }>(`/drafts/${id}`, { draft: draft }).pipe(map(data => data.article));
  }

  publish(id: number): Observable<Article> {
    return this.http.put<{ article: Article }>(`/drafts/${id}/publish`, {}).pipe(map(data => data.article));
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`/drafts/${id}`);
  }
}
