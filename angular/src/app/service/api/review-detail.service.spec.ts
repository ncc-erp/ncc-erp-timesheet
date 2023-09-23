import { TestBed } from '@angular/core/testing';

import { ReviewDetailService } from './review-detail.service';

describe('ReviewDetailService', () => {
  beforeEach(() => TestBed.configureTestingModule({}));

  it('should be created', () => {
    const service: ReviewDetailService = TestBed.get(ReviewDetailService);
    expect(service).toBeTruthy();
  });
});
