import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ComplainMailComponent } from './complain-mail.component';

describe('ComplainMailComponent', () => {
  let component: ComplainMailComponent;
  let fixture: ComponentFixture<ComplainMailComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ComplainMailComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ComplainMailComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
