import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ShowDetailDialogComponent } from './show-detail-dialog.component';

describe('ShowDetailDialogComponent', () => {
  let component: ShowDetailDialogComponent;
  let fixture: ComponentFixture<ShowDetailDialogComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ShowDetailDialogComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ShowDetailDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
