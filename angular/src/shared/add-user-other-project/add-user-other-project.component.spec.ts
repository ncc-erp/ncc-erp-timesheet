import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { AddUserOtherProjectComponent } from './add-user-other-project.component';

describe('AddUserOtherProjectComponent', () => {
  let component: AddUserOtherProjectComponent;
  let fixture: ComponentFixture<AddUserOtherProjectComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ AddUserOtherProjectComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(AddUserOtherProjectComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
