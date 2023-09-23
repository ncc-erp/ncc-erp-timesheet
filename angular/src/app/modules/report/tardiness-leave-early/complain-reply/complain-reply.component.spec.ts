import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ComplainReplyComponent } from './complain-reply.component';

describe('ComplainReplyComponent', () => {
  let component: ComplainReplyComponent;
  let fixture: ComponentFixture<ComplainReplyComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ComplainReplyComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ComplainReplyComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
