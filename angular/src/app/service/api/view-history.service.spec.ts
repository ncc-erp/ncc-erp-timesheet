import { TestBed } from '@angular/core/testing';

import { ViewHistoryService } from './view-history.service';

describe('ViewHistoryService', () => {
  beforeEach(() => TestBed.configureTestingModule({}));

  it('should be created', () => {
    const service: ViewHistoryService = TestBed.get(ViewHistoryService);
    expect(service).toBeTruthy();
  });
});
