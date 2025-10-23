import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { switchMap, catchError } from 'rxjs/operators';
import { BaseApiService } from './base-api.service';

export interface ProjectDto {
  id: number;
  name: string;
  code: string;
  status: number;
}

export interface BotReportSettingDto {
  enable: boolean;
  everyday: boolean;
  hour: number;
  minute: number;
  dayofweek: string;
  botUri: string;
  branchCodes?: string[];
  minHours: number;
  topN: number;
  projectIds?: number[];
}

@Injectable({
  providedIn: 'root'
})
export class BotReportSettingService extends BaseApiService {

  constructor(http: HttpClient) {
    super(http);
  }

  changeUrl(): string {
    return 'Configuration';
  }

  getBotReportSetting(): Observable<any> {
    return this.http.get<any>(this.rootUrl + '/GetBotReportSetting');
  }

  setBotReportSetting(input: BotReportSettingDto): Observable<BotReportSettingDto> {
    const payload = {
      ...input,
      projectIds: input.projectIds ? input.projectIds.map(id => Number(id)) : []
    };
    return this.http.post<BotReportSettingDto>(this.rootUrl + '/SetBotReportSetting', payload);
  }

  getActiveProjects(): Observable<ProjectDto[]> {
    return this.http.get<ProjectDto[]>(this.baseUrl + '/api/services/app/Project/GetAllActiveProjects');
  }

  getSelectedProjectIds(): Observable<number[]> {
    return new Observable<number[]>(observer => {
      this.getBotReportSetting().subscribe({
        next: (response: any) => {
          const projectIds = response && response.result && response.result.projectIds 
            ? response.result.projectIds 
            : [];
          observer.next(projectIds);
          observer.complete();
        },
        error: (err: any) => {
          console.error('Error getting selected projects:', err);
          observer.next([]);
          observer.complete();
        }
      });
    });
  }

  updateSelectedProjects(projectIds: number[]): Observable<any> {
    return this.getBotReportSetting().pipe(
      switchMap(setting => {
        if (!setting || !setting.result) {
          throw new Error('Unable to retrieve current configuration');
        }
        const numericProjectIds = projectIds ? projectIds.map(id => Number(id)) : [];
        const updatedSetting: BotReportSettingDto = {
          ...setting.result,
          projectIds: numericProjectIds
        };
        return this.setBotReportSetting(updatedSetting);
      }),
      catchError(error => {
        console.error('Error updating project list:', error);
        return of({ success: false, error: error.message || 'An error occurred' });
      })
    );
  }
}