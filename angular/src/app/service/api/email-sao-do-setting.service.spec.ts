import { TestBed } from '@angular/core/testing';

import { EmailSaoDoSettingService } from './email-sao-do-setting.service';

describe('EmailSaoDoSettingService', () => {
  beforeEach(() => TestBed.configureTestingModule({}));

  it('should be created', () => {
    const service: EmailSaoDoSettingService = TestBed.get(EmailSaoDoSettingService);
    expect(service).toBeTruthy();
  });
});
