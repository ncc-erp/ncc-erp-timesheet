import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { CreateCheckBoardComponent } from './create-check-board.component';

describe('CreateCheckBoardComponent', () => {
  let component: CreateCheckBoardComponent;
  let fixture: ComponentFixture<CreateCheckBoardComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ CreateCheckBoardComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(CreateCheckBoardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
