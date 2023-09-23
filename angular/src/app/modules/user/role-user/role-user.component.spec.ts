import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { RoleUserComponent } from './role-user.component';

describe('RoleUserComponent', () => {
  let component: RoleUserComponent;
  let fixture: ComponentFixture<RoleUserComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ RoleUserComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(RoleUserComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
