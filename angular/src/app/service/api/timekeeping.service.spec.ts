import { TestBed } from '@angular/core/testing';

import { TimekeepingService } from './timekeeping.service';

describe('TimekeepingService', () => {
  beforeEach(() => TestBed.configureTestingModule({}));

  it('should be created', () => {
    const service: TimekeepingService = TestBed.get(TimekeepingService);
    expect(service).toBeTruthy();
  });
});
