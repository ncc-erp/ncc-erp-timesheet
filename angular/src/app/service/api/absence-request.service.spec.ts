import { TestBed } from '@angular/core/testing';

import { AbsenceRequestService } from './absence-request.service';

describe('AbsenceRequestService', () => {
  beforeEach(() => TestBed.configureTestingModule({}));

  it('should be created', () => {
    const service: AbsenceRequestService = TestBed.get(AbsenceRequestService);
    expect(service).toBeTruthy();
  });
});
