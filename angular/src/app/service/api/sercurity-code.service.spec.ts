import { TestBed } from '@angular/core/testing';

import { SercurityCodeService } from './sercurity-code.service';

describe('SercurityCodeService', () => {
  beforeEach(() => TestBed.configureTestingModule({}));

  it('should be created', () => {
    const service: SercurityCodeService = TestBed.get(SercurityCodeService);
    expect(service).toBeTruthy();
  });
});
