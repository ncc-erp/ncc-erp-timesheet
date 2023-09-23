import { TestBed } from '@angular/core/testing';

import { SendKomuPunishedCheckInService } from './send-komu-punished-check-in.service';

describe('SendKomuPunishedCheckInService', () => {
  beforeEach(() => TestBed.configureTestingModule({}));

  it('should be created', () => {
    const service: SendKomuPunishedCheckInService = TestBed.get(SendKomuPunishedCheckInService);
    expect(service).toBeTruthy();
  });
});
