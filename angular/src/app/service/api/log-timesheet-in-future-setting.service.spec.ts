import { TestBed } from '@angular/core/testing';

import { LogTimesheetInFutureSettingService } from './log-timesheet-in-future-setting.service';

describe('LogTimesheetInFutureSettingService', () => {
  beforeEach(() => TestBed.configureTestingModule({}));

  it('should be created', () => {
    const service: LogTimesheetInFutureSettingService = TestBed.get(LogTimesheetInFutureSettingService);
    expect(service).toBeTruthy();
  });
});
