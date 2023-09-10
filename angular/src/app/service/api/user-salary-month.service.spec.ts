import { TestBed } from '@angular/core/testing';

import { UserSalaryMonthService } from './user-salary-month.service';

describe('UserSalaryMonthService', () => {
  beforeEach(() => TestBed.configureTestingModule({}));

  it('should be created', () => {
    const service: UserSalaryMonthService = TestBed.get(UserSalaryMonthService);
    expect(service).toBeTruthy();
  });
});
