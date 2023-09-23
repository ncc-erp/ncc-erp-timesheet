import { TestBed } from '@angular/core/testing';

import { MyProfileService } from './my-profile.service';

describe('MyProfileService', () => {
  beforeEach(() => TestBed.configureTestingModule({}));

  it('should be created', () => {
    const service: MyProfileService = TestBed.get(MyProfileService);
    expect(service).toBeTruthy();
  });
});
