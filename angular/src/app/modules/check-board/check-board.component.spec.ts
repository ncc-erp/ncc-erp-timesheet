import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { CheckBoardComponent } from './check-board.component';

describe('CheckBoardComponent', () => {
  let component: CheckBoardComponent;
  let fixture: ComponentFixture<CheckBoardComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ CheckBoardComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(CheckBoardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
