import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { CreateEditPositionComponent } from './create-edit-position.component';

describe('CreateEditPositionComponent', () => {
  let component: CreateEditPositionComponent;
  let fixture: ComponentFixture<CreateEditPositionComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ CreateEditPositionComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(CreateEditPositionComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
