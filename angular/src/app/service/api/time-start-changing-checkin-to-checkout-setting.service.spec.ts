import { TestBed } from '@angular/core/testing';

import { TimeStartChangingCheckinToCheckoutSettingService } from './time-start-changing-checkin-to-checkout-setting.service';

describe('TimeStartChangingCheckinToCheckoutSettingService', () => {
  beforeEach(() => TestBed.configureTestingModule({}));

  it('should be created', () => {
    const service: TimeStartChangingCheckinToCheckoutSettingService = TestBed.get(TimeStartChangingCheckinToCheckoutSettingService);
    expect(service).toBeTruthy();
  });
});
