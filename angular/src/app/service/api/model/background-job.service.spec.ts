import { TestBed } from '@angular/core/testing';

import { BackgroundJobService } from './background-job.service';

describe('BackgroundJobService', () => {
  beforeEach(() => TestBed.configureTestingModule({}));

  it('should be created', () => {
    const service: BackgroundJobService = TestBed.get(BackgroundJobService);
    expect(service).toBeTruthy();
  });
});
