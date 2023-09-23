import { TestBed } from '@angular/core/testing';

import { UserSalaryService } from './user-salary.service';

describe('UserSalaryService', () => {
  beforeEach(() => TestBed.configureTestingModule({}));

  it('should be created', () => {
    const service: UserSalaryService = TestBed.get(UserSalaryService);
    expect(service).toBeTruthy();
  });
});
