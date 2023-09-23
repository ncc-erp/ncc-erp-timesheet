import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ManageNewsDetailComponent } from './manage-news-detail.component';

describe('ManageNewsDetailComponent', () => {
  let component: ManageNewsDetailComponent;
  let fixture: ComponentFixture<ManageNewsDetailComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ManageNewsDetailComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ManageNewsDetailComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
