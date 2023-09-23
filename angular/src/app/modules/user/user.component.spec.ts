import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import {  UserSecondComponent } from './user.component';

describe('UserComponent', () => {
  let component: UserSecondComponent;
  let fixture: ComponentFixture<UserSecondComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ UserSecondComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(UserSecondComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
