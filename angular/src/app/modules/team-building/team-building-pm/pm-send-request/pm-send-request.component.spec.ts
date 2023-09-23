import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { PmSendRequestComponent } from './pm-send-request.component';

describe('PmSendRequestComponent', () => {
  let component: PmSendRequestComponent;
  let fixture: ComponentFixture<PmSendRequestComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ PmSendRequestComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(PmSendRequestComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
