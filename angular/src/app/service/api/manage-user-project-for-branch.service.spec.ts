import { TestBed } from '@angular/core/testing';

import { ManageUserProjectForBranchService } from './manage-user-project-for-branch.service';

describe('ManageUserProjectForBranchService', () => {
  beforeEach(() => TestBed.configureTestingModule({}));

  it('should be created', () => {
    const service: ManageUserProjectForBranchService = TestBed.get(ManageUserProjectForBranchService);
    expect(service).toBeTruthy();
  });
});
