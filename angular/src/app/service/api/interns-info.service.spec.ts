import { TestBed } from '@angular/core/testing';

import { InternsInfoService } from './interns-info.service';

describe('InternsInfo', () => {
  beforeEach(() => TestBed.configureTestingModule({}));

  it('should be created', () => {
    const service: InternsInfoService = TestBed.get(InternsInfoService);
    expect(service).toBeTruthy();
  });
});
