import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ManageDetailComponent } from './manage-detail.component';

describe('ManageDetailComponent', () => {
  let component: ManageDetailComponent;
  let fixture: ComponentFixture<ManageDetailComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ManageDetailComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ManageDetailComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
