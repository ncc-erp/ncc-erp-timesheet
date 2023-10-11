import { TestBed } from '@angular/core/testing';

import { ManageUserForBranchService } from './manage-user-for-branch.service';

describe('ManageUserForBranchService', () => {
  beforeEach(() => TestBed.configureTestingModule({}));

  it('should be created', () => {
    const service: ManageUserForBranchService = TestBed.get(ManageUserForBranchService);
    expect(service).toBeTruthy();
  });
});
