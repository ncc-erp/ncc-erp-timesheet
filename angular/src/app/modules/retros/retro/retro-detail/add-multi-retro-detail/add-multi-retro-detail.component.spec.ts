import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { AddMultiRetroDetailComponent } from './add-multi-retro-detail.component';

describe('AddMultiRetroDetailComponent', () => {
  let component: AddMultiRetroDetailComponent;
  let fixture: ComponentFixture<AddMultiRetroDetailComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ AddMultiRetroDetailComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(AddMultiRetroDetailComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
