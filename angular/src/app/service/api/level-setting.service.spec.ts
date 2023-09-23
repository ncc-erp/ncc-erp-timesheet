import { TestBed } from '@angular/core/testing';

import { LevelSettingService } from './level-setting.service';

describe('LevelSettingService', () => {
  beforeEach(() => TestBed.configureTestingModule({}));

  it('should be created', () => {
    const service: LevelSettingService = TestBed.get(LevelSettingService);
    expect(service).toBeTruthy();
  });
});
