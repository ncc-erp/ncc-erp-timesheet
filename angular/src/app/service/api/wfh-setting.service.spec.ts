import { TestBed } from '@angular/core/testing';

import { WfhSettingService } from './wfh-setting.service';

describe('WfhSettingService', () => {
  beforeEach(() => TestBed.configureTestingModule({}));

  it('should be created', () => {
    const service: WfhSettingService = TestBed.get(WfhSettingService);
    expect(service).toBeTruthy();
  });
});
