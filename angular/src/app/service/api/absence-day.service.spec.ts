import { TestBed } from '@angular/core/testing';

import { AbsenceDayService } from './absence-day.service';

describe('AbsenceDayServiceService', () => {
  beforeEach(() => TestBed.configureTestingModule({}));

  it('should be created', () => {
    const service: AbsenceDayService = TestBed.get(AbsenceDayService);
    expect(service).toBeTruthy();
  });
});
