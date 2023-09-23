import { TestBed } from '@angular/core/testing';

import { SpecialProjectTaskSettingService } from './special-project-task-config.service';

describe('SpecialProjectTaskSettingService', () => {
  beforeEach(() => TestBed.configureTestingModule({}));

  it('should be created', () => {
    const service: SpecialProjectTaskSettingService = TestBed.get(SpecialProjectTaskSettingService);
    expect(service).toBeTruthy();
  });
});
